using System;
using System.Collections.Generic;
using System.Text;

namespace Triumph.J1939
{
    public class J1939msg
    {
        public byte prio = 0;
        public uint pgn = 0;
        public byte da = 0;
        public byte sa = 0;
        public uint dlc = 0;
        public byte[] data;

        public J1939msg()
        {
            data = new byte[1785];
        }
    }

}
