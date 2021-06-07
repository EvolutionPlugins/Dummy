using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SDG.NetTransport;
using SDG.Unturned;

namespace EvolutionPlugins.Dummy.Extensions.Network
{
    public class DummyNetworkStream : Stream
    {
        
        internal static List<DummyNetworkStream> Streams { get; } = new List<DummyNetworkStream>();
        
        private static ClientInstanceMethod<byte[]> SendRawData { get; } = 
            ClientInstanceMethod<byte[]>.Get(typeof(DummyNetworkStream), "RawData");

        
        public DummyNetworkStream(PlayerDummy dummy)
        {
            _dummy = dummy;
            _buffer = new List<byte[]>();
            Streams.Add(this);
        }

        private readonly List<byte[]> _buffer;

        public override bool Equals(object o)
        {
            return o is DummyNetworkStream stream && stream._dummy.Data.UnturnedUser.Player.Player.GetNetId() == _dummy.Data.UnturnedUser.Player.Player.GetNetId();
        }
        
        public bool Equals(NetId o)
        {
            return o == _dummy.Data.UnturnedUser.Player.Player.GetNetId();
        }

        public byte[] Buffer
        {
            get
            {
                List<byte> bytes = new List<byte>();
                foreach (byte[] b in _buffer)
                {
                    foreach (byte byt in b)
                        bytes.Add(byt);
                }

                return bytes.ToArray();
            }
        }


        public override void Flush()
        {
            _buffer.Clear();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            byte[] data = Buffer;
            if (Position + count > data.Length)
            {
                _canRead = false;
                return 0;
            }

            System.Buffer.BlockCopy(data, (int) Position, buffer, offset, count);
            Position += count;
            return data.Length;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return 0;
        }

        public override void SetLength(long value)
        {
            List<byte> newBuffer = Buffer.ToList().GetRange(0, (int) value);
            _buffer.Clear();
            _buffer.Add(newBuffer.ToArray());
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _buffer.Add(buffer);
            _canRead = true;
            SendRawData.Invoke(_dummy.Data.UnturnedUser.Player.Player.GetNetId(), ENetReliability.Reliable, _dummy.Data.UnturnedUser.Player.Player.channel.GetOwnerTransportConnection(),buffer.ToArray());
        }

        internal void Write(byte[] buffer)
        {
            _buffer.Add(buffer);
            _canRead = true;
        }

        private bool _canRead = true;
        private readonly PlayerDummy _dummy;

        public override bool CanRead
        {
            get => _canRead;
        } 
        public override bool CanSeek { get; } = true;
        public override bool CanWrite { get; } = true;
        public override long Length
        {
            get => Buffer.Length;
        }
        public override long Position { get; set; }
    }
}