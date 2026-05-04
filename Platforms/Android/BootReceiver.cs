using Android.App;
using Android.Content;
using Android.OS;

namespace P2PDaemon.Platforms.Android
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[] { Intent.ActionBootCompleted, Intent.ActionLockedBootCompleted })]
    public class BootReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == Intent.ActionBootCompleted || intent.Action == Intent.ActionLockedBootCompleted)
            {
                var serviceIntent = new Intent(context, typeof(P2PBackgroundService));
                
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    context.StartForegroundService(serviceIntent);
                }
                else
                {
                    context.StartService(serviceIntent);
                }
            }
        }
    }
}