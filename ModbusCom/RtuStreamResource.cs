using Modbus.IO;
using System.IO.Ports;
using System.Linq;

namespace ModbusCom
{
    public class RtuStreamResource : IStreamResource
    {
        private readonly RtuStreamDistributor _distributor;

        public int ID { get; private set; }

        public int ReadTimeout
        {
            get => _distributor.ReadTimeout;
            set
            {
                if (_distributor.ReadTimeout != value)
                    _distributor.ReadTimeout = value;
            }
        }
        public int WriteTimeout
        {
            get => _distributor.WriteTimeout;
            set
            {
                if (_distributor.WriteTimeout != value)
                    _distributor.WriteTimeout = value;
            }
        }

        public int InfiniteTimeout => SerialPort.InfiniteTimeout;

        public void DiscardInBuffer()
        {
            _distributor.DiscardInBuffer();
            _distributor.ClearPoolData();
        }

        public int Read(byte[] buffer, int offset, int count) => _distributor.ReadPoolBytes(ID, buffer, offset, count);
        public void Write(byte[] buffer, int offset, int count) => _distributor.Write(buffer, offset, count);
        public void Dispose() => _distributor.Dispose();

        public RtuStreamResource(RtuStreamDistributor distributor, int id)
        {
            ID = id;
            _distributor = distributor;
            _distributor.AddMember(id);
        }
    }
}
