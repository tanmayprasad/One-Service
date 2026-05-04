using Com.P2p.Daemon.Ipc;
using System.Collections.Generic;
using P2PDaemon.Routing;
using P2PDaemon.Models;
using System.Security.Cryptography;

namespace P2PDaemon.Platforms.Android.Ipc
{
    public class P2PNetworkServiceStub : IP2PNetworkServiceStub
    {
        private readonly object _clientLock = new object();
        private readonly Dictionary<string, IDataReceiver> _registeredClients = new();
        private readonly GossipRoutingEngine _routingEngine;

        public P2PNetworkServiceStub(GossipRoutingEngine routingEngine)
        {
            _routingEngine = routingEngine;
            // Wire the callback from the routing engine back to the client apps
            _routingEngine.OnPayloadReceivedForLocalApp += HandleIncomingNetworkPayload;
        }

        public override int RegisterClient(string appId, IDataReceiver receiver)
        {
            lock (_clientLock)
            {
                _registeredClients[appId] = receiver;
            }
            return 1;
        }

        public override async void SendPayload(string targetUniqueId, byte[] payloadBytes)
        {
            // Construct a new payload originated by a local client app
            var payload = new DataPayload
            {
                PayloadId = Guid.NewGuid().ToString(),
                TargetId = targetUniqueId,
                // Note: Ensure your DataPayload model is updated to accept byte if it doesn't already
                EncryptedData = payloadBytes, 
                HopCountTTL = 10, // Set initial TTL limit for propagation
                PayloadHash = ComputeHash(payloadBytes), 
                Timestamp = DateTime.UtcNow
            };

            // Push to the gossip engine to broadcast to the P2P network
            await _routingEngine.PropagatePayloadAsync(payload);
        }

        public override async void SendGroupPayload(IList<string> targetIds, byte[] payloadBytes)
        {
            // TODO: Implement Message Layer Security (MLS) tree derivation here before routing
        }

        private void HandleIncomingNetworkPayload(DataPayload payload)
        {
            // Push data asynchronously back up to all registered client apps via AIDL callbacks
            lock (_clientLock)
            {
                foreach (var client in _registeredClients.Values)
                {
                    try
                    {
                        client.OnPayloadReceived(payload.SenderId, payload.EncryptedData); 
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to send to client app: {ex.Message}");
                    }
                }
            }
        }

        private string ComputeHash(byte[] data)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(data);
            return Convert.ToBase64String(hashBytes);
        }
    }
}