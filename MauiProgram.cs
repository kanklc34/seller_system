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
                .UseBarcodeReader()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Singleton servisler
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<AyarlarServisi>();
            builder.Services.AddSingleton<SepetServisi>();
            builder.Services.AddSingleton<ExcelServisi>();
            builder.Services.AddSingleton<App>();

            // Sayfalar
            builder.Services.AddTransient<SplashSayfa>();
            builder.Services.AddTransient<KurulumSihirbazi>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<AnaSayfa>();
            builder.Services.AddTransient<BarkodSayfa>();
            builder.Services.AddTransient<SepetSayfa>();
            builder.Services.AddTransient<UrunListesi>();
            builder.Services.AddTransient<UrunEkle>();
            builder.Services.AddTransient<UrunDuzenle>();
            builder.Services.AddTransient<Raporlar>();
            builder.Services.AddTransient<KullaniciYonetimi>();
            builder.Services.AddTransient<AyarlarSayfa>();
            builder.Services.AddTransient<FiyatGecmisiSayfa>();
            builder.Services.AddTransient<SatisGecmisiSayfa>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Android entry görünümü
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
                handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);

                bool darkMode = (handler.PlatformView.Context?.Resources?.Configuration?.UiMode
                    & Android.Content.Res.UiMode.NightMask) == Android.Content.Res.UiMode.NightYes;

                if (darkMode)
                {
                    handler.PlatformView.SetTextColor(Android.Graphics.Color.White);
                    handler.PlatformView.SetHintTextColor(Android.Graphics.Color.ParseColor("#9CA3AF"));
                }
                else
                {
                    handler.PlatformView.SetTextColor(Android.Graphics.Color.ParseColor("#0F172A"));
                    handler.PlatformView.SetHintTextColor(Android.Graphics.Color.ParseColor("#94A3B8"));
                }
#endif
            });

            return builder.Build();
        }
    }
}