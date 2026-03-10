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
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            HosgeldinLabel.Text = $"Hoş geldiniz, {OturumServisi.AktifKullanici?.KullaniciAdi}";

            // Rol bazlı görünürlük
            RaporlarBtn.IsVisible = OturumServisi.YoneticiMi;
            KullaniciYonetimiBtn.IsVisible = OturumServisi.AdminMi;
            AyarlarBtn.IsVisible = OturumServisi.YoneticiMi;
            YonetimBolumu.IsVisible = OturumServisi.YoneticiMi;
            OzetBilgiGrid.IsVisible = OturumServisi.YoneticiMi;

            UrunListesiAciklamasiniAyarla();

            // Dark mode
            var darkMode = await _ayarlar.GetAsync("DarkMode", "0");
            Application.Current!.UserAppTheme = darkMode == "1" ? AppTheme.Dark : AppTheme.Light;

            // Özet kartları doldur
            if (OturumServisi.YoneticiMi)
                await OzetKartlariniDoldur();
        }

        private async Task OzetKartlariniDoldur()
        {
            await _db.InitAsync();
            var gunlukSayi = await _db.GunlukSatisSayisiAsync(DateTime.Today);
            var gunlukCiro = await _db.GunlukCiroAsync(DateTime.Today);
            GunlukSatisLabel.Text = gunlukSayi.ToString();
            GunlukCiroLabel.Text = $"₺{gunlukCiro:N0}";
        }

        private void UrunListesiAciklamasiniAyarla()
        {
            UrunListesiAciklamaLabel.Text = OturumServisi.YoneticiMi
                ? "Kayıtlı ürünleri görüntüle ve yönet"
                : "Kayıtlı ürünleri görüntüle";
        }

        private async void BarkodOkutClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//BarkodSayfa");
        private async void UrunListesiClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//UrunListesi");
        private async void RaporlarClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//Raporlar");
        private async void KullaniciYonetimiClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//KullaniciYonetimi");
        private async void AyarlarClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//AyarlarSayfa");
        private async void CikisClicked(object sender, EventArgs e)
        {
            OturumServisi.AktifKullanici = null;
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}