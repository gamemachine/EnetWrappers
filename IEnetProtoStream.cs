using System;
using System.Buffers;
using System.IO;

namespace EnetWrappers
{
    public unsafe interface IEnetProtoStream : IDisposable
    {
        int Length { get; }
        Stream Stream { get; }
        Memory<byte> Memory { get; }
        ReadOnlySequence<byte> Sequence { get; }
        Span<byte> Span { get; }
        byte[] Buffer { get; }
        int Serialize(object message, int offset = 0);
        T Deserialize<T>(byte[] data, int length, int offset = 0);
        T Deserialize<T>(T value, byte[] data, int length, int offset = 0);
        object Deserialize(Type type, byte[] data, int length, int offset = 0);
    }
}
