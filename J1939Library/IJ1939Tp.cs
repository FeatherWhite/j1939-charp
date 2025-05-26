using System;
using System.Collections.Generic;
using System.Text;

namespace Triumph.J1939
{
    public interface IJ1939Tp
    {
        public J1939TpStatus Job();
        public bool Send(byte dataPage, byte pduFormat, 
            byte priority, byte[] data);
        public void Receive();
        public byte[] GetRevcData();
    }
}
