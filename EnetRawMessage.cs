﻿using GameCommon.MemoryUtils;
using Unity.Collections.LowLevel.Unsafe;

namespace EnetWrappers
{
    public unsafe struct EnetRawMessage
    {
        [NativeDisableUnsafePtrRestriction]
        public byte* Data;
        public int Length;
        public EnetChannel Channel;
        public EnetMessageType MessageType;
        public EnetMessageFlags Flags;
        public int ClientId;

        public static EnetRawMessage Create(EnetChannel channel, EnetMessageType messageType, EnetMessageFlags flags, byte* data, int length)
        {

            EnetRawMessage message = new EnetRawMessage
            {
                Data = (byte*)RpMalloc.Malloc(length),
                Length = length,
                Channel = channel,
                MessageType = messageType,
                Flags = flags
            };
            EnetUtil.MemoryCopy(data, 0, message.Data, length, length);
            return message;
        }


        public void Dispose()
        {
            if (Data != null)
            {
                RpMalloc.Free(Data);
                Data = null;
            }
        }
    }
}
