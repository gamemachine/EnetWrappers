using ENet;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EnetWrappers
{
    public unsafe class EnetHost
    {
        // Topology
        // game client -> server
        // game client -> unity server
        // main server client -> unity server

        // main server client runs on game client in test mode

        private static List<EnetHost> Hosts = new List<EnetHost>();

        public const int MainServerPort = 8160;
        public const int UnityServerPort = 8161;
        public const int IpcPort = 8162;

        public const int DefaultBufferSize = 1024 * 1024;

        private const int ChannelLimit = 12;
        private const int ServerPeerLimit = 1024;

        public static bool EventLoggingEnabled = true;
        public static bool PacketLoggingEnabled = false;

        public IEnetEventHandler EventHandler { get; private set; }
        public IEnetLogger Logger { get; private set; }
        public int PacketsProcessed { get; private set; }
        public int PacketsSent { get; private set; }
        public int PacketsFreedCount { get; private set; }
        public bool IsClient { get; private set; }
        public ConnectionManager ConnectionManager { get; private set; }
       
        private Host Host;
        private Address Address;
        private Peer ServerPeer;
        private bool ShouldStop;
        private int AuthToken;

        public EnetHost(string address, int port, IEnetEventHandler eventHandler, IEnetLogger logger, bool server)
        {
            EventHandler = eventHandler;
            Logger = logger;
            Address = new Address();
            Address.Port = (ushort)port;

            if (!server)
            {
                Address.SetHost(address);
            }

            ConnectionManager = new ConnectionManager(Logger);

            Hosts.Add(this);
        }

        public static void UpdateHosts(int count, int delay = 0)
        {
            for (int i = 0; i < count; i++)
            {
                foreach (var host in Hosts)
                {
                    host.Update();
                }
            }
            if (delay > 0)
            {
                Thread.Sleep(delay);
            }
        }

        public Peer GetServerPeer()
        {
            return ServerPeer;
        }

        public void Dispose()
        {
            if (Host != null)
            {
                Host.Dispose();
                Host = null;
            }
            
        }

        public void CreateServer(bool background = false)
        {
            Host = new Host();
            Host.Create(Address, ServerPeerLimit, ChannelLimit);

            if (background)
            {
                Task.Factory.StartNew(() => Background(), TaskCreationOptions.LongRunning);
            }
        }

        public void StopServer()
        {
            ShouldStop = true;
        }

        public void CreateClient(int authToken)
        {
            AuthToken = authToken;
            Host = new Host();
            Host.Create();

            ServerPeer = Host.Connect(Address, ChannelLimit, (uint)AuthToken);
            IsClient = true;
        }

        public void ClientDisconnect(int playerId)
        {
            ServerPeer.Disconnect((uint)playerId);
        }

        public void EnsureConnection()
        {
            switch(ServerPeer.State)
            {
                case PeerState.Zombie:
                case PeerState.Disconnected:
                case PeerState.Uninitialized:
                    Logger.LogInformation("Enet connect retry on {0}", ServerPeer.State);
                    ServerPeer = Host.Connect(Address, ChannelLimit, (uint)AuthToken);
                    break;
            }
        }
       
        public void Update()
        {
            if (IsClient)
            {
                EnsureConnection();
            }

            Event netEvent;
            bool polled = false;

            while (!polled)
            {
                if (Host.CheckEvents(out netEvent) <= 0)
                {
                    if (Host.Service(0, out netEvent) <= 0)
                    {
                        break;
                    }

                    polled = true;
                }
                ProcessEvent(netEvent);
            }

            Host.Flush();
        }

        private void ProcessEvent(Event netEvent)
        {
            switch (netEvent.Type)
            {
                case EventType.None:
                    break;

                case EventType.Connect:
                    LogEvent(netEvent, false);
                    EventHandler.OnConnect(netEvent);
                    OnConnect(netEvent);
                    break;

                case EventType.Disconnect:
                    LogEvent(netEvent, false);
                    EventHandler.OnDisconnect(netEvent);
                    OnDisconnect(netEvent);
                    break;

                case EventType.Timeout:
                    LogEvent(netEvent, false);
                    EventHandler.OnTimeout(netEvent);
                    OnDisconnect(netEvent);
                    break;

                case EventType.Receive:
                    LogEvent(netEvent, true);
                    PacketsProcessed++;
                    OnReceive(netEvent);
                    netEvent.Packet.Dispose();
                    break;
            }
        }

        private unsafe void OnReceive(Event netEvent)
        {
            Packet packet = netEvent.Packet;
            EnetChannel channel = (EnetChannel)netEvent.ChannelID;

            byte* ptr = (byte*)packet.Data.ToPointer();

            int header = *(int*)ptr;
            EnetMessageType messageType = (EnetMessageType)header;

            byte* data = ptr + 4;

            EventHandler.OnReceive(netEvent.Peer, channel, messageType, data, packet.Length - 4);
        }

        private void OnConnect(Event netEvent)
        {
            if (IsClient) return;
            int authToken = (int)netEvent.Data;
            ConnectionManager.SetConnection(netEvent.Peer, authToken);
        }

        private void OnDisconnect(Event netEvent)
        {
            if (IsClient) return;
            ConnectionManager.RemoveConnection(netEvent.Peer.ID);
        }

        private void LogEvent(Event netEvent, bool isPacket)
        {
            if (isPacket && PacketLoggingEnabled)
            {
                Logger.LogDebug(GetFormattedEvent(netEvent, isPacket));
            }
            else if (!isPacket && EventLoggingEnabled)
            {
                Logger.LogInformation(GetFormattedEvent(netEvent, isPacket));
            }
        }

        private string GetFormattedEvent(Event netEvent, bool isPacket)
        {
            string hostType = IsClient ? "Client" : "Server";
            if (isPacket)
            {
                return string.Format("EnetEvent.{0} Packet PeerId:{1} Ip:{2} Channel:{3} Length:{4} Data:{5}",
                hostType, netEvent.Peer.ID, netEvent.Peer.IP, netEvent.ChannelID, netEvent.Packet.Length, netEvent.Data);
            }
            else
            {
                return string.Format("EnetEvent.{0} {1} Id:{2} Ip:{3} Data:{4}", hostType, netEvent.Type, netEvent.Peer.ID, netEvent.Peer.IP, netEvent.Data);
            }
        }

        private void Background()
        {
            while (true)
            {
                if (ShouldStop)
                {
                    Dispose();
                    return;
                }

                try
                {
                    Update();
                }
                catch (Exception ex)
                {
                    Logger.LogWarning("{0} {1}", ex.Message, ex.StackTrace);
                    
                }

                Thread.Sleep(20);
            }
        }

    }
}
