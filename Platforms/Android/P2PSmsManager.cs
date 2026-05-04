using Android.Telephony;

namespace P2PDaemon.Platforms.Android
{
    public class P2PSmsManager
    {
        public void SendHandshake(string targetMobileNumber, string localUniqueId)
        {
            if (!PermissionHelper.HasSmsPermissions())
            {
                PermissionHelper.PromptForRestrictedSmsSettings();
                return;
            }

            try
            {
                // Prefix with our specific deep-link URI to distinguish from regular SMS
                string payload = $"p2p-app://handshake?data={localUniqueId}";

                SmsManager smsManager = SmsManager.Default;
                smsManager.SendTextMessage(targetMobileNumber, null, payload, null, null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" Failed to dispatch handshake: {ex.Message}");
            }
        }
    }
}