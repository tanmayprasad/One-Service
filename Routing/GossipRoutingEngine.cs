using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using P2PDaemon.Models;
using P2PDaemon.Data;

namespace P2PDaemon.Routing
{
    public class GossipRoutingEngine
    {
        // Thread-safe cache to prevent broadcast storms 
        private readonly ConcurrentDictionary<string, byte> _processedHashes = new();
        private readonly EncryptedDatabaseContext _dbContext;
        
        // Event to pass data up to the AIDL IPC layer
        public event Action<DataPayload>? OnPayloadReceivedForLocalApp;

        public GossipRoutingEngine(EncryptedDatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task PropagatePayloadAsync(DataPayload payload)
        {
            // 1. Cryptographic Deduplication Check
            if (_processedHashes.ContainsKey(payload.PayloadHash))
            {
                System.Diagnostics.Debug.WriteLine($"[Gossip] Duplicate payload {payload.PayloadHash} dropped.");
                return;
            }

            // 2. Cache the hash to prevent future loops
            _processedHashes.TryAdd(payload.PayloadHash, 1);

            // 3. Save payload to secure local storage via SQLCipher
            await _dbContext.SaveDataPayloadAsync(payload);

            // 4. Notify local third-party apps via AIDL callbacks
            OnPayloadReceivedForLocalApp?.Invoke(payload);

            // 5. Enforce Hop-Count Time-To-Live (TTL)
            if (payload.HopCountTTL <= 0)
            {
                System.Diagnostics.Debug.WriteLine($"[Gossip] Payload {payload.PayloadHash} reached TTL 0. Dropping.");
                return;
            }

            // Decrement TTL before forwarding
            payload.HopCountTTL -= 1;

            // 6. Broadcast to random subset of peers
            await BroadcastToPeersAsync(payload);
        }

        private async Task BroadcastToPeersAsync(DataPayload payload)
        {
            var jsonPayload = JsonSerializer.Serialize(payload);
            var bytes = Encoding.UTF8.GetBytes(jsonPayload);

            // TODO: In the networking phase, fetch active IP endpoints from your decentralized peer list 
            // and dispatch these bytes over raw TCP/UDP sockets or WebRTC data channels.
            System.Diagnostics.Debug.WriteLine($"[Gossip] Propagating payload {payload.PayloadHash} to peers with TTL {payload.HopCountTTL}.");
        }
    }
}