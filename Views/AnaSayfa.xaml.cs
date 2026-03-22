using Saller_System.Services;

namespace Saller_System.Views
{
    public partial class AnaSayfa : ContentPage
    {
        private readonly AyarlarServisi _ayarlar;
        private readonly DatabaseService _db;

        public AnaSayfa(AyarlarServisi ayarlar, DatabaseService db)
        {
            InitializeComponent();
            _ayarlar = ayarlar;
            _db = db;
            TarihLabel.Text = DateTime.Now.ToString("dd.MM.yyyy dddd",
                new System.Globalization.CultureInfo("tr-TR"));
        }

        // ----------------------------------------------------------------
        // Sayfa görünür olduğunda — zamanaşımı kontrolü + veri yenile
        // ----------------------------------------------------------------
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Zamanaşımı dolmuşsa login'e yönlendir
            if (await ZamanAsimKontrolAsync()) return;

            // Sayfa açılışı aktivite sayılır
            OturumServisi.AktiviteYenile();

            var kullanici = OturumServisi.AktifKullanici;
            HosgeldinLabel.Text = $"Hoş geldin, {kullanici?.KullaniciAdi}";

            // YETKİ KONTROLÜ
            bool isPatron = kullanici?.Rol == "Patron" || kullanici?.KullaniciAdi?.ToLower() == "admin";
            bool isYonetici = isPatron || kullanici?.Rol == "Müdür";

            // GÖRÜNÜRLÜK AYARLARI
            RaporlarBtn.IsVisible = isYonetici;
            AyarlarBtn.IsVisible = isYonetici;
            YonetimBolumu.IsVisible = isYonetici;
            OzetBilgiGrid.IsVisible = isYonetici;
            KullaniciYonetimiBtn.IsVisible = isPatron;
            SonIslemlerBolumu.IsVisible = isYonetici;

            UrunListesiAciklamasiniAyarla(isYonetici);

            var darkMode = await _ayarlar.GetAsync("DarkMode", "0");
            Application.Current!.UserAppTheme = darkMode == "1" ? AppTheme.Dark : AppTheme.Light;

            if (isYonetici)
                await VerileriDoldur();
        }

        // ----------------------------------------------------------------
        // Zamanaşımı kontrolü — dolmuşsa login'e yönlendir
        // ----------------------------------------------------------------
        private async Task<bool> ZamanAsimKontrolAsync()
        {
            if (!OturumServisi.OturumSuresiDolduMu()) return false;

            OturumServisi.Cikis();
            await DisplayAlert(
                "Oturum Süresi Doldu",
                "Güvenlik nedeniyle oturumunuz sonlandırıldı. Lütfen tekrar giriş yapın.",
                "Tamam"
            );
            await Shell.Current.GoToAsync("//LoginPage");
            return true;
        }

        // ----------------------------------------------------------------
        // Veri doldur
        // ----------------------------------------------------------------
        private async Task VerileriDoldur()
        {
            await _db.InitAsync();
            var bugun = DateTime.Today;

            var gunlukSayi = await _db.GunlukSatisSayisiAsync(bugun);
            var gunlukCiro = await _db.GunlukCiroAsync(bugun);

            GunlukSatisLabel.Text = gunlukSayi.ToString();
            GunlukCiroLabel.Text = $"₺{gunlukCiro:N0}";

            var bugunkuSatislar = await _db.GunlukSatislerAsync(bugun);
            var sonSatislar = bugunkuSatislar.OrderByDescending(s => s.Tarih).Take(4).ToList();
            BindableLayout.SetItemsSource(SonIslemlerListesi, sonSatislar);
        }

        private void UrunListesiAciklamasiniAyarla(bool isYonetici)
        {
            UrunListesiAciklamaLabel.Text = isYonetici
                ? "Ürünleri görüntüle, ekle ve fiyatları yönet"
                : "Kayıtlı ürün fiyatlarını görüntüle";
        }

        // ----------------------------------------------------------------
        // Navigasyon — her tıklamada aktivite yenilenir
        // ----------------------------------------------------------------
        private async void BarkodOkutClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            await Shell.Current.GoToAsync("//BarkodSayfa");
        }

        private async void UrunListesiClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            await Shell.Current.GoToAsync("//UrunListesi");
        }

        private async void RaporlarClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            await Shell.Current.GoToAsync("//Raporlar");
        }

        private async void KullaniciYonetimiClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            await Shell.Current.GoToAsync("//KullaniciYonetimi");
        }

        private async void AyarlarClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            await Shell.Current.GoToAsync("//AyarlarSayfa");
        }

        private async void CikisClicked(object sender, EventArgs e)
        {
            OturumServisi.Cikis();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}