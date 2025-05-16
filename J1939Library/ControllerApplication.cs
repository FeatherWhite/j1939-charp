using System;
using System.Collections.Generic;
using System.Text;

namespace Triumph.J1939
{
    public class ControllerApplication
    {
        public enum CAState
        {
            None = 0,
            WaitVeto = 1,
            Normal = 2,
            CannotClaim = 3
        }
        public byte DeviceAddress { get; set; }
        public CAState State { get; set; }
        public bool MessageAcceptable(byte da)
        {
            if(State != CAState.Normal)
            {
                return false;
            }
            if(da == ParameterGroupNumber.GLOBAL)
            {
                return true;
            }
            return DeviceAddress == da;
        }
        public void ProcessRequest(MessageId mid,byte da,byte[] data,uint timestamp)
        {

        }
    }
}
