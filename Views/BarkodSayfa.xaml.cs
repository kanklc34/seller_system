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

            if (await ZamanAsimKontrolAsync()) return;

            OturumServisi.AktiviteYenile();
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
                await UrunGetirAsync(result.Value);
                BarkodOkuyucu.IsDetecting = false;
            });
        }

        private async Task UrunGetirAsync(string barkod)
        {
            if (string.IsNullOrEmpty(barkod)) return;

            OturumServisi.AktiviteYenile();
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
                bool ekle = await DisplayAlert("Ürün Bulunamadı",
                    $"'{barkod}' sistemde yok. Hemen eklemek ister misiniz?", "Evet", "Hayır");
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

            OturumServisi.AktiviteYenile();
            _sepet.Ekle(_bulunanUrun, int.Parse(AdetEntry.Text), _bulunanUrun.Fiyat);

            MesajLabel.Text = $"✅ {_bulunanUrun.Ad} sepete eklendi!";
            MesajBorder.IsVisible = true;
            UrunBilgiFrame.IsVisible = false;
            BarkodEntry.Text = "";
            BarkodOkuyucu.IsDetecting = true;
        }

        private async void UrunGetirTapped(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            await UrunGetirAsync(BarkodEntry.Text);
        }

        private async void BarkodEntry_Completed(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            await UrunGetirAsync(BarkodEntry.Text);
        }

        private async void GeriClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            await Shell.Current.GoToAsync("//AnaSayfa");
        }

        private async void SepeteGitClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            await Shell.Current.GoToAsync("//SepetSayfa");
        }
    }
}