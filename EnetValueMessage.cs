

namespace EnetWrappers
{
    public struct EnetValueMessage<T> where T : unmanaged
    {
        public EnetMessageType MessageType;
        public EnetMessageFlags Flags;
        public T Value;
        public int ClientId;
    }
}
