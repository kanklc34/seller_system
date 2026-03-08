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

        private void GramajliChanged(object sender, CheckedChangedEventArgs e)
        {
            KgFiyatPanel.IsVisible = e.Value;
            KgAlisFiyatPanel.IsVisible = e.Value;
            AlisFiyatPanel.IsVisible = !e.Value;
        }

        private async void KaydetClicked(object sender, EventArgs e)
        {
            if (!OturumServisi.YoneticiMi)
            {
                await DisplayAlert("Yetkisiz", "Ürün eklemek için yönetici yetkisi gereklidir!", "Tamam");
                return;
            }

            string ad = AdEntry.Text?.Trim() ?? "";
            string barkod = BarkodEntry.Text?.Trim() ?? "";
            string kategori = KategoriEntry.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(ad) || string.IsNullOrEmpty(barkod))
            {
                await DisplayAlert("Hata", "Ürün adı ve barkod zorunludur!", "Tamam");
                return;
            }

            bool gramajli = GramajliCheckBox.IsChecked;
            decimal kgFiyati = 0;
            decimal fiyat = 0;
            decimal alisFiyati = 0;
            decimal kgAlisFiyati = 0;

            if (gramajli)
            {
                if (!decimal.TryParse(KgFiyatiEntry.Text, out kgFiyati) || kgFiyati <= 0)
                {
                    await DisplayAlert("Hata", "Geçerli bir kg fiyatı girin!", "Tamam");
                    return;
                }
                decimal.TryParse(KgAlisFiyatiEntry.Text, out kgAlisFiyati);
            }
            else
            {
                if (!decimal.TryParse(FiyatEntry.Text, out fiyat) || fiyat < 0)
                {
                    await DisplayAlert("Hata", "Geçerli bir fiyat girin!", "Tamam");
                    return;
                }
                decimal.TryParse(AlisFiyatiEntry.Text, out alisFiyati);
            }

            var urun = new Urun
            {
                Ad = ad,
                Barkod = barkod,
                Fiyat = gramajli ? 0 : fiyat,
                AlisFiyati = alisFiyati,
                Kategori = kategori,
                GramajliMi = gramajli,
                KgFiyati = kgFiyati,
                KgAlisFiyati = kgAlisFiyati
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
            => await Shell.Current.GoToAsync("//UrunListesi");
    }
}