using Saller_System.Models;
using Saller_System.Services;
using ZXing.Net.Maui;

namespace Saller_System.Views
{
    public partial class BarkodSayfa : ContentPage
    {
        private readonly DatabaseService _db;
        private readonly SepetServisi _sepet;
        private Urun? _bulunanUrun;
        private decimal _hesaplananFiyat = 0;
        private decimal _kg = 0;

        public BarkodSayfa(DatabaseService db, SepetServisi sepet)
        {
            InitializeComponent();
            _db = db;
            _sepet = sepet;

            // Mevcut okuma formatlarını koruduk (hızlandırma değişikliği yapılmadı)
            BarkodOkuyucu.Options = new ZXing.Net.Maui.BarcodeReaderOptions
            {
                Formats = ZXing.Net.Maui.BarcodeFormat.QrCode |
                          ZXing.Net.Maui.BarcodeFormat.Ean13 |
                          ZXing.Net.Maui.BarcodeFormat.Ean8 |
                          ZXing.Net.Maui.BarcodeFormat.Code128 |
                          ZXing.Net.Maui.BarcodeFormat.Code39 |
                          ZXing.Net.Maui.BarcodeFormat.Code93 |
                          ZXing.Net.Maui.BarcodeFormat.Codabar |
                          ZXing.Net.Maui.BarcodeFormat.Pdf417 |
                          ZXing.Net.Maui.BarcodeFormat.DataMatrix |
                          ZXing.Net.Maui.BarcodeFormat.UpcA |
                          ZXing.Net.Maui.BarcodeFormat.UpcE |
                          ZXing.Net.Maui.BarcodeFormat.Itf |
                          ZXing.Net.Maui.BarcodeFormat.Msi,
                AutoRotate = true,
                Multiple = false
            };
        }

        // TELEFONUN FİZİKSEL GERİ TUŞU OLAYI
        protected override bool OnBackButtonPressed()
        {
            Dispatcher.Dispatch(async () => await Shell.Current.GoToAsync("//AnaSayfa"));
            return true;
        }

        private async void UrunGetirTapped(object sender, EventArgs e)
        {
            string barkod = BarkodEntry.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(barkod)) return;
            await UrunGetirAsync(barkod);
        }

        private async void BarkodEntry_Completed(object sender, EventArgs e)
        {
            string barkod = BarkodEntry.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(barkod)) return;
            await UrunGetirAsync(barkod);
        }

        private async void SatisaEkleTapped(object sender, EventArgs e)
        {
            if (_bulunanUrun == null) return;
            decimal fiyat;
            int adet = 1;

            if (_bulunanUrun.GramajliMi && _hesaplananFiyat > 0)
                fiyat = _hesaplananFiyat;
            else
            {
                if (!int.TryParse(AdetEntry.Text, out adet) || adet <= 0)
                {
                    await DisplayAlert("Hata", "Adet 0'dan büyük olmalıdır!", "Tamam");
                    return;
                }
                fiyat = _bulunanUrun.Fiyat;
            }

            _sepet.Ekle(_bulunanUrun, adet, fiyat);
            MesajLabel.Text = $"✅ {_bulunanUrun.Ad} sepete eklendi!";
            MesajLabel.IsVisible = true;
            MesajBorder.IsVisible = true;
            UrunBilgiFrame.IsVisible = false;
            BarkodEntry.Text = "";
            _bulunanUrun = null;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            BarkodOkuyucu.IsDetecting = true;
            MesajBorder.IsVisible = false;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            BarkodOkuyucu.IsDetecting = false;
        }

        private async void BarkodOkundu(object sender, BarcodeDetectionEventArgs e)
        {
            var ilkSonuc = e.Results.FirstOrDefault();
            if (ilkSonuc == null) return;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                BarkodEntry.Text = ilkSonuc.Value;
                await UrunGetirAsync(ilkSonuc.Value);
                BarkodOkuyucu.IsDetecting = false;
            });
        }

        private async Task UrunGetirAsync(string barkod)
        {
            barkod = new string(barkod.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray()).Trim();
            if (string.IsNullOrEmpty(barkod)) return;

            await _db.InitAsync();
            var (urunKodu, kg, tartiMi) = TartiServisi.BarkodCoz(barkod);

            Urun? bulunanUrun = null;
            if (tartiMi)
            {
                bulunanUrun = await _db.BarkodIleGetirAsync(urunKodu);
                if (bulunanUrun != null && bulunanUrun.GramajliMi)
                {
                    decimal hesaplananFiyat = TartiServisi.FiyatHesapla(bulunanUrun.KgFiyati, kg);
                    _bulunanUrun = bulunanUrun;
                    _hesaplananFiyat = hesaplananFiyat;
                    _kg = kg;

                    UrunAdLabel.Text = $"{bulunanUrun.Ad} ({kg:N3} kg)";
                    UrunFiyatLabel.Text = $"Fiyat: ₺{hesaplananFiyat:N2}";
                    UrunKategoriLabel.Text = $"Kategori: {bulunanUrun.Kategori}";
                    AdetEntry.Text = "1";
                    AdetEntry.IsEnabled = false;
                    UrunBilgiFrame.IsVisible = true;
                    MesajBorder.IsVisible = false;
                    return;
                }
            }

            bulunanUrun = await _db.BarkodIleGetirAsync(barkod);
            if (bulunanUrun != null)
            {
                _bulunanUrun = bulunanUrun;
                UrunAdLabel.Text = bulunanUrun.Ad;
                UrunFiyatLabel.Text = $"Fiyat: ₺{bulunanUrun.Fiyat:N2}";
                UrunKategoriLabel.Text = $"Kategori: {bulunanUrun.Kategori}";
                AdetEntry.IsEnabled = true;
                UrunBilgiFrame.IsVisible = true;
                MesajBorder.IsVisible = false;
            }
            else
            {
                bool ekle = await DisplayAlert("Ürün Bulunamadı", $"'{barkod}' sistemde yok. Eklemek ister misiniz?", "Ekle", "İptal");
                if (ekle)
                {
                    UrunDuzenleServisi.HizliEkleBarkod = barkod;
                    await Shell.Current.GoToAsync("//UrunEkle");
                }
            }
        }

        private async void GeriClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//AnaSayfa");

        private async void SepeteGitClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//SepetSayfa");
    }
}