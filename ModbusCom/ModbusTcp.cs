using System.Threading.Tasks;
using Modbus.Data;
using Modbus.Device;
using System;
using System.Net;
using System.Net.Sockets;

namespace ModbusCom
{
    public class ModbusTcp : Modbus, IDisposable
    {
        private bool disposed = false;
        private TcpListener listener;
        public TcpListener Listener { get => listener; }

        public string Ip { get; private set; }
        public string Port { get; private set; }

        private ModbusTcpSlave[] modbusTcpSlaves = new ModbusTcpSlave[MAX_SLAVE_ID];

        /// <summary>
        /// TCP Server
        /// </summary>
        public void StartServer()
        {
            if(Ip == null || Ip.Length == 0)
            {
                listener = new TcpListener(IPAddress.Any, Int32.Parse(Port));
            }
            else
            {
                listener = new TcpListener(IPAddress.Parse(Ip), Int32.Parse(Port));
            }
            listener.Start();
        }

        public void StartServer(string address, string port)
        {
            Ip = address;
            Port = port;
            if(address == null || address.Length == 0)
            {
                listener = new TcpListener(IPAddress.Any, Int32.Parse(port));
            }
            else
            {
                listener = new TcpListener(IPAddress.Parse(address), Int32.Parse(port));
            }
            listener.Start();
        }

        public void StopServer()
        {
            listener.Stop();
        }

        public void StartTcpSlave(byte slaveId)
        {
            ThrowRangeException(slaveId);
            modbusTcpSlaves[slaveId - 1] = ModbusTcpSlave.CreateTcp(slaveId, listener);
            modbusTcpSlaves[slaveId - 1].DataStore = DataStoreFactory.CreateDefaultDataStore();
            Task.Run(() => modbusTcpSlaves[slaveId - 1].Listen());
        }

        public void StopTcpSlave(byte slaveId)
        {
            ThrowRangeException(slaveId);
            if (modbusTcpSlaves[slaveId - 1] != null)
            {
                modbusTcpSlaves[slaveId - 1].Dispose();
            }
        }

        public void ClearTcpSlave()
        {
            foreach (ModbusTcpSlave s in modbusTcpSlaves)
            {
                s.Dispose();
            }
        }

        public ushort ReadTcpSlaveHoldingRegister(byte slaveId, ushort address)
        {
            return modbusTcpSlaves[slaveId - 1].DataStore.HoldingRegisters[address];
        }

        public void WriteTcpSlaveHoldingRegister(byte slaveId, ushort address, ushort data)
        {
            modbusTcpSlaves[slaveId - 1].DataStore.HoldingRegisters[address] = data;
        }

        public ushort ReadTcpSlaveInputRegister(byte slaveId, ushort address)
        {
            return modbusTcpSlaves[slaveId - 1].DataStore.InputRegisters[address];
        }

        public void WriteTcpSlaveInputRegister(byte slaveId, ushort address, ushort data)
        {
            modbusTcpSlaves[slaveId - 1].DataStore.InputRegisters[address] = data;
        }

        public ModbusTcp()
        {
            Ip = String.Empty;
            Port = String.Empty;
        }

        public ModbusTcp(string ip, string port) : this()
        {
            Ip = ip;
            Port = port;
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
                    ClearTcpSlave();
                }
                disposed = true;
            }
        }
    }
}
