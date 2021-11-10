using ENet;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EnetWrappers
{
    public class ConnectionManager
    {
        public Dictionary<uint, EnetConnection> ConnectionsReadOnly { get; private set; } = new Dictionary<uint, EnetConnection>();

        private IEnetLogger Logger;
        private ConcurrentDictionary<uint, EnetConnection> Connections = new ConcurrentDictionary<uint, EnetConnection>();
        private ConcurrentDictionary<int, uint> ClientToPeerId = new ConcurrentDictionary<int, uint>();
        private ConcurrentDictionary<int, int> Authenticated = new ConcurrentDictionary<int, int>();

        public ConnectionManager(IEnetLogger logger)
        {
            Logger = logger;
        }

        public void SetAuthenticated(int authToken, int clientId)
        {
            Authenticated[authToken] = clientId;
        }

        public bool TryGetConnectionByClientId(int clientId, out EnetConnection connection)
        {
            connection = default;
            if (ClientToPeerId.TryGetValue(clientId, out uint id))
            {
                return Connections.TryGetValue(id, out connection);
            }
            return false;
        }

        public bool TryGetConnection(uint id, out EnetConnection connection)
        {
            return Connections.TryGetValue(id, out connection);
        }

        public bool SetConnection(Peer peer, int authToken)
        {
            if (Authenticated.TryGetValue(authToken, out int clientId))
            {
                Authenticated.TryRemove(authToken, out _);
                EnetConnection connection = new EnetConnection
                {
                    Peer = peer,
                    ClientId = clientId
                };

                Connections[peer.ID] = connection;
                ClientToPeerId[clientId] = peer.ID;
                RebuildConnectionsReadOnly();

                Logger.LogInformation("ConnectionManager.SetConnection clientId:{0}", clientId);
                return true;
            }

            return false;
        }

        public bool RemoveConnection(uint id)
        {
            if (Connections.TryGetValue(id, out var connection))
            {
                ClientToPeerId.TryRemove(connection.ClientId, out _);
                Connections.TryRemove(id, out _);
                RebuildConnectionsReadOnly();
                Logger.LogInformation("ConnectionManager.RemoveConnection clientId:{0}", connection.ClientId);
                return true;
            } else
            {
                return false;
            }
            
        }

        private void RebuildConnectionsReadOnly()
        {
            Dictionary<uint, EnetConnection> connections = new Dictionary<uint, EnetConnection>();
            foreach(uint id in Connections.Keys)
            {
                connections[id] = Connections[id];
            }
            ConnectionsReadOnly = connections;
        }
    }
}
