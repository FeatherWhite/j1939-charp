using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;

namespace Triumph.J1939
{
    public enum ConnectionMode : byte
    {
        RTS = 0x10,
        CTS = 0x11,
        EOM_ACK = 0x13,
        BAM = 0x20,
        ABORT = 0xFF
    }

    public enum ConnectionAbortReason : byte
    {
        BUSY = 1,       // Already  in  one  or  more  connection  managed  sessions  and  cannot  support another
        RESOURCES = 2,   // System  resources  were  needed  for  another  task  so  this  connection  managed session was terminated
        TIMEOUT = 3,    // A timeout occured

        // 4..250 Reserved by SAE
        CTS_WHILE_DT = 4  // according AUTOSAR: CTS messages received when data transfer is in progress

        // 251..255 Per J1939/71 definitions - but there are none?
    }

    public enum SendBufferState : byte
    {
        WAITING_CTS = 0, // waiting for CTS
        SENDING_IN_CTS = 1, // sending packages (temporary state)
        SENDING_BM = 2, // sending broadcast packages
        WAITING_ACK = 3, // waiting for EOM_ACK
        FINISHED = 4, // finished, remove buffer
        TIMEOUT = 5 // finished, remove buffer
    }

    public enum RecvBufferState : byte
    {
        WAITING_RTS = 0, // waiting for CTS
        RECVING_IN_RTS = 1, // recving packages (temporary state)
        RECVING_BM = 2,
        FINISHED = 3, // finished, remove buffer
        TIMEOUT = 4 // finished, remove buffer
    }

    public class J1939_21
    {
        public delegate bool SendCanFunc(uint arbitrationId, uint channel, byte[] elems);
        public delegate void NotifySubscribersFunc(byte priority, uint pgn, byte sa, byte da, uint timestamp, byte[] data);
        public delegate void LoggerInfoFunc(string message);

        public SendCanFunc SendCan;
        public NotifySubscribersFunc NotifySubscribers;
        public LoggerInfoFunc LogInfo;

        public const int MAX_TP_DT = 1785;
        public uint Channel { get; set; }
        public byte MaxCmdtPackets { get; set; } = 255;
        public uint MinimumTpBamDtInterval { get; set; }
        public uint MinimumTpRtsCtsDtInterval { get; set; }

        public const uint Tr = 200;
        public const uint Th = 500;
        public const uint T1 = 750;
        public const uint T2 = 1250;
        public const uint T3 = 1250;
        public const uint T4 = 1250;
        public const uint Tb = 50;

        public Dictionary<ushort, ReceiveModel> ReceiveBuffer = new Dictionary<ushort, ReceiveModel>();
        public Dictionary<ushort, SendModel> SendBuffer = new Dictionary<ushort, SendModel>();
        public List<ControllerApplication> ControllerApplications = new List<ControllerApplication>();
        public J1939TpLink link = new J1939TpLink();

