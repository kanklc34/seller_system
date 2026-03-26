using Saller_System.Models;
using Saller_System.Services;
using ZXing.Net.Maui;
using Microsoft.Maui.ApplicationModel;
using Plugin.Maui.Audio;

namespace Saller_System.Views
{
    public partial class BarkodSayfa : ContentPage
    {
        private readonly DatabaseService _db;
        private readonly SepetServisi _sepet;
        private readonly AyarlarServisi _ayarlar;
        private readonly IAudioManager _audioManager;
        private Urun? _bulunanUrun;

        public BarkodSayfa(DatabaseService db, SepetServisi sepet, AyarlarServisi ayarlar, IAudioManager audioManager)
        {
            InitializeComponent();
            _db = db;
            _sepet = sepet;
            _ayarlar = ayarlar;
            _audioManager = audioManager;

            BarkodOkuyucu.Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormat.Ean13 | BarcodeFormat.Ean8 | BarcodeFormat.Code128 | BarcodeFormat.QrCode,
                AutoRotate = true,
                Multiple = false
            };
        }

        private async Task BipCal()
        {
            try
            {
                var player = _audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("bip.wav"));
                player.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ses Çalma Hatası: {ex.Message}");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (await ZamanAsimKontrolAsync()) return;
            OturumServisi.AktiviteYenile();
            var magazaAdi = await _ayarlar.GetAsync("MagazaAdi", "");
            MagazaAdiLabel.Text = string.IsNullOrWhiteSpace(magazaAdi) ? "ÖZ BİGA ET" : magazaAdi.ToUpper();
            BarkodOkuyucu.IsDetecting = true;
            MesajBorder.IsVisible = false;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            BarkodOkuyucu.IsDetecting = false;
        }

        private async Task<bool> ZamanAsimKontrolAsync()
        {
            if (!OturumServisi.OturumSuresiDolduMu()) return false;
            OturumServisi.Cikis();
            await DisplayAlert("Oturum Süresi Doldu", "Güvenlik nedeniyle oturumunuz sonlandırıldı.", "Tamam");
            await Shell.Current.GoToAsync("//LoginPage");
            return true;
        }

        private async void BarkodOkundu(object sender, BarcodeDetectionEventArgs e)
        {
            var result = e.Results.FirstOrDefault();
            if (result == null) return;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                OturumServisi.AktiviteYenile();
                BarkodEntry.Text = result.Value;
                BarkodOkuyucu.IsDetecting = false;
                await UrunGetirAsync(result.Value);
            });
        }

        private async Task UrunGetirAsync(string okunanBarkod)
        {
            if (string.IsNullOrEmpty(okunanBarkod)) return;
            OturumServisi.AktiviteYenile();
            await _db.InitAsync();

            string aranacakBarkod = okunanBarkod;
            decimal okunanMiktar = 1;
            bool teraziUrunuMu = false;
            string prefix = await _ayarlar.GetAsync("TeraziPrefix", "27");

            if (okunanBarkod.Length == 13 && okunanBarkod.StartsWith(prefix))
            {
                teraziUrunuMu = true;
                aranacakBarkod = okunanBarkod.Substring(2, 5);
                string miktarStr = okunanBarkod.Substring(7, 5);
                okunanMiktar = decimal.Parse(miktarStr) / 1000m;
            }

            var urun = await _db.BarkodIleGetirAsync(aranacakBarkod);

            if (urun == null && teraziUrunuMu) urun = await _db.BarkodIleGetirAsync(aranacakBarkod.TrimStart('0'));

            if (urun != null)
            {
                _bulunanUrun = urun;

                if (urun.GramajliMi || teraziUrunuMu)
                {
                    // SES ÇIKMAZ: Sadece bilgiler gösterilir
                    UrunAdLabel.Text = urun.Ad;
                    UrunFiyatLabel.Text = $"Birim Fiyat: ₺{urun.KgFiyati:N2} / Kg";
                    UrunKategoriLabel.Text = $"Kategori: {urun.Kategori}";
                    AdetEntry.Text = okunanMiktar.ToString("0.###");
                    UrunBilgiFrame.IsVisible = true;
                    MesajBorder.IsVisible = false;
                }
                else
                {
                    // SES ÇIKAR: Çünkü direkt sepete ekleniyor
                    _sepet.Ekle(urun, 1, urun.Fiyat);
                    await BipCal();
                    HapticFeedback.Default.Perform(HapticFeedbackType.Click);
                    MesajLabel.Text = $"✅ {urun.Ad} sepete eklendi!";
                    MesajBorder.IsVisible = true;
                    UrunBilgiFrame.IsVisible = false;
                    BarkodEntry.Text = "";
                    await Task.Delay(1000);
                    BarkodOkuyucu.IsDetecting = true;
                }
            }
            else
            {
                bool ekle = await DisplayAlert("Ürün Bulunamadı", $"'{okunanBarkod}' sistemde yok. Hemen eklemek ister misiniz?", "Evet", "Hayır");
                if (ekle) { UrunDuzenleServisi.HizliEkleBarkod = okunanBarkod; await Shell.Current.GoToAsync("//UrunListesi"); }
                else BarkodOkuyucu.IsDetecting = true;
            }
        }

        private async void SatisaEkleTapped(object sender, EventArgs e)
        {
            if (_bulunanUrun == null) return;
            OturumServisi.AktiviteYenile();

            decimal eklenecekFiyat = _bulunanUrun.GramajliMi ? _bulunanUrun.KgFiyati : _bulunanUrun.Fiyat;
            decimal girilenMiktar = decimal.Parse(AdetEntry.Text);

            _sepet.Ekle(_bulunanUrun, girilenMiktar, eklenecekFiyat);

            // SES ÇIKAR: Onay butonuna basıldı ve ürün sepete girdi
            await BipCal();
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);

            MesajLabel.Text = $"✅ {_bulunanUrun.Ad} sepete eklendi!";
            MesajBorder.IsVisible = true;
            UrunBilgiFrame.IsVisible = false;
            BarkodEntry.Text = "";
            BarkodOkuyucu.IsDetecting = true;
        }

        private async void UrunGetirTapped(object sender, EventArgs e) { await UrunGetirAsync(BarkodEntry.Text); }
        private async void BarkodEntry_Completed(object sender, EventArgs e) { await UrunGetirAsync(BarkodEntry.Text); }
        private async void GeriClicked(object sender, EventArgs e) { await Shell.Current.GoToAsync("//AnaSayfa"); }
        private async void SepeteGitClicked(object sender, EventArgs e) { await Shell.Current.GoToAsync("//SepetSayfa"); }
    }
}