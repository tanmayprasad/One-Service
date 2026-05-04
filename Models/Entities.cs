using SQLite;
using System;

namespace P2PDaemon.Models
{
    public class LocalAccount
    {
        [PrimaryKey]
        public string MobileNumber { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string UniqueId { get; set; } // Ed25519 Public Key
        public byte[] EncryptedPrivateKey { get; set; }
    }

    public class Peer
    {
        [PrimaryKey]
        public string UniqueId { get; set; }
        [Unique]
        public string MobileNumber { get; set; }
        public string Name { get; set; }
        public bool IsTrusted { get; set; }
    }

    public class DataPayload
    {
        [PrimaryKey]
        public string PayloadId { get; set; }
        public string SenderId { get; set; }
        public string TargetId { get; set; }
        public byte[] EncryptedData { get; set; }
        public int HopCountTTL { get; set; }
        public string PayloadHash { get; set; }
        public DateTime Timestamp { get; set; }
    }
}