        public bool SendPGN(byte dataPage, byte pduFormat, byte pduSpecific,
            byte priority, byte sa, byte[] data)
        {
            var pgn = new ParameterGroupNumber(dataPage, pduFormat, pduSpecific);
            link.SendBuf = data;
            link.SendBufSize = (ushort)data.Length;
            link.RecvBufSize = 0;
            link.RecvBuf = new byte[0];
            if (data.Length <= 8)
            {
                var mid = new MessageId(priority, pgn.Value, sa);
                SendCan(mid.CanId, Channel, data);
            }
            else
            {
                byte da = 0;
                if (pduSpecific == ParameterGroupNumber.GLOBAL
                    || new ParameterGroupNumber(0, pduFormat, pduSpecific).IsPDU2Format())
                {
                    da = ParameterGroupNumber.GLOBAL;
                }
                else
                {
                    da = pduSpecific;
                }
                ushort bufferHash = BufferHash(sa, da);
                if (SendBuffer.ContainsKey(bufferHash))
                {
                    return false;
                }
                int messageSize = data.Length;
                int numPackets = messageSize / 7;
                if (messageSize % 7 != 0)
                {
                    numPackets += 1;
                }
                if (da == ParameterGroupNumber.GLOBAL)
                {
                    SendTPBAM(sa, priority, pgn.Value, (ushort)messageSize, (byte)numPackets);
                    SendBuffer[bufferHash] = new SendModel()
                    {
                        pgn = pgn.Value,
                        priority = priority,
                        messageSize = (ushort)messageSize,
                        numPackages = (byte)numPackets,
                        data = data,
                        state = SendBufferState.SENDING_BM,
                        deadline = GetTimestamp() + (MinimumTpBamDtInterval == 0 ? Tb : MinimumTpBamDtInterval),
                        sa = sa,
                        da = ParameterGroupNumber.GLOBAL,
                        nextPacketToSend = 0
                    };
                    link.SendStatus = SendBufferState.SENDING_BM;
                }
                else
                {
                    pgn.pduSpecific = 0;
                    SendBuffer[bufferHash] = new SendModel()
                    {
                        pgn = pgn.Value,
                        priority = priority,
                        messageSize = (ushort)messageSize,
                        numPackages = (byte)numPackets,
                        data = data,
                        state = SendBufferState.WAITING_CTS,
                        deadline = GetTimestamp() + T3,
                        sa = sa,
                        da = pduSpecific,
                        nextPacketToSend = 0,
                        nextWaitOnCts = 0
                    };
                    //SendTPRTS(sa, pduSpecific, priority, pgn.Value, (ushort)messageSize,
                    //    (byte)numPackets, (byte)Math.Min(MaxCmdtPackets, numPackets));
                    link.SendStatus = SendBufferState.WAITING_CTS;
                    link.RecvStatus = RecvBufferState.WAITING_RTS;
                    SendTPRTS(sa, pduSpecific, priority, pgn.Value, (ushort)messageSize,
                        (byte)numPackets, MaxCmdtPackets);
                }
                //启动工作线程
            }
            return true;
        }

