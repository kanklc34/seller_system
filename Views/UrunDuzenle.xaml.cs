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
            FiyatEntry.Text = urun.Fiyat.ToString();
            KategoriEntry.Text = urun.Kategori;
        }
        private async void GeriClicked(object sender, EventArgs e)
    => await Shell.Current.GoToAsync("//UrunListesi");
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

            if (!decimal.TryParse(FiyatEntry.Text, out decimal fiyat) || fiyat < 0)
            {
                await DisplayAlert("Hata", "Geçerli bir fiyat girin!", "Tamam");
                return;
            }

            // Eski ürünü kopyala, orijinalini koru
            var yeniUrun = new Saller_System.Models.Urun
            {
                Id = eskiUrun.Id,
                Ad = ad,
                Barkod = barkod,
                Fiyat = fiyat,
                Kategori = KategoriEntry.Text?.Trim() ?? "",
                GramajliMi = eskiUrun.GramajliMi,
                KgFiyati = eskiUrun.KgFiyati
            };

            await _db.InitAsync();
            await _db.UrunGuncelleAsync(yeniUrun, eskiUrun);

            await DisplayAlert("Baþarýlý", "Ürün güncellendi!", "Tamam");
            await Shell.Current.GoToAsync("//UrunListesi");
        }
    }
}