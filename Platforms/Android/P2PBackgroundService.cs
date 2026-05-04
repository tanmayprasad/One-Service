using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using P2PDaemon.Platforms.Android.Ipc;
using P2PDaemon.Routing;
using P2PDaemon.Data;

namespace P2PDaemon.Platforms.Android
{
    [Service(Name = "crc64.P2PBackgroundService", Exported = true)]
    [IntentFilter(new[] { "com.p2p.daemon.ipc.BIND_SERVICE" })]
    public class P2PBackgroundService : Service
    {
        private P2PNetworkServiceStub _ipcBinder;
        private GossipRoutingEngine _routingEngine;

        public override void OnCreate()
        {
            base.OnCreate();

            // Resolve the SQLCipher database context via DI
            var dbContext = IPlatformApplication.Current.Services.GetService<EncryptedDatabaseContext>();
            
            // Instantiate the Gossip Engine
            _routingEngine = new GossipRoutingEngine(dbContext);
            
            // Inject the routing engine into the thread-safe AIDL stub
            _ipcBinder = new P2PNetworkServiceStub(_routingEngine);
        }

        public override IBinder OnBind(Intent intent)
        {
            return _ipcBinder;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            CreateNotificationChannel();

            var notification = new NotificationCompat.Builder(this, "P2P_SERVICE_CHANNEL")
             .SetContentTitle("P2P Network Daemon")
             .SetContentText("Gossip protocol active. Listening for payloads...")
             .SetOngoing(true)
             .Build();

            // Escalate to Foreground Service to bypass Android 15 restrictions
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                StartForeground(1001, notification, ForegroundService.TypeRemoteMessaging);
            }
            else
            {
                StartForeground(1001, notification);
            }

            // TODO: Start UDP/TCP network socket listeners here 
            // When data arrives over the network, pass it to _routingEngine.PropagatePayloadAsync()

            return StartCommandResult.Sticky;
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel("P2P_SERVICE_CHANNEL", "P2P Daemon", NotificationImportance.Low);
                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager?.CreateNotificationChannel(channel);
            }
        }
    }
}