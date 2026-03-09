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

        protected override void OnAppearing()
        {
            base.OnAppearing();

            HosgeldinLabel.Text = $"Hoş geldiniz, {OturumServisi.AktifKullanici?.KullaniciAdi}";

            OzetBilgiGrid.IsVisible = OturumServisi.YoneticiMi;
            RaporlarBtn.IsVisible = OturumServisi.YoneticiMi;
            KullaniciYonetimiBtn.IsVisible = OturumServisi.AdminMi;
            AyarlarBtn.IsVisible = OturumServisi.CalisanMi;
            YonetimBolumu.IsVisible = OturumServisi.YoneticiMi;

            UrunListesiAciklamasiniAyarla();
        }

        private void UrunListesiAciklamasiniAyarla()
        {
            if (OturumServisi.YoneticiMi)
            {
                UrunListesiAciklamaLabel.Text = "Kayıtlı ürünleri görüntüle ve yönet";
            }
            else
            {
                UrunListesiAciklamaLabel.Text = "Kayıtlı ürünleri görüntüle";
            }
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