using Saller_System.Services;

namespace Saller_System.Views
{
    public partial class AnaSayfa : ContentPage
    {
        private readonly AyarlarServisi _ayarlar;

        public AnaSayfa(AyarlarServisi ayarlar)
        {
            InitializeComponent();
            _ayarlar = ayarlar;
        }

        private bool _temaYukleniyor = false;

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _temaYukleniyor = true;
            HosgeldinLabel.Text = $"Hoş geldiniz, {OturumServisi.AktifKullanici?.KullaniciAdi} 👋";
            RaporlarBtn.IsVisible = OturumServisi.YoneticiMi;
            KullaniciYonetimiBtn.IsVisible = OturumServisi.YoneticiMi;
            AyarlarBtn.IsVisible = OturumServisi.YoneticiMi;

            var darkMode = await _ayarlar.GetAsync("DarkMode", "0");
            DarkModeSwitch.IsToggled = darkMode == "1";
            Application.Current!.UserAppTheme = darkMode == "1" ? AppTheme.Dark : AppTheme.Light;
            _temaYukleniyor = false;
        }

        private async void DarkModeToggled(object sender, ToggledEventArgs e)
        {
            if (_temaYukleniyor) return;
            Application.Current!.UserAppTheme = e.Value ? AppTheme.Dark : AppTheme.Light;
            await _ayarlar.SetAsync("DarkMode", e.Value ? "1" : "0");
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