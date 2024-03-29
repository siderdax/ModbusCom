﻿using System.Net.Sockets;
using System;
using Modbus.Device;

namespace ModbusCom
{
    public class ModbusIp : Modbus
    {
        private bool disposed = false;
        private TcpClient client;
        public TcpClient Client { get => client; }

        public string Ip { get; private set; }
        public string Port { get; private set; }

        private ModbusIpMaster modbusIpMaster;

        /// <summary>
        /// TCP Client
        /// </summary>
        public void StartClient()
        {
            client = new TcpClient(Ip, int.Parse(Port));
        }

        public void StartClient(string address, string port)
        {
            client = new TcpClient(address, int.Parse(port));
        }

        public void StopClient()
        {
            client.Close();
        }

        public void StartIpMaster()
        {
            modbusIpMaster = ModbusIpMaster.CreateIp(client);
        }

        public void StopIpMaster()
        {
            modbusIpMaster.Dispose();
        }

        public ushort[] ReadIpMasterHoldingRegisters(byte slaveId, ushort address, ushort length)
        {
            return modbusIpMaster.ReadHoldingRegisters(slaveId, (ushort)(address - 1), length);
        }

        public void WriteIpMasterHoldingRegister(byte slaveId, ushort address, ushort register)
        {
            modbusIpMaster.WriteSingleRegister(slaveId, (ushort)(address - 1), register);
        }

        public ushort[] ReadIpMasterInputRegisters(byte slaveId, ushort address, ushort length)
        {
            return modbusIpMaster.ReadInputRegisters(slaveId, (ushort)(address - 1), length);
        }

        public ModbusIp()
        {
            Ip = string.Empty;
            Port = string.Empty;
        }

        public ModbusIp(string ip, string port) : this()
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
            if (!disposed)
            {
                if (disposing)
                {
                    StopIpMaster();
                }
                disposed = true;
            }
        }
    }
}
