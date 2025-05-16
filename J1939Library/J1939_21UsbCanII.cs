using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ZLG.CAN;

namespace Triumph.J1939
{
    public class J1939_21UsbCanII : IJ1939Tp
    {
        public J1939TpLink link { get; set; } = new J1939TpLink();
        public byte MaxCmdtPackets { get; set; }
        public byte SourceAddress { get; set; }
        public byte DestAddress { get; set; }
        private USBCanIICommunication can;
        private J1939_21 j1939_21;
        public MessageListener Listener { get; set; }
        private uint[] canIds;
        public J1939_21UsbCanII(USBCanIICommunication can)
        {
            this.can = can;
            j1939_21 = new J1939_21();
            j1939_21.SendCan += can.Send;
            j1939_21.link = link;
            canIds = [new MessageId(7, new ParameterGroupNumber(0, 0xEC, SourceAddress).Value, DestAddress).CanId,
                new MessageId(7, new ParameterGroupNumber(0, 0xEB, SourceAddress).Value, DestAddress).CanId];
            //j1939_21.LoggerInfo += 
        }
        public J1939_21UsbCanII()
        {
            j1939_21 = new J1939_21();
            j1939_21.SendCan += SendCan;
            j1939_21.link = link;
            j1939_21.LoggerInfo += LoggerInfo;
            canIds = [new MessageId(7, new ParameterGroupNumber(0, 0xEC, SourceAddress).Value, DestAddress).CanId,
                new MessageId(7, new ParameterGroupNumber(0, 0xEB, SourceAddress).Value, DestAddress).CanId];
        }
        public void LoggerInfo(string message)
        {
            Console.WriteLine(message);
        }
        ZCAN_Receive_Data[] query;

        private bool Receive(uint channel, uint[] canIds)
        {
            bool ret = false;
            var gets = can.Receive<ZCAN_Receive_Data[]>(channel);
            foreach (var canId in canIds)
            {
                var query = Get(canId, gets);
                if (query.Count() <= 0)
                {
                    ret = false;
                }
                else
                {
                    ret = true;
                }
            }
            return ret;
        }

        public void Receive()
        {

        }

        public void Poll()
        {
            if (Receive(j1939_21.Channel, canIds))
            {
                foreach (var q in query)
                {
                    Notify(q.frame.can_id, q.frame.data, (uint)q.timestamp);
                    JobThread();
                }
            }
            else
            {
                JobThread();
            }
        }

        private ZCAN_Receive_Data[] Get(uint id, ZCAN_Receive_Data[] array)
        {
            ZCAN_Receive_Data[] ret = new ZCAN_Receive_Data[0];
            if (array != null)
            {
                if (array.Length > 0)
                {
                    var query = array.
                        Where(data => GetId(data.frame.can_id) == id);
                    ret = query.ToArray();
                    int index = 0;
                    foreach (var q in query)
                    {
                        ret[index].frame.can_id = GetId(q.frame.can_id);
                        Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} " +
                            $"{can.CanPara.deviceIndex} CanId:0x{q.frame.can_id.ToString("X")},通道:{link.Channel} 接收:{BitConverter.ToString(q.frame.data)}");
                        index++;
                    }
                }
            }
            return ret;
        }

        private uint GetId(uint canid)
        {
            return canid & 0x1FFFFFFFU;
        }

        public void Notify(uint canId, byte[] data, uint timestamp)
        {
            j1939_21.Notify(canId, data, timestamp);
        }
        public uint JobThread()
        {
            return j1939_21.JobThread(j1939_21.GetTimestamp());
        }

        public bool Send(byte dataPage, byte pduFormat, 
            byte priority, byte[] data)
        {
            //return j1939_21.SendPGN(dataPage, pduFormat, pduSpecific, priority, sa, data);
            return j1939_21.SendPGN(dataPage, pduFormat, DestAddress, priority, SourceAddress, data);
        }

        public bool SendCan(uint arbitrationId, uint channel, byte[] data)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} " +
                    $"CanId:0x{arbitrationId.ToString("X")},通道:{channel} 发送:{BitConverter.ToString(data)}");
            return true;
        }
    }
    public class MessageListener
    {
        private J1939_21UsbCanII ecu;
        public bool Stopped { get; private set; }
        public MessageListener(J1939_21UsbCanII ecu)
        {
            this.ecu = ecu;
            this.Stopped = false;
        }
        public void OnMessageReceived(ZCAN_Receive_Data msg)
        {
            if (!Stopped)
            {
                ecu.Notify(msg.frame.can_id, msg.frame.data, (uint)msg.timestamp);
            }
        }
        public void Stop()
        {
            Stopped = true;
        }
    }

    public class J1939TpLink
    {
        public uint Channel { get; set; }
        public byte[] SendBuf { get; set; }
        public ushort SendBufSize { get; set; }
        public SendBufferState SendStatus { get; set; }
        public byte[] RecvBuf { get; set; }
        public uint RecvCanId { get; set; }
        public ushort RecvBufSize { get; set; }
        public RecvBufferState RecvStatus { get; set; }
    }
}
