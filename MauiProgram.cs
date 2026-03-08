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

               builder.UseMauiApp<App>()

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
            builder.Services.AddTransient<KullaniciYonetimi>();
            builder.Services.AddTransient<UrunDuzenle>();
            builder.Services.AddSingleton<SepetServisi>();
            builder.Services.AddTransient<SepetSayfa>();
            builder.Services.AddSingleton<AyarlarServisi>();
            builder.Services.AddTransient<AyarlarSayfa>();
            builder.Services.AddTransient<FiyatGecmisiSayfa>();
            builder.Services.AddSingleton<App>();
            builder.Services.AddTransient<BarkodSayfa>();
#if DEBUG
            builder.Logging.AddDebug();
#endif

            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
#if ANDROID
    handler.PlatformView.BackgroundTintList =
        Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
    handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);

    // Tema kontrolü
    bool darkMode = (handler.PlatformView.Context?.Resources?.Configuration?.UiMode
        & Android.Content.Res.UiMode.NightMask) == Android.Content.Res.UiMode.NightYes;

    if (darkMode)
    {
        handler.PlatformView.SetTextColor(Android.Graphics.Color.White);
        handler.PlatformView.SetHintTextColor(Android.Graphics.Color.ParseColor("#9CA3AF"));
    }
    else
    {
        handler.PlatformView.SetTextColor(Android.Graphics.Color.ParseColor("#1A1A2E"));
        handler.PlatformView.SetHintTextColor(Android.Graphics.Color.ParseColor("#6B7280"));
    }
#endif
            });

            return builder.Build(); // ← bu en sonda
        }
    }
}