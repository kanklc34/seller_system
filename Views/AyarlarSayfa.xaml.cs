using Saller_System.Services;

namespace Saller_System.Views
{
    public partial class AyarlarSayfa : ContentPage
    {
        private readonly AyarlarServisi _ayarlar;
        private string? _algılananPrefix;
        private bool _temaYukleniyor = false;

        public AyarlarSayfa(AyarlarServisi ayarlar)
        {
            InitializeComponent();
            _ayarlar = ayarlar;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (await ZamanAsimKontrolAsync()) return;

            OturumServisi.AktiviteYenile();

            _temaYukleniyor = true;
            var darkMode = await _ayarlar.GetAsync("DarkMode", "0");
            DarkModeSwitch.IsToggled = darkMode == "1";
            Application.Current!.UserAppTheme = darkMode == "1" ? AppTheme.Dark : AppTheme.Light;
            _temaYukleniyor = false;

            KayitliPrefixLabel.Text = await _ayarlar.GetAsync("TaraziPrefix", "Tanımsız");
            MagazaAdiEntry.Text = await _ayarlar.GetAsync("MagazaAdi", "");
            TelefonEntry.Text = await _ayarlar.GetAsync("Telefon", "");
        }

        private async Task<bool> ZamanAsimKontrolAsync()
        {
            if (!OturumServisi.OturumSuresiDolduMu()) return false;

            OturumServisi.Cikis();
            await DisplayAlert("Oturum Süresi Doldu", "Güvenlik nedeniyle oturumunuz sonlandırıldı.", "Tamam");
            await Shell.Current.GoToAsync("//LoginPage");
            return true;
        }

        private async void DarkModeToggled(object sender, ToggledEventArgs e)
        {
            if (_temaYukleniyor) return;

            OturumServisi.AktiviteYenile();
            Application.Current!.UserAppTheme = e.Value ? AppTheme.Dark : AppTheme.Light;
            await _ayarlar.SetAsync("DarkMode", e.Value ? "1" : "0");
        }

        private void PrefixAlgilaClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
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

            OturumServisi.AktiviteYenile();
            await _ayarlar.SetAsync("TaraziPrefix", _algılananPrefix);
            KayitliPrefixLabel.Text = _algılananPrefix;
            await DisplayAlert("Başarılı", $"Prefix '{_algılananPrefix}' kaydedildi!", "Tamam");
        }
        private async void ArkaPlanResmiSecClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            try
            {
                // 1. Telefonun galerisini açar
                var result = await MediaPicker.Default.PickPhotoAsync();

                if (result != null)
                {
                    // 🔥 KRİTİK NOKTA: Sabit yazıyı sildik, seçilen resmin gerçek yolunu kaydediyoruz.
                    // Bu yol şuna benzer: "/storage/emulated/0/DCIM/Camera/resim.jpg"
                    await _ayarlar.SetAsync("DukkanArkaPlan", result.FullPath);

                    await DisplayAlert("Başarılı", "Dükkan görseli güncellendi! Değişiklikleri görmek için sayfaları yenileyin.", "Tamam");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", "Görsel seçilemedi: " + ex.Message, "Tamam");
            }
        }

        private async void VarsayilanResmeDonClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            // "DukkanArkaPlanPath" olan ismi "DukkanArkaPlan" olarak güncelledik
            await _ayarlar.SetAsync("DukkanArkaPlan", "dukkan_fotogece.jpg");
            await DisplayAlert("Bilgi", "Varsayılan dükkan görseline dönüldü.", "Tamam");
        }
        private async void BilgileriKaydetClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            await _ayarlar.SetAsync("MagazaAdi", MagazaAdiEntry.Text?.Trim() ?? "");
            await _ayarlar.SetAsync("Telefon", TelefonEntry.Text?.Trim() ?? "");
            await DisplayAlert("Başarılı", "Bilgiler kaydedildi!", "Tamam");
        }

        private async void GeriClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            await Shell.Current.GoToAsync("//AnaSayfa");
        }
    }
}