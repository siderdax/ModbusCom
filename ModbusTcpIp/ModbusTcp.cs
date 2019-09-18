using Modbus.Data;
using Modbus.Device;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ModbusTcpIp
{
    public class ModbusTcp : Modbus, IDisposable
    {
        private TcpListener server;
        private ModbusTcpSlave[] modbusTcpSlaves = new ModbusTcpSlave[MAX_SLAVE_ID];
        private bool disposed = false;

        public string Ip { get; private set; }
        public string Port { get; private set; }

        /// <summary>
        /// TCP Slave (Server)
        /// </summary>
        public void StartServer()
        {
            if(Ip == null || Ip.Length == 0)
            {
                server = new TcpListener(IPAddress.Any, Int32.Parse(Port));
            }
            else
            {
                server = new TcpListener(IPAddress.Parse(Ip), Int32.Parse(Port));
            }
            server.Start();
        }

        public void StartServer(string address, string port)
        {
            Ip = address;
            Port = port;
            if(address == null || address.Length == 0)
            {
                server = new TcpListener(IPAddress.Any, Int32.Parse(port));
            }
            else
            {
                server = new TcpListener(IPAddress.Parse(address), Int32.Parse(port));
            }
            server.Start();
        }

        public void StopServer()
        {
            server.Stop();
        }

        public void StartTcpSlave(byte slaveId)
        {
            ThrowRangeException(slaveId);
            modbusTcpSlaves[slaveId - 1] = ModbusTcpSlave.CreateTcp(slaveId, server);
            modbusTcpSlaves[slaveId - 1].DataStore = DataStoreFactory.CreateDefaultDataStore();
            Thread slaveThread = new Thread(modbusTcpSlaves[slaveId - 1].Listen);
            slaveThread.Start();
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

        ~ModbusTcp()
        {
            Dispose(false);
        }
    }
}
