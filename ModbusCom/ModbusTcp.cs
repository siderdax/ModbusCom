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
        public TcpListener Listener { get; private set; }

        public string Ip { get; private set; }
        public string Port { get; private set; }

        private readonly ModbusTcpSlave[] modbusTcpSlaves = new ModbusTcpSlave[MAX_SLAVE_ID];

        /// <summary>
        /// TCP Server
        /// </summary>
        public void StartServer()
        {
            if(Ip == null || Ip.Length == 0)
            {
                Listener = new TcpListener(IPAddress.Any, int.Parse(Port));
            }
            else
            {
                Listener = new TcpListener(IPAddress.Parse(Ip), int.Parse(Port));
            }
            Listener.Start();
        }

        public void StartServer(string address, string port)
        {
            Ip = address;
            Port = port;
            if(address == null || address.Length == 0)
            {
                Listener = new TcpListener(IPAddress.Any, int.Parse(port));
            }
            else
            {
                Listener = new TcpListener(IPAddress.Parse(address), int.Parse(port));
            }
            Listener.Start();
        }

        public void StopServer()
        {
            Listener.Stop();
        }

        public void StartTcpSlave(byte slaveId)
        {
            ThrowRangeException(slaveId);
            modbusTcpSlaves[slaveId - 1] = ModbusTcpSlave.CreateTcp(slaveId, Listener);
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
            Ip = string.Empty;
            Port = string.Empty;
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
