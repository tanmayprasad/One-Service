using Android.App;
using Android.Content;
using Android.OS;
using Android.Telephony;
using P2PDaemon.Data;
using P2PDaemon.Models;

namespace P2PDaemon.Platforms.Android
{
   
    [IntentFilter(new[] { "android.provider.Telephony.SMS_RECEIVED" }, Priority = 999)]
    public class SmsHandshakeReceiver : BroadcastReceiver
    {
        public override async void OnReceive(Context context, Intent intent)
        {
            if (intent.Action!= "android.provider.Telephony.SMS_RECEIVED") return;

            var bundle = intent.Extras;
            if (bundle == null) return;

            var pdus = bundle.Get("pdus") as Java.Lang.Object;
            if (pdus == null) return;

            string senderNumber = string.Empty;
            string fullMessage = string.Empty;

            foreach (Java.Lang.Object pdu in (Java.Lang.Object[])pdus)
            {
                var bytes = (byte[])pdu;
                global::Android.Telephony.SmsMessage message;
                
                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    string format = bundle.GetString("format");
                    message = global::Android.Telephony.SmsMessage.CreateFromPdu(bytes, format);
                }
                else
                {
                    message = global::Android.Telephony.SmsMessage.CreateFromPdu(bytes);
                }

                senderNumber = message.OriginatingAddress;
                fullMessage += message.MessageBody;
            }

            // Filter for our P2P Handshake Scheme
            if (fullMessage.StartsWith("p2p-app://handshake?data="))
            {
                string peerUniqueId = fullMessage.Replace("p2p-app://handshake?data=", "");
                await ProcessIncomingHandshakeAsync(senderNumber, peerUniqueId);
            }
        }

        private async Task ProcessIncomingHandshakeAsync(string mobileNumber, string uniqueId)
        {
            try
            {
                // In a production environment, this dependency would be resolved via DI
                var dbContext = IPlatformApplication.Current.Services.GetService<EncryptedDatabaseContext>();
                
                var newPeer = new Peer
                {
                    UniqueId = uniqueId,
                    MobileNumber = mobileNumber,
                    IsTrusted = true
                };

                await dbContext.SavePeerAsync(newPeer);
                System.Diagnostics.Debug.WriteLine($" Successfully registered peer {mobileNumber} with Key {uniqueId}");
                
                // TODO: Optionally trigger the P2PSmsManager to send a reciprocal acceptance SMS back
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" DB Write Failed: {ex.Message}");
            }
        }
    }
}