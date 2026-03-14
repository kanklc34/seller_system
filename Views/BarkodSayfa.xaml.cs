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

        public BarkodSayfa(DatabaseService db, SepetServisi sepet)
        {
            InitializeComponent();
            _db = db;
            _sepet = sepet;

            BarkodOkuyucu.Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormat.Ean13 | BarcodeFormat.Ean8 | BarcodeFormat.Code128 | BarcodeFormat.QrCode,
                AutoRotate = true,
                Multiple = false
            };
        }

        protected override async void OnAppearing()
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
            var result = e.Results.FirstOrDefault();
            if (result == null) return;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                BarkodEntry.Text = result.Value;
                await UrunGetirAsync(result.Value);
                BarkodOkuyucu.IsDetecting = false;
            });
        }

        private async Task UrunGetirAsync(string barkod)
        {
            if (string.IsNullOrEmpty(barkod)) return;
            await _db.InitAsync();

            var urun = await _db.BarkodIleGetirAsync(barkod);

            if (urun != null)
            {
                _bulunanUrun = urun;
                UrunAdLabel.Text = urun.Ad;
                UrunFiyatLabel.Text = $"Fiyat: ₺{urun.Fiyat:N2}";
                UrunKategoriLabel.Text = $"Kategori: {urun.Kategori}";
                UrunBilgiFrame.IsVisible = true;
                MesajBorder.IsVisible = false;
            }
            else
            {
                // HIZLI EKLEME ÖZELLİĞİ
                bool ekle = await DisplayAlert("Ürün Bulunamadı", $"'{barkod}' sistemde yok. Hemen eklemek ister misiniz?", "Evet", "Hayır");
                if (ekle)
                {
                    UrunDuzenleServisi.HizliEkleBarkod = barkod;
                    await Shell.Current.GoToAsync("//UrunListesi");
                }
            }
        }

        private async void SatisaEkleTapped(object sender, EventArgs e)
        {
            if (_bulunanUrun == null) return;
            _sepet.Ekle(_bulunanUrun, int.Parse(AdetEntry.Text), _bulunanUrun.Fiyat);

            MesajLabel.Text = $"✅ {_bulunanUrun.Ad} sepete eklendi!";
            MesajBorder.IsVisible = true;
            UrunBilgiFrame.IsVisible = false;
            BarkodEntry.Text = "";
            BarkodOkuyucu.IsDetecting = true;
        }

        private async void UrunGetirTapped(object sender, EventArgs e) => await UrunGetirAsync(BarkodEntry.Text);
        private async void BarkodEntry_Completed(object sender, EventArgs e) => await UrunGetirAsync(BarkodEntry.Text);
        private async void GeriClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//AnaSayfa");
        private async void SepeteGitClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//SepetSayfa");
    }
}