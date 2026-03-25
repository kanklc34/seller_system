using Microsoft.Maui.Controls;
using Saller_System.Views; // KIRMIZI HATALARI ÇÖZECEK KRİTİK SATIR

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
            
            // YENİ EKLENENLER:
            { "ToptanSatis",        "//AnaSayfa" },
            { "StokYonetimi",       "//AnaSayfa" }
        };

        public AppShell()
        {
            InitializeComponent();

            // ÇÖKMEYİ ENGELLEYEN KRİTİK ROTA KAYITLARI BURADA:
            // "Views." takısını kaldırdık, çünkü yukarıya using ile ekledik.
            Routing.RegisterRoute("VeresiyeDefteri", typeof(VeresiyeDefteri));
            Routing.RegisterRoute("ToptanSatis", typeof(ToptanSatis));
            Routing.RegisterRoute("StokYonetimi", typeof(StokYonetimi));
        }

        protected override bool OnBackButtonPressed()
        {
            var location = Shell.Current.CurrentState.Location.ToString();

            // Kurulum ve splash — normal davran
            if (location.Contains("KurulumSihirbazi") || location.Contains("SplashSayfa"))
                return base.OnBackButtonPressed();

            // Login ve Ana sayfa — uygulamadan çık
            if (location.Contains("LoginPage") || location.Contains("AnaSayfa"))
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

            // Diğer sayfalar — map'e göre git
            var hedef = GeriMap.FirstOrDefault(k => location.Contains(k.Key)).Value;
            if (hedef != null)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                    await Shell.Current.GoToAsync(hedef));
                return true;
            }

            // Map'te yoksa ana sayfaya
            MainThread.BeginInvokeOnMainThread(async () =>
                await Shell.Current.GoToAsync("//AnaSayfa"));
            return true;
        }
    }
}