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

            TarihLabel.Text = DateTime.Now.ToString("dd.MM.yyyy dddd", new System.Globalization.CultureInfo("tr-TR"));
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var kullanici = OturumServisi.AktifKullanici;
            HosgeldinLabel.Text = $"Hoş geldin, {kullanici?.KullaniciAdi}";

            // YETKİ KONTROLÜ
            bool isPatron = kullanici?.Rol == "Patron" || kullanici?.KullaniciAdi?.ToLower() == "admin";
            bool isYonetici = isPatron || kullanici?.Rol == "Müdür";

            // GÖRÜNÜRLÜK AYARLARI (GİZLİLİK KURALLARI)
            RaporlarBtn.IsVisible = isYonetici;
            AyarlarBtn.IsVisible = isYonetici;
            YonetimBolumu.IsVisible = isYonetici;
            OzetBilgiGrid.IsVisible = isYonetici; // Üst ciro kartları
            KullaniciYonetimiBtn.IsVisible = isPatron;

            // PERSONEL FİYATLARI TOPLAYAMASIN DİYE SON İŞLEMLERİ DE KAPATIYORUZ
            SonIslemlerBolumu.IsVisible = isYonetici;

            UrunListesiAciklamasiniAyarla(isYonetici);

            var darkMode = await _ayarlar.GetAsync("DarkMode", "0");
            Application.Current!.UserAppTheme = darkMode == "1" ? AppTheme.Dark : AppTheme.Light;

            // Sadece yöneticiyse verileri doldur (Veritabanı güvenliği)
            if (isYonetici)
            {
                await VerileriDoldur();
            }
        }

        private async Task VerileriDoldur()
        {
            await _db.InitAsync();
            var bugun = DateTime.Today;

            // Ciro ve adet güncellemesi
            var gunlukSayi = await _db.GunlukSatisSayisiAsync(bugun);
            var gunlukCiro = await _db.GunlukCiroAsync(bugun);
            GunlukSatisLabel.Text = gunlukSayi.ToString();
            GunlukCiroLabel.Text = $"₺{gunlukCiro:N0}";

            // SON İŞLEMLERİ (SATIŞLARI) LİSTEYE ÇEK
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

        private async void BarkodOkutClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//BarkodSayfa");
        private async void UrunListesiClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//UrunListesi");
        private async void RaporlarClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//Raporlar");
        private async void KullaniciYonetimiClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//KullaniciYonetimi");
        private async void AyarlarClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//AyarlarSayfa");

        private async void CikisClicked(object sender, EventArgs e)
        {
            OturumServisi.AktifKullanici = null;
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}