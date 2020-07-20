using System.Diagnostics;
using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Modbus.Data;
using Modbus.Device;

namespace ModbusCom
{
    public class ModbusSlave : Modbus
    {
        private bool disposed = false;
        private SerialPort _serial;
        public SerialPort Serial { get => _serial; }

        public string Port { get; private set; }
        public int Baud { get; private set; }

        private ModbusSerialSlave[] modbusSerialSlaves = new ModbusSerialSlave[MAX_SLAVE_ID];

        /// <summary>
        /// TCP Slave
        /// </summary>
        public void StartSlave()
        {
            _serial = new SerialPort(Port, Baud);
            _serial.Open();
        }

        public void StartSlave(string port, int baud)
        {
            _serial = new SerialPort(port, baud);
            _serial.Open();
        }

        public void StopSlave()
        {
            _serial.Close();
        }

        public void StartSerialSlave(byte slaveId)
        {
            ThrowRangeException(slaveId);
            modbusSerialSlaves[slaveId - 1] = ModbusSerialSlave.CreateRtu(slaveId, _serial);
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
                    _serial.Dispose();
                }
                disposed = true;
            }
        }
    }
}
