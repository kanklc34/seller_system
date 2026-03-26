using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;
using Saller_System.Services;
using Saller_System.Views;
using SkiaSharp.Views.Maui.Controls.Hosting; // DÜZELTİLDİ: .Hosting eklendi
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
                .UseSkiaSharp() // Grafik motoru artık aktif
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // --- SERVİSLER ---
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<AyarlarServisi>();
            builder.Services.AddSingleton<SepetServisi>();
            builder.Services.AddSingleton<ExcelServisi>();
            builder.Services.AddSingleton(AudioManager.Current);
            builder.Services.AddSingleton<App>();

            // --- SAYFALAR ---
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
            builder.Services.AddTransient<VeresiyeDefteri>();
            builder.Services.AddTransient<MusteriEkstresi>();
            builder.Services.AddTransient<ToptanSatis>();
            builder.Services.AddTransient<StokYonetimi>();
            builder.Services.AddTransient<GiderlerSayfa>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
                handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);

                bool isDarkMode = (handler.PlatformView.Context?.Resources?.Configuration?.UiMode
                    & Android.Content.Res.UiMode.NightMask) == Android.Content.Res.UiMode.NightYes;

                if (isDarkMode)
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