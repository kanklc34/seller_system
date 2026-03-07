using Microsoft.Extensions.Logging;
using Saller_System.Services;
using Saller_System.Views;
using ZXing.Net.Maui.Controls;

namespace Saller_System
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseBarcodeReader()  // barkod okuyucu
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // DatabaseService'i uygulamaya tanıt
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddTransient<UrunEkle>();
            builder.Services.AddSingleton<ExcelServisi>();
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}