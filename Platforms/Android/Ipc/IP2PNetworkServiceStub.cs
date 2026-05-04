using System;
using System.Collections.Generic;
using Android.OS;
using Android.Runtime;

namespace Com.P2p.Daemon.Ipc
{
    public interface IDataReceiver : IInterface
    {
        void OnPayloadReceived(string senderUniqueId, byte[] payload);
    }

    public abstract class IDataReceiverStub : Binder, IDataReceiver
    {
        public IDataReceiverStub()
        {
            this.AttachInterface(this, "com.p2p.daemon.ipc.IDataReceiver");
        }

        public abstract void OnPayloadReceived(string senderUniqueId, byte[] payload);

        public IBinder AsBinder()
        {
            return this;
        }

        protected override bool OnTransact(int code, Parcel data, Parcel reply, int flags)
        {
            if (code == 1) // Transaction code for onPayloadReceived
            {
                data.EnforceInterface("com.p2p.daemon.ipc.IDataReceiver");
                string senderUniqueId = data.ReadString();
                byte[] payload = data.CreateByteArray();
                this.OnPayloadReceived(senderUniqueId, payload);
                return true;
            }
            return base.OnTransact(code, data, reply, flags);
        }
    }

    public interface IP2PNetworkService : IInterface
    {
        int RegisterClient(string appId, IDataReceiver receiver);
        void SendPayload(string targetUniqueId, byte[] payload);
        void SendGroupPayload(IList<string> targetIds, byte[] payload);
    }

    public abstract class IP2PNetworkServiceStub : Binder, IP2PNetworkService
    {
        public IP2PNetworkServiceStub()
        {
            this.AttachInterface(this, "com.p2p.daemon.ipc.IP2PNetworkService");
        }

        public abstract int RegisterClient(string appId, IDataReceiver receiver);
        public abstract void SendPayload(string targetUniqueId, byte[] payload);
        public abstract void SendGroupPayload(IList<string> targetIds, byte[] payload);

        public IBinder AsBinder()
        {
            return this;
        }

        protected override bool OnTransact(int code, Parcel data, Parcel reply, int flags)
        {
            data.EnforceInterface("com.p2p.daemon.ipc.IP2PNetworkService");

            if (code == 1) // RegisterClient
            {
                string appId = data.ReadString();
                // We're omitting full IDataReceiver proxy deserialization here for mock brevity
                int result = this.RegisterClient(appId, null);
                reply.WriteNoException();
                reply.WriteInt(result);
                return true;
            }
            if (code == 2) // SendPayload
            {
                string targetUniqueId = data.ReadString();
                byte[] payload = data.CreateByteArray();
                this.SendPayload(targetUniqueId, payload);
                return true;
            }
            if (code == 3) // SendGroupPayload
            {
                var targetIds = new List<string>();
                data.ReadStringList(targetIds);
                byte[] payload = data.CreateByteArray();
                this.SendGroupPayload(targetIds, payload);
                return true;
            }

            return base.OnTransact(code, data, reply, flags);
        }
    }
}
