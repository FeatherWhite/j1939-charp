using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace J1939Library
{
    public class J1939TP
    {
        public delegate bool SendCanFunc(uint arbitrationId, uint channel, byte[] package);

        public SendCanFunc SendCan { get; set; }
        public uint Channel { get; set; }
        private Link link = new Link();

        public void ReadDataTransfer(byte SA, byte[] data)
        {
            link.ReceiveTP_DT.SequenceNumber = data[0];
            link.ReceiveTP_DT.FromEcuAddress = SA;
            byte index = Convert.ToByte(data[0] - 1);
            for (byte i = 0; i < 8; i++)
            {
                //For every package, we send 7 bytes of data where the first byte data[0] is the sequence number
                link.ReceiveTP_DT.Data[index * 7 + i - 1] = data[i];
            }
            //Check if we have completed our message - Return = Not completed
            if ((link.ReceiveTP_CM.NumberOfPackagesBeingTransmitted != link.ReceiveTP_DT.SequenceNumber) || 
                (link.ReceiveTP_CM.NumberOfPackagesBeingTransmitted == 0))
            {
                if(link.ReceiveTP_CM.ControlByte == (byte)ControlByteCodes.TP_CM_RTS)
                {
                    //Send new CTS
                    link.SendTP_CM.ControlByte = (byte)ControlByteCodes.TP_CM_CTS;
                    link.SendTP_CM.TotalNumberOfPackagesTransmitted++;
                    link.SendTP_CM.NextPacketNumberTransmitted++;
                    SendConnectionManagement(SA);
                }
                return;
            }
            //Our message are complete - Build it and call it complete_data[total_message_size]
            uint pgn = link.ReceiveTP_CM.PGNOfThePacketedMessage;
            ushort totalMessageSize = link.ReceiveTP_CM.TotalMessageSizeBeingTransmitted;
            byte[] completeData = new byte[Constant.MAX_TP_DT];
            ushort insertedBytes = 0;
            for (byte i = 0; i < link.ReceiveTP_DT.SequenceNumber; i++)
            {
                for (byte j = 0; j < 7; j++)
                {
                    if (insertedBytes < totalMessageSize)
                    {
                        completeData[insertedBytes++] = link.ReceiveTP_DT.Data[i * 7 + j];
                    }
                }
            }
            if(link.ReceiveTP_CM.ControlByte == (byte)ControlByteCodes.TP_CM_RTS)
            {
                link.SendTP_CM.ControlByte = (byte)ControlByteCodes.TP_CM_EndOfMsgACK;
                link.SendTP_CM.TotalNumberOfBytesReceived = link.ReceiveTP_CM.TotalMessageSizeBeingTransmitted;
                link.SendTP_CM.TotalNumberOfPackagesReceived = link.ReceiveTP_DT.SequenceNumber;
                SendConnectionManagement(SA);
            }
            switch (pgn)
            {
                default:
                    break;
            }
            link.ReceiveTP_DT = new TP_DT();
            link.ReceiveTP_CM = new TP_CM();
        }

        public void ReadConnectionManagement(byte SA, byte[] data)
        {
            //Read the control byte
            link.ReceiveTP_CM.ControlByte = data[0];
            //PGN
            link.ReceiveTP_CM.PGNOfThePacketedMessage = Convert.ToUInt32((data[7] << 16) | (data[6] << 8) | data[5]);
            //Source address
            link.ReceiveTP_CM.FromEcuAddress = SA;
            switch (data[0])
            {
                case (byte)ControlByteCodes.TP_CM_RTS:
                    //Set the RTS values
                    link.ReceiveTP_CM.TotalMessageSizeBeingTransmitted = Convert.ToUInt16((data[2] << 8) | data[1]);
                    link.ReceiveTP_CM.NumberOfPackagesBeingTransmitted = Convert.ToByte(data[3]);
                    //Send CTS
                    link.SendTP_CM.ControlByte = (byte)ControlByteCodes.TP_CM_CTS;
                    link.SendTP_CM.TotalNumberOfPackagesTransmitted = 0;
                    link.SendTP_CM.NextPacketNumberTransmitted = 0;
                    link.SendTP_CM.PGNOfThePacketedMessage = Convert.ToUInt32((data[7] << 16) | (data[6] << 8) | data[5]);
                    SendConnectionManagement(SA);
                    break;
                case (byte)ControlByteCodes.TP_CM_CTS:
                    link.ReceiveTP_CM.TotalNumberOfPackagesTransmitted = data[1];
                    link.ReceiveTP_CM.NextPacketNumberTransmitted = data[2];
                    SendDataTransfer(SA);
                    break;
                case (byte)ControlByteCodes.TP_CM_BAM:
                    link.ReceiveTP_CM.TotalMessageSizeBeingTransmitted = Convert.ToUInt16((data[2] << 8) | data[1]);
                    link.ReceiveTP_CM.NumberOfPackagesBeingTransmitted = Convert.ToByte(data[3]);
                    SendDataTransfer(SA);
                    break;
                case (byte)ControlByteCodes.TP_CM_EndOfMsgACK:
                    link.ReceiveTP_CM.TotalNumberOfBytesReceived = Convert.ToUInt16((data[2] << 8) | data[1]);
                    link.ReceiveTP_CM.TotalNumberOfPackagesReceived = data[3];
                break;
            }
        }

        public J1939StatusCode SendConnectionManagement(byte DA)
        {
            uint ID = Convert.ToUInt32((0x1CEC << 16) | (DA << 8) | link.SendInfo.Address);
            byte[] data = new byte[8];
            data[0] = link.SendTP_CM.ControlByte;
            switch ((ControlByteCodes)data[0])
            {
                case ControlByteCodes.TP_CM_RTS:
                    data[1] = (byte)link.SendTP_CM.TotalMessageSizeBeingTransmitted;
                    data[2] = (byte)(link.SendTP_CM.TotalMessageSizeBeingTransmitted >> 8);
                    data[3] = link.SendTP_CM.NumberOfPackagesBeingTransmitted;
                    data[4] = 0xFF;
                    break;
                case ControlByteCodes.TP_CM_CTS:
                    data[1] = link.SendTP_CM.TotalNumberOfPackagesTransmitted;
                    data[2] = link.SendTP_CM.NextPacketNumberTransmitted;
                    data[3] = 0xFF;
                    data[4] = 0xFF;
                    break;
                case ControlByteCodes.TP_CM_BAM:
                    data[1] = (byte)link.SendTP_CM.TotalMessageSizeBeingTransmitted;
                    data[2] = (byte)(link.SendTP_CM.TotalMessageSizeBeingTransmitted >> 8);
                    data[3] = link.SendTP_CM.NumberOfPackagesBeingTransmitted;
                    data[4] = 0xFF;
                    break;
                case ControlByteCodes.TP_CM_EndOfMsgACK:
                    data[1] = (byte)link.SendTP_CM.TotalNumberOfBytesReceived;
                    data[2] = (byte)(link.SendTP_CM.TotalNumberOfBytesReceived >> 8);
                    data[3] = link.SendTP_CM.TotalNumberOfPackagesReceived;
                    data[4] = 0xFF;
                    break;
            }
            data[5] = (byte)link.SendTP_CM.PGNOfThePacketedMessage;
            data[6] = (byte)(link.SendTP_CM.PGNOfThePacketedMessage >> 8);
            data[7] = (byte)(link.SendTP_CM.PGNOfThePacketedMessage >> 16);
            bool isSuccess = SendCan(ID, Channel, data);
            J1939StatusCode status = J1939StatusCode.SendOK;
            if (!isSuccess)
            {
                status = J1939StatusCode.SendError;
            }
            return status;
        }

        public J1939StatusCode SendDataTransfer(byte DA)
        {
            uint ID = Convert.ToUInt32((0x1CEB << 16) | (DA << 8) | link.SendInfo.Address);
            byte[] package = new byte[8];
            ushort bytesSent = 0;
            J1939StatusCode status = J1939StatusCode.SendOK;
            switch ((ControlByteCodes)link.ReceiveTP_CM.ControlByte)
            {
                case ControlByteCodes.TP_CM_BAM:
                    {
                        for (byte i = 1; i <= link.SendTP_CM.NumberOfPackagesBeingTransmitted; i++)
                        {
                            package[0] = i;
                            for (byte j = 0; j < 7; j++)
                            {
                                if (bytesSent < link.SendTP_CM.TotalMessageSizeBeingTransmitted)
                                {
                                    package[j + 1] = link.SendTP_DT.Data[bytesSent];
                                }
                                else
                                {
                                    package[j + 1] = 0xFF;
                                }
                            }
                        }
                        bool isSuccess = SendCan(ID, Channel, package);
                        Thread.Sleep(100);
                        if (!isSuccess)
                        {
                            status = J1939StatusCode.SendError;
                            break;
                        }
                        break;
                    }

                case ControlByteCodes.TP_CM_CTS:
                    {
                        package[0] = Convert.ToByte(link.ReceiveTP_CM.NextPacketNumberTransmitted + 1);
                        bytesSent = Convert.ToByte(link.ReceiveTP_CM.TotalNumberOfPackagesTransmitted * 7);
                        for (byte j = 0; j < 7; j++)
                        {
                            if (bytesSent < link.SendTP_CM.TotalMessageSizeBeingTransmitted)
                            {
                                package[j + 1] = link.SendTP_DT.Data[bytesSent++];
                            }
                            else
                            {
                                package[j + 1] = 0xFF;
                            }
                        }
                        bool isSuccess = SendCan(ID, Channel, package);
                        if (!isSuccess)
                        {
                            status = J1939StatusCode.SendError;
                            break;
                        }
                        break;
                    }
            }
            return status;
        }
        public J1939StatusCode SendAcknowledgement
            (byte DA, byte controlByte, byte groupFunctionValue, uint PGNOfRequestedInfo)
        {
            uint ID = Convert.ToUInt32((0x18E8 << 16) | (DA << 8) | link.SendInfo.Address);
            byte[] data = new byte[8];
            data[0] = controlByte;
            data[1] = groupFunctionValue; //The cause of the control byte
            data[2] = 0xFF;
            data[3] = 0xFF;
            data[4] = link.SendInfo.Address; //This source address
            data[5] = (byte)PGNOfRequestedInfo;
            data[6] = (byte)(PGNOfRequestedInfo >> 8);
            data[7] = (byte)(PGNOfRequestedInfo >> 16);
            bool isSuccess = SendCan(ID, Channel, data);
            J1939StatusCode status = J1939StatusCode.SendOK;
            if (!isSuccess)
            {
                status = J1939StatusCode.SendError;
            }
            return status;
        }

        public J1939StatusCode SendRequest(byte DA,uint PGNCode)
        {
            byte[] PGN = new byte[3];
            PGN[0] = (byte)PGNCode;
            PGN[1] = (byte)(PGNCode >> 8);
            PGN[2] = (byte)(PGNCode >> 16);
            uint ID = Convert.ToUInt32((0x18EA << 16) | (DA << 8) | link.SendInfo.Address);
            bool isSuccess = SendCan(ID, Channel, PGN);
            J1939StatusCode status = J1939StatusCode.SendOK;
            if (!isSuccess)
            {
                status = J1939StatusCode.SendError;
            }
            return status;
        }

        public void ReadRequest(byte SA, byte[] data)
        {
            uint PGN = Convert.ToUInt32((data[2] << 16) | (data[1] << 8) | data[0]);
            switch ((PGNCode)PGN)
            {
                case PGNCode.PGN_ACKNOWLEDGEMENT:
                    SendAcknowledgement(SA, (byte)ControlByteCodes.ACKNOWLEDGEMENT_PGN_SUPPORTED, 
                        (byte)GroupFunctionValueCodes.NORMAL, PGN);
                    break;
                case PGNCode.PGN_REQUEST:
                    SendAcknowledgement(SA, (byte)ControlByteCodes.ACKNOWLEDGEMENT_PGN_SUPPORTED,
                        (byte)GroupFunctionValueCodes.NORMAL, PGN);
                    break;
                case PGNCode.PGN_TP_CM:
                    SendAcknowledgement(SA, (byte)ControlByteCodes.ACKNOWLEDGEMENT_PGN_SUPPORTED,
                        (byte)GroupFunctionValueCodes.NORMAL, PGN);
                    break;
                case PGNCode.PGN_TP_DT:
                    SendAcknowledgement(SA, (byte)ControlByteCodes.ACKNOWLEDGEMENT_PGN_SUPPORTED,
                        (byte)GroupFunctionValueCodes.NORMAL, PGN);
                    break;
            }
        }
    }
}