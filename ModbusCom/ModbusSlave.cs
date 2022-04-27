using System;
using System.Threading.Tasks;
using Modbus.Data;
using Modbus.Device;

namespace ModbusCom
{
    public class ModbusSlave : Modbus
    {
        private bool disposed = false;
        public RtuStreamDistributor StreamDistributor { get; private set; }

        public string Port { get; private set; }
        public int Baud { get; private set; }

        private readonly ModbusSerialSlave[] modbusSerialSlaves = new ModbusSerialSlave[MAX_SLAVE_ID];

        /// <summary>
        /// TCP Slave
        /// </summary>
        public void StartSlave()
        {
            StreamDistributor = new RtuStreamDistributor { PortName = Port, BaudRate = Baud };
            StreamDistributor.Open();
        }

        public void StartSlave(string port, int baud)
        {
            StreamDistributor = new RtuStreamDistributor { PortName = port, BaudRate = baud };
            StreamDistributor.Open();
        }

        public void StopSlave()
        {
            StreamDistributor.Close();
        }

        public void StartSerialSlave(byte slaveId)
        {
            ThrowRangeException(slaveId);
            modbusSerialSlaves[slaveId - 1] = ModbusSerialSlave.CreateRtu(
                slaveId, new RtuStreamResource(StreamDistributor, slaveId));
            modbusSerialSlaves[slaveId - 1].DataStore = DataStoreFactory.CreateDefaultDataStore();
            Task.Run(() => modbusSerialSlaves[slaveId - 1].Listen());
        }

        public void StopSerialSlave(byte slaveId)
        {
            ThrowRangeException(slaveId);
            if (modbusSerialSlaves[slaveId - 1] != null)
            {
                modbusSerialSlaves[slaveId - 1].Dispose();
            }
        }

        public void ClearSerialSlave()
        {
            foreach (ModbusSerialSlave s in modbusSerialSlaves)
            {
                s.Dispose();
            }
        }

        public ushort ReadSerialSlaveHoldingRegister(byte slaveId, ushort address)
        {
            return modbusSerialSlaves[slaveId - 1].DataStore.HoldingRegisters[address];
        }

        public void WriteSerialSlaveHoldingRegister(byte slaveId, ushort address, ushort data)
        {
            modbusSerialSlaves[slaveId - 1].DataStore.HoldingRegisters[address] = data;
        }

        public ushort ReadSerialSlaveInputRegister(byte slaveId, ushort address)
        {
            return modbusSerialSlaves[slaveId - 1].DataStore.InputRegisters[address];
        }

        public void WriteSerialSlaveInputRegister(byte slaveId, ushort address, ushort data)
        {
            modbusSerialSlaves[slaveId - 1].DataStore.InputRegisters[address] = data;
        }

        public ModbusSlave()
        {
            Port = string.Empty;
            Baud = 9600;
        }

        public ModbusSlave(string port, int baud) : this()
        {
            Port = port;
            Baud = baud;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    ClearSerialSlave();
                    StreamDistributor.Dispose();
                }
                disposed = true;
            }
        }
    }
}
