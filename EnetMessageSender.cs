using ENet;
using System;

namespace EnetWrappers
{
    public unsafe class EnetMessageSender
    {
        public byte[] UserSendBuffer { get; private set; }
        private byte[] SendBuffer;
        private EnetHost Host;
        private IEnetProtoStream SendStream;
        

        public EnetMessageSender(IEnetProtoStream sendStream, EnetHost host)
        {
            Host = host;
            SendStream = sendStream;
            SendBuffer = new byte[EnetHost.DefaultBufferSize];
            UserSendBuffer = new byte[EnetHost.DefaultBufferSize];
        }

        public bool Send(EnetRawMessage message)
        {
            Peer peer = GetPeer(message.ClientId);
            if (!peer.IsSet) return false;
           
            return SendTo(peer, message.Channel, message.MessageType, message.Data, message.Length, message.Flags);
        }

        public bool Send<T>(EnetValueMessage<T> message) where T : unmanaged
        {
            Peer peer = GetPeer(message.ClientId);
            if (!peer.IsSet) return false;

            return SendValueTo(peer, message.MessageType, message.Value, message.Flags);
        }

        public unsafe bool SendValue<T>(EnetMessageType messageType, T value, EnetMessageFlags flags) where T : unmanaged
        {
            return SendValueTo(Host.GetServerPeer(), messageType, value, flags);
        }

        public unsafe bool SendValueTo<T>(Peer peer, EnetMessageType messageType, T value, EnetMessageFlags flags) where T : unmanaged
        {
            WriteMessageType(messageType);
            int size = EnetUtil.SizeOf<T>();

            fixed(byte* sendPtr = &SendBuffer[0])
            {
                byte* destinationPointer = sendPtr + 4;

                Buffer.MemoryCopy(&value, destinationPointer, size, size);
            }
           
            return SendBufferTo(peer, EnetChannel.ValueType, size + 4, flags);
        }

        public unsafe bool Send(EnetChannel channel, EnetMessageType messageType, byte[] data, int length, EnetMessageFlags flags)
        {
            return SendTo(Host.GetServerPeer(), channel, messageType, data, length, flags);
        }

        public unsafe bool SendTo(Peer peer, EnetChannel channel, EnetMessageType messageType, byte[] data, int length, EnetMessageFlags flags)
        {
            fixed (byte* ptr = &data[0])
            {
                return SendTo(peer, channel, messageType, ptr, length, flags);
            }
        }

        public unsafe bool Send(EnetChannel channel, EnetMessageType messageType, byte* data, int length, EnetMessageFlags flags)
        {
            return SendTo(Host.GetServerPeer(), channel, messageType, data, length, flags);
        }

        public unsafe bool SendTo(Peer peer, EnetChannel channel, EnetMessageType messageType, byte* data, int length, EnetMessageFlags flags)
        {
            WriteMessageType(messageType);
            fixed (byte* sendPtr = &SendBuffer[0])
            {
                byte* destinationPointer = sendPtr + 4;
                Buffer.MemoryCopy(data, destinationPointer, length, length);
            }
            
            return SendBufferTo(peer, channel, length + 4, flags);

        }

        public unsafe bool SendProto(EnetMessageType messageType, object message, EnetMessageFlags flags)
        {
            return SendProtoTo(Host.GetServerPeer(), messageType, message, flags);
        }

        public unsafe bool SendProtoTo(Peer peer, EnetMessageType messageType, object message, EnetMessageFlags flags)
        {
            SerializeWithHeader(messageType, message, out int length);
            EnetChannel channel = EnetChannel.Protobuf;
            Packet packet = new Packet();
            PacketFlags packetFlags = EnetUtil.ToPacketFlags(flags);

            packet.Create(SendBuffer, length, packetFlags);

            if (peer.Send((byte)channel, ref packet))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private unsafe bool SendBufferTo(Peer peer, EnetChannel channel, int length, EnetMessageFlags flags)
        {
            Packet packet = new Packet();
            PacketFlags packetFlags = EnetUtil.ToPacketFlags(flags);
            packet.Create(SendBuffer, length, packetFlags);

            if (peer.Send((byte)channel, ref packet))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private Peer GetPeer(int clientId)
        {
            Peer peer = default;
            if (Host.IsClient)
            {
                peer = Host.GetServerPeer();
            }
            else
            {
                if (Host.ConnectionManager.TryGetConnectionByClientId(clientId, out var connection))
                {
                    peer = connection.Peer;
                }
            }
            return peer;
        }

        private unsafe void WriteMessageType(EnetMessageType messageType)
        {
            fixed (byte* sendPtr = &SendBuffer[0])
            {
                *(int*)sendPtr = (int)messageType;
            }
           
        }

        private unsafe void SerializeWithHeader(EnetMessageType messageType, object message, out int length)
        {
            int streamPos = SendStream.Serialize(message, 4);
            int messageLength = streamPos - 4;
            var span = SendStream.Span;
            fixed (byte* ptr = &span[0])
            {
                *(int*)ptr = (int)messageType;
            }
            length = streamPos;

            EnetUtil.MemoryCopy(ref span, 0, SendBuffer, 0, length);
        }

        
    }
}
