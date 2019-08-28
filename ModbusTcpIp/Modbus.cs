using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModbusTcpIp
{
    public class Modbus
    {
        protected const byte MAX_SLAVE_ID = 247;

        protected void ThrowRangeException(byte slaveId)
        {
            if (slaveId > MAX_SLAVE_ID)
            {
                throw new Exception("Slave ID range is 1 to 247.");
            }
        }
    }
}
