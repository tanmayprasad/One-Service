using Android.Content;
using Android.Net;
using Android.Provider;
using Application = Android.App.Application;

namespace P2PDaemon.Platforms.Android
{
    public static class PermissionHelper
    {
        public static void PromptForRestrictedSmsSettings()
        {
            // Create an intent to open the specific App Info page for this application
            var intent = new Intent(Settings.ActionApplicationDetailsSettings);
            var uri = global::Android.Net.Uri.FromParts("package", Application.Context.PackageName, null);
            intent.SetData(uri);
            intent.AddFlags(ActivityFlags.NewTask);

            // Instruct the user on how to bypass the Android 15 restriction
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert(
                    "SMS Permission Required",
                    "To enable P2P handshakes, please tap the 3-dot menu (⋮) in the top right, select 'Allow restricted settings', and then grant the SMS permission.",
                    "Open Settings");

                Application.Context.StartActivity(intent);
            });
        }

        public static bool HasSmsPermissions()
        {
            var sendStatus = global::AndroidX.Core.Content.ContextCompat.CheckSelfPermission(global::Android.App.Application.Context, global::Android.Manifest.Permission.SendSms);
            var receiveStatus = global::AndroidX.Core.Content.ContextCompat.CheckSelfPermission(global::Android.App.Application.Context, global::Android.Manifest.Permission.ReceiveSms);
            
            return sendStatus == global::Android.Content.PM.Permission.Granted && receiveStatus == global::Android.Content.PM.Permission.Granted;
        }
    }
}