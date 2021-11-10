using ENet;
using System;
using System.Runtime.CompilerServices;

namespace EnetWrappers
{
    public class EnetUtil
    {
        public static PacketFlags ToPacketFlags(EnetMessageFlags flags)
        {
            switch (flags)
            {
                case EnetMessageFlags.Unreliable:
                    return PacketFlags.Unsequenced | PacketFlags.Unthrottled;
                case EnetMessageFlags.Reliable:
                    return PacketFlags.Reliable;
                default:
                    return PacketFlags.Reliable;
            }
        }

        public static unsafe T Read<T>(byte* data) where T : unmanaged
        {
            byte* ptr = data;
            return *(T*)ptr;
        }

        public static unsafe T Deserialize<T>(byte* data, int length, IEnetProtoStream stream, byte[] buffer)
        {
            Span<byte> from = new Span<byte>(data, length);
            Span<byte> to = stream.Span;
            MemoryCopy(ref from, 0, buffer, 0, length);
            return stream.Deserialize<T>(buffer, length, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void MemoryCopy(ref Span<byte> source, int sourceOffset, byte[] destination, int destinationOffset, int length)
        {
            if (length > 0)
            {
                fixed (byte* sourcePointer = &source[sourceOffset])
                {
                    fixed (byte* destinationPointer = &destination[destinationOffset])
                    {
                        Buffer.MemoryCopy(sourcePointer, destinationPointer, length, length);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void MemoryCopy(byte* source, int sourceOffset, byte* destination, int destinationOffset, int length)
        {
            if (length > 0)
            {
                byte* sourcePointer = &source[sourceOffset];
                byte* destinationPointer = &destination[destinationOffset];
                Buffer.MemoryCopy(sourcePointer, destinationPointer, length, length);
            }
        }

        public static unsafe int SizeOf<T>() where T : unmanaged
        {
            return sizeof(T);
        }

    }
}
