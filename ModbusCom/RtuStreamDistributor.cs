using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace ModbusCom
{
    public class RtuStreamDistributor : SerialPort
    {
        private const int MAX_DATA_CAPACITY = 1024;

        private readonly Dictionary<int, List<byte>> _dataPool;

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (BytesToRead > 0)
            {
                byte[] bytes = new byte[BytesToRead];

                Read(bytes, 0, bytes.Length);

                lock (_dataPool)
                {
                    foreach (var dl in _dataPool)
                    {
                        if (dl.Value.Capacity - dl.Value.Count < bytes.Length)
                            dl.Value.RemoveRange(0, bytes.Length - (dl.Value.Capacity - dl.Value.Count));

                        dl.Value.AddRange(bytes);
                    }
                }
            }
        }

        public int ReadPoolBytes(int id, byte[] buffer, int offset, int count)
        {
            lock (_dataPool)
            {
                if (_dataPool.ContainsKey(id))
                {
                    int length = count > _dataPool[id].Count ? _dataPool[id].Count : count;
                    var poolBytes = _dataPool[id].Take(length - offset);
                    var newBufData = buffer.Take(offset).Concat(poolBytes).ToArray();

                    for (int i = 0; i < length; i++)
                    {
                        buffer[i] = newBufData[i];
                    }

                    _dataPool[id].RemoveRange(0, length);

                    return length;
                }
                else
                {
                    throw new InvalidOperationException($"ID {id} is not exists.");
                }
            }
        }

        public void ClearPoolData()
        {
            lock (_dataPool)
            {
                foreach (var dl in _dataPool)
                {
                    dl.Value.Clear();
                }
            }
        }

        public bool AddMember(int id)
        {
            lock (_dataPool)
            {
                if (!_dataPool.ContainsKey(id))
                {
                    _dataPool.Add(id, new List<byte>(MAX_DATA_CAPACITY));
                    return true;
                }

                return false;
            }
        }

        public void RemoveMember(int id)
        {
            lock (_dataPool)
            {
                if (_dataPool.ContainsKey(id))
                    _dataPool.Remove(id);
            }
        }

        public RtuStreamDistributor()
        {
            _dataPool = new Dictionary<int, List<byte>>();
            DataReceived += OnDataReceived;
        }
    }
}