        public uint JobThread(uint now)
        {
            uint nextWakeup = now + 5;
            for (int i = 0; i < ReceiveBuffer.Count; i++ )
            {
                var buf = ReceiveBuffer.ElementAt(i);
                if (buf.Value.deadline != 0)
                {
                    if (buf.Value.deadline > now)
                    {
                        if (nextWakeup >= buf.Value.deadline)
                        {
                            nextWakeup = buf.Value.deadline;
                        }
                    }
                    else
                    {
                        if(LogInfo != null)
                        {
                            LogInfo($"Deadline reached for rcv_buffer src 0x{buf.Value.sa.ToString("X")} " +
                            $"dst 0x{buf.Value.da.ToString("X")}");
                        }                        
                        if (buf.Value.da != ParameterGroupNumber.GLOBAL)
                        {
                            SendTPAbort(buf.Value.sa, buf.Value.da, (byte)ConnectionAbortReason.TIMEOUT, buf.Value.pgn);
                        }
                        link.RecvStatus = RecvBufferState.TIMEOUT;
                        ReceiveBuffer.Remove(buf.Key);
                    }
                }
            }

            for(int j = 0;j < SendBuffer.Count; j++)
            {
                var buf = SendBuffer.ElementAt(j);
                if (buf.Value.deadline != 0)
                {
                    if (buf.Value.deadline > now)
                    {
                        if (nextWakeup > buf.Value.deadline)
                        {
                            nextWakeup = buf.Value.deadline;
                        }
                    }
                    else
                    {
                        if (buf.Value.state == SendBufferState.WAITING_CTS)
                        {
                            if (LogInfo != null)
                            {
                                LogInfo($"Deadline WAITING_CTS reached for snd_buffer src 0x{buf.Value.sa.ToString("X")} " +
                                    $"dst 0x{buf.Value.da.ToString("X")}");
                            }
                            
                            SendTPAbort(buf.Value.sa, buf.Value.da, (byte)ConnectionAbortReason.TIMEOUT, buf.Value.pgn);
                            link.SendStatus = SendBufferState.TIMEOUT;
                            SendBuffer.Remove(buf.Key);
                        }
                        else if (buf.Value.state == SendBufferState.SENDING_IN_CTS)
                        {
                            //while (buf.Value.nextPacketToSend < buf.Value.numPackages)
                            {
                                byte package = buf.Value.nextPacketToSend;
                                ushort offset = (ushort)(package * 7);
                                int restOfDataLength = buf.Value.data.Length - offset;
                                //Console.WriteLine($"Data Length:{buf.Value.data.Length},Offset:{offset}");
                                byte[] data;
                                if (restOfDataLength > 8)
                                {
                                    data = new byte[8];
                                    Array.Copy(buf.Value.data, offset, data, 1, 7);
                                }
                                else
                                {
                                    byte[] temp = new byte[8];
                                    Array.Copy(buf.Value.data, offset, temp, 1, restOfDataLength);
                                    data = temp;
                                    for (int i = restOfDataLength + 1; i < 8; i++)
                                    {
                                        data[i] = 0xFF;
                                    }
                                }
                                data[0] = (byte)(package + 1);

                                buf.Value.nextPacketToSend += 1;
                                bool shouldBreak = false;
                                if (buf.Value.nextWaitOnCts == package)
                                {
                                    buf.Value.state = SendBufferState.WAITING_CTS;
                                    link.SendStatus = SendBufferState.WAITING_ACK;                                   
                                    buf.Value.deadline = GetTimestamp() + T3;
                                    shouldBreak = true;
                                }
                                else if (MinimumTpRtsCtsDtInterval > 0)
                                {
                                    buf.Value.deadline = GetTimestamp() + MinimumTpRtsCtsDtInterval;
                                    shouldBreak = true;
                                }
                                SendTP_DT(buf.Value.sa, buf.Value.da, data);
                                if (shouldBreak)
                                {
                                    break;
                                }
                            }

                            if (nextWakeup > buf.Value.deadline)
                            {
                                nextWakeup = buf.Value.deadline;
                            }
                        }
                        else if (buf.Value.state == SendBufferState.SENDING_BM)
                        {
                            ushort offset = (ushort)(buf.Value.nextPacketToSend * 7);
                            byte[] data = new byte[buf.Value.data.Length - offset];
                            if (data.Length > 8)
                            {
                                data = new byte[8];
                                Array.Copy(buf.Value.data, offset, data, 1, 7);
                            }
                            else
                            {
                                byte[] temp = new byte[8];
                                Array.Copy(buf.Value.data, offset, temp, 1, data.Length);
                                data = temp;
                                for (int i = 8 - data.Length; i < 8; i++)
                                {
                                    data[i] = 0xFF;
                                }
                            }
                            data[0] = (byte)(buf.Value.nextPacketToSend + 1);
                            buf.Value.nextPacketToSend += 1;
                            if (buf.Value.nextPacketToSend < buf.Value.numPackages)
                            {
                                buf.Value.deadline = GetTimestamp() + MinimumTpBamDtInterval;
                                if (nextWakeup > buf.Value.deadline)
                                {
                                    nextWakeup = buf.Value.deadline;
                                }
                            }
                            else
                            {
                                SendBuffer.Remove(buf.Key);
                            }
                            SendTP_DT(buf.Value.sa, buf.Value.da, data);
                        }
                        else if (buf.Value.state == SendBufferState.FINISHED)
                        {
                            SendBuffer.Remove(buf.Key);
                        }
                        else
                        {
                            if(LogInfo != null)
                            {
                                LogInfo("Unknown SendBufferState: " + buf.Value.state);
                            }
                            SendBuffer.Remove(buf.Key);
                        }
                    }
                }
            }
            return nextWakeup;
        }

