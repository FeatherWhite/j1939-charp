using System;
using System.Collections.Generic;
using System.Text;

namespace Triumph.J1939
{
    public class J1939Client
    {
        J1939_21UsbCanII j1939tp;
        public IJ1939Tp tp { get; set; }
        public J1939Client()
        {
            j1939tp = new J1939_21UsbCanII();
        }
        public bool SendData(uint pgn, byte[] data)
        {
            return tp.Send(0, (byte)((pgn >> 8) & 0xFF), 7, data);
        }
        public bool SendRequest(byte dataPage, uint pgn)
        {
            byte[] data = new byte[] { (byte)(pgn & 0xFF), (byte)((pgn >> 8) & 0xFF), (byte)((pgn >> 16) & 0xFF) };
            return tp.Send(dataPage, (byte)(((uint)PGNCode.REQUEST >> 8) & 0xFF), 6, data);
        }
        public void Poll()
        {
            tp.Poll();
        }
    }
}
