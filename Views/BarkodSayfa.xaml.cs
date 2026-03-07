using Saller_System.Models;
using Saller_System.Services;

namespace Saller_System.Views
{
    public partial class BarkodSayfa : ContentPage
    {
        private readonly DatabaseService _db;
        private Urun? _bulunanUrun;

        public BarkodSayfa(DatabaseService db)
        {
            InitializeComponent();
            _db = db;
        }

        private async void UrunGetirClicked(object sender, EventArgs e)
        {
            string barkod = BarkodEntry.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(barkod)) return;

            await _db.InitAsync();
            _bulunanUrun = await _db.BarkodIleGetirAsync(barkod);

            if (_bulunanUrun != null)
            {
                UrunAdLabel.Text = _bulunanUrun.Ad;
                UrunFiyatLabel.Text = $"Fiyat: {_bulunanUrun.Fiyat:C2}";
                UrunKategoriLabel.Text = $"Kategori: {_bulunanUrun.Kategori}";
                UrunBilgiFrame.IsVisible = true;
                MesajLabel.IsVisible = false;
            }
            else
            {
                await DisplayAlert("Bulunamadı", "Bu barkoda ait ürün bulunamadı!", "Tamam");
            }
        }

        private async void SatisaEkleClicked(object sender, EventArgs e)
        {
            if (_bulunanUrun == null) return;

            int adet = int.TryParse(AdetEntry.Text, out int a) ? a : 1;

            var satis = new Satis
            {
                UrunId = _bulunanUrun.Id,
                UrunAd = _bulunanUrun.Ad,
                Fiyat = _bulunanUrun.Fiyat,
                Adet = adet,
                Tarih = DateTime.Now,
                KasiyerAd = "Kasiyer"
            };

            await _db.SatisKaydetAsync(satis);

            MesajLabel.Text = $"✅ {_bulunanUrun.Ad} satışa eklendi!";
            MesajLabel.IsVisible = true;
            UrunBilgiFrame.IsVisible = false;
            BarkodEntry.Text = "";
            _bulunanUrun = null;
        }

        private async void GeriClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//AnaSayfa");
        }
    }
}