        public bool SendTP_DT(byte sa, byte da, byte[] data)
        {
            var pgn = new ParameterGroupNumber(0, 0xEB, da);
            var mid = new MessageId(7, pgn.Value, sa);
            return SendCan(mid.CanId, Channel, data);
        }

        public bool SendTPAbort(byte sa, byte da, byte reason, uint pgnVal)
        {
            var pgn = new ParameterGroupNumber(0, 0xEC, da);
            var mid = new MessageId(7, pgn.Value, sa);
            byte[] data = new byte[]
            {
                (byte)ConnectionMode.ABORT,
                reason,
                0xFF,
                0xFF,
                0xFF,
                (byte)(pgnVal & 0xFF),
                (byte)((pgnVal >> 8) & 0xFF),
                (byte)((pgnVal >> 16) & 0xFF),
            };
            return SendCan(mid.CanId, Channel, data);
        }

        public bool SendTPCTS(byte sa, byte da, byte numPackets, byte nextPacket, uint pgnVal)
        {
            var pgn = new ParameterGroupNumber(0, 0xEC, da);
            var mid = new MessageId(7, pgn.Value, sa);
            byte[] data = new byte[]
            {
                (byte)ConnectionMode.CTS,
                numPackets,
                nextPacket,
                0xFF,
                0xFF,
                (byte)(pgnVal & 0xFF),
                (byte)((pgnVal >> 8) & 0xFF),
                (byte)((pgnVal >> 16) & 0xFF),
            };
            return SendCan(mid.CanId, Channel, data);
        }

        public bool SendTPEOMAck(byte sa, byte da, ushort messageSize, byte numPackets, uint pgnVal)
        {
            var pgn = new ParameterGroupNumber(0, 0xEC, da);
            var mid = new MessageId(7, pgn.Value, sa);
            byte[] data = new byte[]
            {
                (byte)ConnectionMode.EOM_ACK,
                (byte)(messageSize & 0xFF),
                (byte)((messageSize >> 8) & 0xFF),
                numPackets,
                0xFF,
                (byte)(pgnVal & 0xFF),
                (byte)((pgnVal >> 8) & 0xFF),
                (byte)((pgnVal >> 16) & 0xFF),
            };
            return SendCan(mid.CanId, Channel, data);
        }

        public bool SendTPRTS(byte sa, byte da, byte priority, uint pgnVal,
            ushort messageSize, byte numPackets, byte maxCmdtPackets)
        {
            var pgn = new ParameterGroupNumber(0, 0xEC, da);
            var mid = new MessageId(priority, pgn.Value, sa);
            //link.RecvCanId = new MessageId(priority, new ParameterGroupNumber(0, 0xEC, sa).Value, da).CanId;
            byte[] data = new byte[]
            {
                (byte)ConnectionMode.RTS,
                Convert.ToByte(messageSize & 0xFF),
                Convert.ToByte((messageSize >> 8) & 0xFF),
                numPackets,
                maxCmdtPackets,
                (byte)(pgnVal & 0xFF),
                (byte)((pgnVal >> 8) & 0xFF),
                (byte)((pgnVal >> 16) & 0xFF),
            };
            return SendCan(mid.CanId, Channel, data);
        }

        public bool SendTPBAM(byte sa, byte priority, uint pgnVal,
            ushort messageSize, byte numPackets)
        {
            var pgn = new ParameterGroupNumber(0, 0xEC, ParameterGroupNumber.GLOBAL);
            var mid = new MessageId(priority, pgn.Value, sa);
            byte[] data = new byte[]
            {
                (byte)ConnectionMode.BAM,
                Convert.ToByte(messageSize & 0xFF),
                Convert.ToByte((messageSize >> 8) & 0xFF),
                numPackets,
                0xFF,
                (byte)(pgnVal & 0xFF),
                (byte)((pgnVal >> 8) & 0xFF),
                (byte)((pgnVal >> 16) & 0xFF),
            };
            return SendCan(mid.CanId, Channel, data);
        }

