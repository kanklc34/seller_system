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

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            OturumServisi.AktiviteYenile();

            var magazaAdi = await _ayarlar.GetAsync("MagazaAdi", "");

            if (MagazaAdiLabel != null)
            {
                MagazaAdiLabel.Text = string.IsNullOrWhiteSpace(magazaAdi)
                    ? "ÖZ BİGA ET"
                    : magazaAdi.ToUpper();
            }

            var kullanici = OturumServisi.AktifKullanici;

            if (HosgeldinLabel != null)
            {
                HosgeldinLabel.Text = $"Hoş geldin, {kullanici?.KullaniciAdi}";
            }

            bool isPatron = kullanici?.Rol == "Patron" || kullanici?.KullaniciAdi?.ToLower() == "admin";
            bool isYonetici = isPatron || kullanici?.Rol == "Müdür";

            RaporlarBtn.IsVisible = isYonetici;
            AyarlarBtn.IsVisible = isYonetici;
            YonetimBolumu.IsVisible = isYonetici;
            OzetBilgiGrid.IsVisible = isYonetici;
            KullaniciYonetimiBtn.IsVisible = isPatron;
            SonIslemlerBolumu.IsVisible = isYonetici;
            VeresiyeBtn.IsVisible = true;

            // Stok Butonunu Sadece Yönetici ve Patron görsün
            if (StokBtn != null)
            {
                StokBtn.IsVisible = isYonetici;
            }

            UrunListesiAciklamasiniAyarla(isYonetici);

            var darkMode = await _ayarlar.GetAsync("DarkMode", "0");
            Application.Current!.UserAppTheme = darkMode == "1" ? AppTheme.Dark : AppTheme.Light;

            if (isYonetici)
                await VerileriDoldur();
        }

        protected override bool OnBackButtonPressed() => true;

        private async Task VerileriDoldur()
        {
            await _db.InitAsync();
            var bugun = DateTime.Today;

            var gunlukSayi = await _db.GunlukSatisSayisiAsync(bugun);
            var gunlukCiro = await _db.GunlukCiroAsync(bugun);
            GunlukSatisLabel.Text = gunlukSayi.ToString("N1");
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

        private async void VeresiyeDefteriClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            try
            {
                await Shell.Current.GoToAsync("VeresiyeDefteri");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Navigasyon Hatası", $"Sayfa açılamadı: {ex.Message}", "Tamam");
            }
        }

        private async void BarkodOkutClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            await Shell.Current.GoToAsync("//BarkodSayfa");
        }

        private async void ToptanSatisClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            try
            {
                await Shell.Current.GoToAsync("ToptanSatis");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Navigasyon Hatası", $"Sayfa açılamadı: {ex.Message}", "Tamam");
            }
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

        // YENİ EKLENEN STOK BUTONU OLAYI
        private async void StokClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            try
            {
                await Shell.Current.GoToAsync("StokYonetimi");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Stok Sayfası", "Stok sayfası henüz oluşturulmadı, yakında eklenecek!", "Tamam");
            }
        }

        private async void AyarlarClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            await Shell.Current.GoToAsync("//AyarlarSayfa");
        }

        private async void CikisClicked(object sender, EventArgs e)
        {
            bool onay = await DisplayAlert("Çıkış", "Oturumu kapatmak istiyor musunuz?", "Evet", "Hayır");
            if (!onay) return;
            OturumServisi.Cikis();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}