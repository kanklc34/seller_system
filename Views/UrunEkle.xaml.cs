using Saller_System.Models;
using Saller_System.Services;

namespace Saller_System.Views
{
    public partial class UrunEkle : ContentPage
    {
        private readonly DatabaseService _db;

        public UrunEkle(DatabaseService db)
        {
            InitializeComponent();
            _db = db;
        }

        private async void KaydetClicked(object sender, EventArgs e)
        {
            string ad = AdEntry.Text?.Trim() ?? "";
            string barkod = BarkodEntry.Text?.Trim() ?? "";
            string kategori = KategoriEntry.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(ad) || string.IsNullOrEmpty(barkod))
            {
                await DisplayAlert("Hata", "Ürün adı ve barkod zorunludur!", "Tamam");
                return;
            }

            decimal fiyat = decimal.TryParse(FiyatEntry.Text, out decimal f) ? f : 0;

            var urun = new Urun
            {
                Ad = ad,
                Barkod = barkod,
                Fiyat = fiyat,
                Kategori = kategori
            };

            await _db.InitAsync();
            await _db.UrunEkleAsync(urun);

            MesajLabel.Text = $"✅ {ad} başarıyla eklendi!";
            MesajLabel.IsVisible = true;

            AdEntry.Text = "";
            BarkodEntry.Text = "";
            FiyatEntry.Text = "";
            KategoriEntry.Text = "";
        }

        private async void GeriClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//UrunListesi");
        }
    }
}