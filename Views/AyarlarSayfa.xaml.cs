using Saller_System.Services;

namespace Saller_System.Views
{
    public partial class AyarlarSayfa : ContentPage
    {
        private readonly AyarlarServisi _ayarlar;
        private string? _algılananPrefix;

        public AyarlarSayfa(AyarlarServisi ayarlar)
        {
            InitializeComponent();
            _ayarlar = ayarlar;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            KayitliPrefixLabel.Text = await _ayarlar.GetAsync("TaraziPrefix", "Tanımsız");
            MagazaAdiEntry.Text = await _ayarlar.GetAsync("MagazaAdi", "");
            TelefonEntry.Text = await _ayarlar.GetAsync("Telefon", "");
        }

        private void PrefixAlgilaClicked(object sender, EventArgs e)
        {
            string barkod = TestBarkodEntry.Text?.Trim() ?? "";

            if (barkod.Length != 13)
            {
                PrefixLabel.Text = "❌ Geçersiz barkod (13 hane olmalı)";
                PrefixLabel.TextColor = Colors.Red;
                PrefixKaydetBtn.IsVisible = false;
                return;
            }

            _algılananPrefix = TartiServisi.PrefixAlgila(barkod);

            if (_algılananPrefix != null)
            {
                PrefixLabel.Text = $"✅ '{_algılananPrefix}' algılandı";
                PrefixLabel.TextColor = Colors.Green;
                PrefixKaydetBtn.IsVisible = true;
            }
            else
            {
                PrefixLabel.Text = "❌ Prefix algılanamadı";
                PrefixLabel.TextColor = Colors.Red;
                PrefixKaydetBtn.IsVisible = false;
            }
        }

        private async void PrefixKaydetClicked(object sender, EventArgs e)
        {
            if (_algılananPrefix == null) return;
            await _ayarlar.SetAsync("TaraziPrefix", _algılananPrefix);
            KayitliPrefixLabel.Text = _algılananPrefix;
            await DisplayAlert("Başarılı", $"Prefix '{_algılananPrefix}' kaydedildi!", "Tamam");
        }

        private async void BilgileriKaydetClicked(object sender, EventArgs e)
        {
            await _ayarlar.SetAsync("MagazaAdi", MagazaAdiEntry.Text?.Trim() ?? "");
            await _ayarlar.SetAsync("Telefon", TelefonEntry.Text?.Trim() ?? "");
            await DisplayAlert("Başarılı", "Bilgiler kaydedildi!", "Tamam");
        }

        private async void GeriClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//AnaSayfa");
    }
}