using Saller_System.Models;
using Saller_System.Services;

namespace Saller_System.Views
{
    public partial class UrunDuzenle : ContentPage
    {
        private readonly DatabaseService _db;

        public UrunDuzenle(DatabaseService db)
        {
            InitializeComponent();
            _db = db;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var urun = UrunDuzenleServisi.SeciliUrun;
            if (urun == null) return;

            AdEntry.Text = urun.Ad;
            BarkodEntry.Text = urun.Barkod;
            KategoriEntry.Text = urun.Kategori;
            GramajliCheckBox.IsChecked = urun.GramajliMi;

            if (urun.GramajliMi)
            {
                KgFiyatiEntry.Text = urun.KgFiyati.ToString();
                KgAlisFiyatiEntry.Text = urun.KgAlisFiyati.ToString();
                NormalFiyatPanel.IsVisible = false;
                AlisFiyatPanel.IsVisible = false;
                KgFiyatPanel.IsVisible = true;
                KgAlisFiyatPanel.IsVisible = true;
            }
            else
            {
                FiyatEntry.Text = urun.Fiyat.ToString();
                AlisFiyatiEntry.Text = urun.AlisFiyati.ToString();
                NormalFiyatPanel.IsVisible = true;
                AlisFiyatPanel.IsVisible = true;
                KgFiyatPanel.IsVisible = false;
                KgAlisFiyatPanel.IsVisible = false;
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
            var eskiUrun = UrunDuzenleServisi.SeciliUrun;
            if (eskiUrun == null) return;

            string ad = AdEntry.Text?.Trim() ?? "";
            string barkod = BarkodEntry.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(ad) || string.IsNullOrEmpty(barkod))
            {
                await DisplayAlert("Hata", "Ad ve barkod zorunludur!", "Tamam");
                return;
            }

            bool gramajli = GramajliCheckBox.IsChecked;
            decimal fiyat = 0;
            decimal kgFiyati = 0;
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

            var yeniUrun = new Urun
            {
                Id = eskiUrun.Id,
                Ad = ad,
                Barkod = barkod,
                Fiyat = gramajli ? 0 : fiyat,
                AlisFiyati = alisFiyati,
                Kategori = KategoriEntry.Text?.Trim() ?? "",
                GramajliMi = gramajli,
                KgFiyati = kgFiyati,
                KgAlisFiyati = kgAlisFiyati
            };

            await _db.InitAsync();
            await _db.UrunGuncelleAsync(yeniUrun, eskiUrun);

            await DisplayAlert("Başarılı", "Ürün güncellendi!", "Tamam");
            await Shell.Current.GoToAsync("//UrunListesi");
        }

        private async void GeriClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//UrunListesi");
    }
}