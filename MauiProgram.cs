using Microsoft.Extensions.Logging;
using P2PDaemon.Data; // Add this using directive

namespace P2PDaemon
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            // Register the Encrypted Database Context
            builder.Services.AddSingleton<EncryptedDatabaseContext>();

            return builder.Build();
        }
    }
}