        public void ProcessTPCM(MessageId mid, byte da, byte[] data, uint timestamp)
        {
            /*Processes a Transport Protocol Connection Management (TP.CM) message

            :param j1939.MessageId mid:
                A MessageId object holding the information extracted from the can_id.
            :param int dest_address:
                The destination address of the message
            :param bytearray data:
                The data contained in the can-message.
            :param float timestamp:
                The timestamp the message was received(mostly) in fractions of Epoch - Seconds.
            */

            byte controlByte = data[0];
            uint pgn = data[5] | (uint)data[6] << 8 | (uint)data[7] << 16;
            byte sa = mid.SourceAddress;

            if (controlByte == (byte)ConnectionMode.RTS)
            {
                ushort messageSize = (ushort)(data[1] | (data[2] << 8));
                byte numPackages = data[3];
                byte maxNumPackages = data[4];
                ushort bufferHash = BufferHash(sa, da);
                if (ReceiveBuffer.ContainsKey(bufferHash))
                {
                    SendTPAbort(da, sa, (byte)ConnectionAbortReason.BUSY, pgn);
                    return;
                }
                maxNumPackages = Math.Min(maxNumPackages, numPackages);
                ReceiveBuffer[bufferHash] = new ReceiveModel()
                {
                    pgn = pgn,
                    messageSize = messageSize,
                    numPackages = numPackages,
                    nextPacket = Math.Min(MaxCmdtPackets, maxNumPackages),
                    maxCmdtPackages = MaxCmdtPackets,
                    numPackagesMaxRec = Math.Min(MaxCmdtPackets, maxNumPackages),
                    //data = default,
                    deadline = GetTimestamp() + T2,
                    sa = sa,
                    da = da
                };
                link.RecvStatus = RecvBufferState.RECVING_IN_RTS;
                SendTPCTS(da, sa, ReceiveBuffer[bufferHash].numPackagesMaxRec, 1, pgn);
                //工作线程启动
            }
            else if (controlByte == (byte)ConnectionMode.CTS)
            {
                //link.RecvCanId = new MessageId(7, new ParameterGroupNumber(0, 0xEB, sa).Value, da).CanId;
                byte numPackages = data[1];
                byte nextPackageNumber = (byte)(data[2] - 1);
                ushort bufferHash = BufferHash(da, sa);
                if (!SendBuffer.ContainsKey(bufferHash))
                {
                    SendTPAbort(da, sa, (byte)ConnectionAbortReason.RESOURCES, pgn);
                    return;
                }
                if (numPackages == 0)
                {
                    SendBuffer[bufferHash].deadline = GetTimestamp() + Th;
                    //工作线程启动
                    return;
                }
                byte numPackagesAll = SendBuffer[bufferHash].numPackages;
                if (numPackages > numPackagesAll)
                {
                    if(LogInfo != null)
                    {
                        LogInfo($"CTS: Allowed more packets {numPackages} than complete transmission {numPackagesAll}");
                        numPackages = numPackagesAll;
                    }
                }
                if (nextPackageNumber + numPackages > numPackagesAll)
                {
                    if( LogInfo != null)
                    {
                        LogInfo($"CTS: Allowed more packets {numPackages} " +
                        $"than needed to complete transmission {numPackagesAll - nextPackageNumber}");
                    }
                    numPackages = Convert.ToByte(numPackagesAll - nextPackageNumber);
                }
                //此处类型待定
                SendBuffer[bufferHash].nextWaitOnCts =
                    Convert.ToUInt32(SendBuffer[bufferHash].nextPacketToSend + numPackages - 1);
                SendBuffer[bufferHash].state = SendBufferState.SENDING_IN_CTS;
                SendBuffer[bufferHash].deadline = GetTimestamp();
                //工作线程启动
            }
            else if (controlByte == (byte)ConnectionMode.EOM_ACK)
            {
                ushort bufferHash = BufferHash(da, sa);
                if (!SendBuffer.ContainsKey(bufferHash))
                {
                    SendTPAbort(da, sa, (byte)ConnectionAbortReason.RESOURCES, pgn);
                    return;
                }
                //NotifySubscribers(mid.Priority, pgn, mid.SourceAddress, da, timestamp, data);
                SendBuffer[bufferHash].state = SendBufferState.FINISHED;
                link.SendStatus = SendBufferState.FINISHED;
                SendBuffer[bufferHash].deadline = GetTimestamp();
                //工作线程启动
            }
            else if (controlByte == (byte)ConnectionMode.BAM)
            {
                ushort messageSize = Convert.ToUInt16(data[1] | (data[2] << 8));
                byte numPackages = data[3];
                ushort bufferHash = BufferHash(sa, da);
                if (ReceiveBuffer.ContainsKey(bufferHash))
                {
                    ReceiveBuffer.Remove(bufferHash);
                    //工作线程启动
                }
                ReceiveBuffer[bufferHash] = new ReceiveModel()
                {
                    pgn = pgn,
                    messageSize = messageSize,
                    numPackages = numPackages,
                    nextPacket = 1,
                    maxCmdtPackages = MaxCmdtPackets,
                    //data = default,
                    sa = sa,
                    da = da
                };
                link.RecvStatus = RecvBufferState.RECVING_BM;
                //工作线程启动
            }
            else if (controlByte == (byte)ConnectionMode.ABORT)
            {
                ushort bufferHash = BufferHash(da, sa);
                if (SendBuffer.ContainsKey(bufferHash)
                    && SendBuffer[bufferHash].state == SendBufferState.WAITING_CTS)
                {
                    link.SendStatus = SendBufferState.FINISHED;
                    SendBuffer[bufferHash].state = SendBufferState.FINISHED;
                    SendBuffer[bufferHash].deadline = GetTimestamp();
                }
            }
            else
            {
                if (LogInfo != null)
                {
                    LogInfo($"Received TP.CM with unknown control_byte {controlByte}");

                }
            }
        }

