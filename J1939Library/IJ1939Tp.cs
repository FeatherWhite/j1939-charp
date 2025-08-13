using System;
using System.Collections.Generic;
using System.Text;

namespace Triumph.J1939
{
    public interface IJ1939Tp
    {
        public J1939TpStatus Job();
        public uint Channel { get; set; }
        public bool Send(byte dataPage, byte pduFormat, 
            byte priority, byte[] data);
        public void Receive();
        public byte[] GetRevcData();
        //public byte[] Receive(uint channel, uint receiveCanId);
        //public bool Send(uint id, uint channel, byte[] data);
    }
}
