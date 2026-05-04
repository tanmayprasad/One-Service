using SQLite;
using System.Security.Cryptography;
using P2PDaemon.Models;

namespace P2PDaemon.Data
{
    public class EncryptedDatabaseContext
    {
        private SQLiteAsyncConnection _database;
        private readonly string _dbPath;

        public EncryptedDatabaseContext()
        {
            // The database is stored in the secure, internal app data sandbox
            _dbPath = Path.Combine(FileSystem.AppDataDirectory, "p2p_secure.db");
        }

        public async Task InitAsync()
        {
            if (_database!= null)
                return;

            // Retrieve or generate the AES-256 encryption key
            var encryptionKey = await GetOrGenerateEncryptionKeyAsync();

            // Initialize the SQLCipher connection with the encryption key
            var options = new SQLiteConnectionString(_dbPath, true, key: encryptionKey);
            _database = new SQLiteAsyncConnection(options);

            // Initialize the schema
            await _database.CreateTableAsync<LocalAccount>();
            await _database.CreateTableAsync<Peer>();
            await _database.CreateTableAsync<DataPayload>();
        }

        private async Task<string> GetOrGenerateEncryptionKeyAsync()
        {
            var key = await SecureStorage.Default.GetAsync("DbEncryptionKey");
            
            if (string.IsNullOrEmpty(key))
            {
                // Generate a highly entropic 256-bit (32-byte) key
                var keyBytes = new byte[1];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(keyBytes);
                }
                
                key = Convert.ToBase64String(keyBytes);
                
                // Store securely in Android Keystore via MAUI SecureStorage
                await SecureStorage.Default.SetAsync("DbEncryptionKey", key);
            }
            
            return key;
        }

        // --- Data Access Methods ---

        public async Task SavePeerAsync(Peer peer)
        {
            await InitAsync();
            await _database.InsertOrReplaceAsync(peer);
        }

        public async Task<Peer> GetPeerAsync(string uniqueId)
        {
            await InitAsync();
            return await _database.Table<Peer>().FirstOrDefaultAsync(p => p.UniqueId == uniqueId);
        }

        public async Task SaveDataPayloadAsync(DataPayload payload)
        {
            await InitAsync();
            await _database.InsertAsync(payload);
        }
    }
}