        public void ProcessTPDT(MessageId mid, byte da, byte[] data, uint timestamp)
        {
            byte sequenceNumber = data[0];
            byte sa = mid.SourceAddress;
            ushort bufferHash = BufferHash(sa, da);
            if (!ReceiveBuffer.ContainsKey(bufferHash))
            {
                return;
            }
            Array.Copy(data, 1, ReceiveBuffer[bufferHash].data, (sequenceNumber - 1) * 7, data.Length - 1);
            if ((sequenceNumber * 7) >= ReceiveBuffer[bufferHash].messageSize)
            {
                ushort messageSize = ReceiveBuffer[bufferHash].messageSize;
                if (LogInfo != null)
                {
                    LogInfo($"finished RCV of PGN {ReceiveBuffer[bufferHash].pgn} " +
                    $"with size {ReceiveBuffer[bufferHash].messageSize}");
                }
                
                Array.Copy(ReceiveBuffer[bufferHash].data, ReceiveBuffer[bufferHash].data, messageSize);
                if (da != ParameterGroupNumber.GLOBAL)
                {
                    SendTPEOMAck(da, sa, ReceiveBuffer[bufferHash].messageSize,
                        ReceiveBuffer[bufferHash].numPackages, ReceiveBuffer[bufferHash].pgn);
                    link.RecvBuf = ReceiveBuffer[bufferHash].data;
                    link.RecvBufSize = messageSize;
                    link.RecvStatus = RecvBufferState.FINISHED;
                    //NotifySubscribers(mid.Priority, ReceiveBuffer[bufferHash].pgn, sa, da, timestamp, ReceiveBuffer[bufferHash].data);
                    ReceiveBuffer.Remove(bufferHash);
                    //工作线程启动
                    return;
                }
            }
            if (da != ParameterGroupNumber.GLOBAL && sequenceNumber >= ReceiveBuffer[bufferHash].nextPacket)
            {
                byte numberOfPacketsThatCanBeSent =
                    (byte)Math.Min(ReceiveBuffer[bufferHash].numPackagesMaxRec,
                    ReceiveBuffer[bufferHash].numPackages - ReceiveBuffer[bufferHash].nextPacket);
                byte nextPacketToBeSent = Convert.ToByte(ReceiveBuffer[bufferHash].nextPacket + 1);
                SendTPCTS(da, sa, numberOfPacketsThatCanBeSent, nextPacketToBeSent, ReceiveBuffer[bufferHash].pgn);
                ReceiveBuffer[bufferHash].nextPacket =
                    (byte)Math.Min(ReceiveBuffer[bufferHash].nextPacket + ReceiveBuffer[bufferHash].numPackagesMaxRec
                    , ReceiveBuffer[bufferHash].numPackages);
                ReceiveBuffer[bufferHash].deadline = GetTimestamp() + T2;
                //启动工作线程
                return;
            }
            ReceiveBuffer[bufferHash].deadline = GetTimestamp() + T1;
            //启动工作线程
        }

