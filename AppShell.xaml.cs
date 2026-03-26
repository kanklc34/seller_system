using Microsoft.Maui.Controls;
using Saller_System.Views;

namespace Saller_System
{
    public partial class AppShell : Shell
    {
        // Her sayfanın geri tuşunda nereye gideceği
        private static readonly Dictionary<string, string> GeriMap = new()
        {
            { "BarkodSayfa",        "//AnaSayfa" },
            { "UrunListesi",        "//AnaSayfa" },
            { "Raporlar",           "//AnaSayfa" },
            { "KullaniciYonetimi",  "//AnaSayfa" },
            { "AyarlarSayfa",       "//AnaSayfa" },
            { "SepetSayfa",         "//BarkodSayfa" },
            { "UrunEkle",           "//UrunListesi" },
            { "UrunDuzenle",        "//UrunListesi" },
            { "FiyatGecmisiSayfa",  "//UrunListesi" },
            { "SatisGecmisiSayfa",  "//Raporlar" },
            { "VeresiyeDefteri",    "//AnaSayfa" },
            { "ToptanSatis",        "//AnaSayfa" },
            { "StokYonetimi",       "//AnaSayfa" },
            { "MusteriEkstresi",    "//AnaSayfa" },
            { "GiderlerSayfa",      "//Raporlar" } // YENİ EKLENDİ: Giderden Raporlara döner
        };

        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("VeresiyeDefteri", typeof(VeresiyeDefteri));
            Routing.RegisterRoute("ToptanSatis", typeof(ToptanSatis));
            Routing.RegisterRoute("StokYonetimi", typeof(StokYonetimi));
            Routing.RegisterRoute("MusteriEkstresi", typeof(MusteriEkstresi));

            // ÇÖKMEYİ ENGELLEYEN SATIR:
            Routing.RegisterRoute("GiderlerSayfa", typeof(GiderlerSayfa));
        }

        protected override bool OnBackButtonPressed()
        {
            var location = Shell.Current.CurrentState.Location.ToString();

            if (location.Contains("KurulumSihirbazi") || location.Contains("SplashSayfa"))
                return base.OnBackButtonPressed();

            if (location.EndsWith("LoginPage") || location.EndsWith("AnaSayfa"))
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    bool cikis = await DisplayAlert(
                        "Çıkış", "Uygulamadan çıkmak istiyor musunuz?", "Evet", "Hayır");
                    if (cikis)
                        Application.Current?.Quit();
                });
                return true;
            }

            var hedef = GeriMap.FirstOrDefault(k => location.Contains(k.Key)).Value;
            if (hedef != null)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                    await Shell.Current.GoToAsync(hedef));
                return true;
            }

            MainThread.BeginInvokeOnMainThread(async () =>
                await Shell.Current.GoToAsync("//AnaSayfa"));
            return true;
        }
    }
}