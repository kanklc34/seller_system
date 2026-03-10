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

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _db.InitAsync();
            if (!string.IsNullOrEmpty(UrunDuzenleServisi.HizliEkleBarkod))
            {
                BarkodEntry.Text = UrunDuzenleServisi.HizliEkleBarkod;
                UrunDuzenleServisi.HizliEkleBarkod = null;
                AdEntry.Focus();
            }
        }

        private void GramajliChanged(object sender, CheckedChangedEventArgs e)
        {
            KgFiyatPanel.IsVisible = e.Value;
            KgAlisFiyatPanel.IsVisible = e.Value;
            NormalFiyatPanel.IsVisible = !e.Value;
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
            decimal kgFiyati = 0, fiyat = 0, alisFiyati = 0, kgAlisFiyati = 0;

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

            await _db.UrunEkleAsync(urun);

            MesajLabel.Text = $"✅ {ad} başarıyla eklendi!";
            MesajLabel.IsVisible = true;
            MesajBorder.IsVisible = true;

            AdEntry.Text = "";
            BarkodEntry.Text = "";
            FiyatEntry.Text = "";
            KategoriEntry.Text = "";
            KgFiyatiEntry.Text = "";
            AlisFiyatiEntry.Text = "";
            KgAlisFiyatiEntry.Text = "";
            GramajliCheckBox.IsChecked = false;
        }

        private async void GeriClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//UrunListesi");
    }
}