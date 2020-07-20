using System;
using System.IO.Ports;
using Modbus.Device;

namespace ModbusTcpIp
{
    public class ModbusMaster : Modbus, IDisposable
    {
        private bool disposed = false;
        private SerialPort _serial;
        public SerialPort Serial { get => _serial; }

        public string Port { get; private set; }
        public int Baud { get; private set; }

        private ModbusSerialMaster modbusSerialMaster;

        /// <summary>
        /// TCP Master
        /// </summary>
        public void StartMaster()
        {
            _serial = new SerialPort(Port, Baud);
            _serial.Open();
        }

        public void StartMaster(string port, int baud)
        {
            _serial = new SerialPort(port, baud);
            _serial.Open();
        }

        public void StopMaster()
        {
            _serial.Close();
        }

        public void StartSerialMaster()
        {
            modbusSerialMaster = ModbusSerialMaster.CreateRtu(_serial);
        }

        public void StopSerialMaster()
        {
            modbusSerialMaster.Dispose();
        }

        public ushort[] ReadSerialMasterHoldingRegisters(byte slaveId, ushort address, ushort length)
        {
            return modbusSerialMaster.ReadHoldingRegisters(slaveId, (ushort)(address - 1), length);
        }

        public void WriteSerialMasterHoldingRegister(byte slaveId, ushort address, ushort register)
        {
            modbusSerialMaster.WriteSingleRegister(slaveId, (ushort)(address - 1), register);
        }

        public ushort[] ReadSerialMasterInputRegisters(byte slaveId, ushort address, ushort length)
        {
            return modbusSerialMaster.ReadInputRegisters(slaveId, (ushort)(address - 1), length);
        }

        public ModbusMaster()
        {
            Port = String.Empty;
            Baud = 9600;
        }

        public ModbusMaster(string port, int baud) : this()
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
                    _serial.Dispose();
                    StopSerialMaster();
                }
                disposed = true;
            }
        }
    }
}
