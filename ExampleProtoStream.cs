using ProtoBuf;
using System;
using System.Buffers;
using System.IO;

namespace EnetWrappers
{
    public class ExampleProtoStream : IEnetProtoStream, IDisposable
    {
        public const int Size = 1024 * 1024 * 2;
        private MemoryStream _stream;
        private byte[] _buffer;

        public int Length => _buffer.Length;
        public Stream Stream => _stream;
        public byte[] Buffer => _buffer;

        public ReadOnlySequence<byte> Sequence
        {
            get
            {
                return new ReadOnlySequence<byte>(_buffer);
            }
        }

        public Memory<byte> Memory
        {
            get
            {
                return new Memory<byte>(_buffer);
            }
        }

        public Span<byte> Span
        {
            get
            {
                return new Span<byte>(_buffer);
            }
        }

        public ExampleProtoStream(int bufferSize)
        {
            _buffer = new byte[bufferSize];
            _stream = new MemoryStream(_buffer, true);
        }

        public ExampleProtoStream(byte[] buffer)
        {
            _buffer = buffer;
            _stream = new MemoryStream(_buffer, true);
        }

        public void Dispose()
        {
        }

        public int Serialize(object message, int offset = 0)
        {
            _stream.Position = offset;
            Serializer.Serialize(_stream, message);
            return (int)_stream.Position;
        }

        public int SerializeWithLengthPrefix<T>(T message)
        {
            _stream.Position = 0;
            Serializer.SerializeWithLengthPrefix(_stream, message, PrefixStyle.Base128);
            return (int)_stream.Position;
        }

        public T Deserialize<T>(byte[] data, int length, int offset = 0)
        {
            _stream.SetLength(0);
            _stream.Write(data, offset, length);
            _stream.Position = 0;
            return Serializer.Deserialize<T>(_stream);
        }

        public T Deserialize<T>(T value, byte[] data, int length, int offset = 0)
        {
            _stream.SetLength(0);
            _stream.Write(data, offset, length);
            _stream.Position = 0;
            return Serializer.Deserialize<T>(_stream);
        }

        public object Deserialize(Type type, byte[] data, int length, int offset = 0)
        {
            _stream.SetLength(0);
            _stream.Write(data, offset, length);
            _stream.Position = 0;
            return Serializer.Deserialize(type, _stream);
        }
    }
}