        public ushort BufferHash(byte sa, byte da)
        {
            return (ushort)(((sa & 0xFF) << 8) | (da & 0xFF));
        }
        uint firstRecvNothingTimestamp = 0;
        public void Notify(uint canId, byte[] data, uint timestamp)
        {
            MessageId mid = new MessageId();
            mid.CanId = canId;
            ParameterGroupNumber pgn = new ParameterGroupNumber();
            pgn.FromMessageId(mid);
            if (pgn.IsPDU2Format())
            {
                NotifySubscribers(mid.Priority, pgn.Value, mid.SourceAddress, 
                    ParameterGroupNumber.GLOBAL, timestamp, data);
                return;
            }
            uint pgnValue = pgn.Value & 0x1FF00;
            byte da = pgn.pduSpecific;
            if(da != ParameterGroupNumber.GLOBAL)
            {

            }
            if(pgnValue == (uint)PGNCode.REQUEST)
            {
                foreach (var ca in ControllerApplications)
                {
                    if (ca.MessageAcceptable(da))
                    {
                        ca.ProcessRequest(mid, da, data, timestamp);
                    }
                }
            }
            else if (pgnValue == (uint)PGNCode.TP_CM)
            {
                ProcessTPCM(mid, da, data, timestamp);
            }
            else if (pgnValue == (uint)PGNCode.TP_DT)
            {
                ProcessTPDT(mid, da, data, timestamp);
            }
            else if(data.Count() == 0)
            {
                if (firstRecvNothingTimestamp == 0)
                {
                    firstRecvNothingTimestamp = GetTimestamp();
                }
                if(GetTimestamp() - firstRecvNothingTimestamp > 1000)
                {
                    firstRecvNothingTimestamp = 0;
                    link.RecvStatus = RecvBufferState.TIMEOUT;
                }
                return;
            }
            else
            {
                NotifySubscribers(mid.Priority, pgnValue, mid.SourceAddress, da, timestamp, data);
            }
            firstRecvNothingTimestamp = 0;
        }

        public uint GetTimestamp()
        {
            return (uint)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % uint.MaxValue);
        }
    }

    public class ReceiveModel
    {
        public uint pgn = 0;
        public ushort messageSize = 0;
        public byte numPackages = 0;
        public byte nextPacket = 0;
        public byte maxCmdtPackages = 0;
        public byte numPackagesMaxRec = 0;
        public byte[] data = new byte[J1939_21.MAX_TP_DT];
        public uint deadline = 0;
        public byte sa = 0;
        public byte da = 0;
    }

    public class SendModel
    {
        public uint pgn = 0;
        public byte priority;
        public ushort messageSize = 0;
        public byte numPackages = 0;
        public byte[] data = new byte[J1939_21.MAX_TP_DT];
        public SendBufferState state;
        public uint deadline = 0;
        public byte sa = 0;
        public byte da = 0;
        public byte nextPacketToSend = 0;
        public uint nextWaitOnCts = 0;
    }
}