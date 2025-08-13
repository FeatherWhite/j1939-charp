using System;
using System.Collections.Generic;
using System.Text;

namespace Triumph.J1939
{
    public class J1939Client
    {
        public const int Idle = 0;
        public const int SendingAwaitCTS = 1;
        public const int Sending = 2;
        public const int SendingAwaitACK = 3;
        public const int RecvingAwaitRTS = 4;
        public const int RecvingTPDT = 5;

        public byte[] RecvBuffer { get; private set; }
        //public ushort RecvSize { get; private set; }
        public byte[] SendBuffer { get; private set; }
        //public ushort SendSize { get; private set; }

        public int state { get; set; }

        public IJ1939Tp tp { get; set; }
        public uint Channel
        {
            get { return tp.Channel; }
            set {tp.Channel = value; }
        }
        public J1939Client()
        {
            RecvBuffer = new byte[0];
            SendBuffer = new byte[0];
        }
        public bool SendData(uint pgn, byte[] data)
        {
            SendBuffer = data;
            bool ret = tp.Send(0, (byte)((pgn >> 8) & 0xFF), 7, data);
            ChanageState(SendingAwaitCTS);
            return ret;
        }
        public bool SendRequest(byte dataPage, uint pgn)
        {
            SendBuffer = [(byte)(pgn & 0xFF), (byte)((pgn >> 8) & 0xFF), (byte)((pgn >> 16) & 0xFF),0x00, 0x00, 0x00, 0x00,0x00];
            bool ret = tp.Send(dataPage, (byte)(((uint)PGNCode.REQUEST >> 8) & 0xFF), 6, SendBuffer);
            ChanageState(RecvingAwaitRTS);
            return ret;
        }
        public void Poll()
        {
            switch (state)
            {             
                case Idle:
                    break;
                case SendingAwaitCTS:
                    tp.Receive();
                    ChanageState(Sending);
                    //Console.WriteLine("SendingAwaitCTS");
                    break;
                case Sending:
                    J1939TpStatus sendStatus = tp.Job();
                    if(sendStatus == J1939TpStatus.SendTpDataFinished)
                    {
                        ChanageState(SendingAwaitACK);
                    }
                    if(sendStatus == J1939TpStatus.SendingAwaitCTS)
                    {
                        ChanageState(SendingAwaitCTS);
                    }
                    if (sendStatus == J1939TpStatus.SendTimeout)
                    {
                        ChanageState(Idle);
                    }
                    //Console.WriteLine("Sending");
                    break;
                case SendingAwaitACK:
                    tp.Receive();
                    J1939TpStatus recvStatus1 = tp.Job();
                    ChanageState(Idle);
                    //Console.WriteLine("SendingAwaitACK");
                    break;
                case RecvingAwaitRTS:
                    tp.Receive();
                    J1939TpStatus recvStatus2 = tp.Job();
                    ChanageState(RecvingTPDT);
                    if(recvStatus2 == J1939TpStatus.RecvTimeout)
                    {
                        ChanageState(Idle);
                    }
                    //Console.WriteLine("RecvingAwaitRTS");
                    break;
                case RecvingTPDT:
                    tp.Receive();
                    J1939TpStatus recvStatus3 = tp.Job();
                    if(recvStatus3 == J1939TpStatus.RecvFinished )
                    {
                        ChanageState(Idle);
                        RecvBuffer = tp.GetRevcData();
                    }
                    if(recvStatus3 == J1939TpStatus.RecvTimeout)
                    {
                        ChanageState(Idle);
                    }
                    //Console.WriteLine("RecvingTPDT");
                    break;
   
            }
        }
        ///// <summary>
        ///// 发送普通CAN消息
        ///// </summary>
        ///// <param name="id">发送CanId</param>
        ///// <param name="channel">Can通道索引</param>
        ///// <param name="data">发送的数据</param>
        //public void Send(uint id, uint channel, byte[] data)
        //{
        //    tp.Send(id, channel, data);
        //}
        ///// <summary>
        ///// 接收普通CAN消息
        ///// </summary>
        ///// <param name="channel">Can通道索引</param>
        ///// <param name="receiveCanId">接收CanId</param>
        ///// <returns></returns>
        //public byte[] Receive(uint channel, uint receiveCanId)
        //{
        //    return tp.Receive(channel, receiveCanId);
        //}


        private void ChanageState(int state)
        {
            if(this.state != state)
            {
                this.state = state;
            }
        }
    }
}
