using ENet;

namespace EnetWrappers
{
    public interface IEnetEventHandler
    {
        unsafe void OnReceive(Peer peer, EnetChannel channel, EnetMessageType messageType, byte* data, int length);
        void OnConnect(Event netEvent);
        void OnDisconnect(Event netEvent);
        void OnTimeout(Event netEvent);
    }
}
