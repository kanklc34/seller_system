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

            // Verileri Form Alanlarına Doldur
            AdEntry.Text = urun.Ad;
            BarkodEntry.Text = urun.Barkod;
            KategoriEntry.Text = urun.Kategori;
            GramajliSwitch.IsToggled = urun.GramajliMi;

            if (urun.GramajliMi)
            {
                KgFiyatiEntry.Text = urun.KgFiyati.ToString();
                KgAlisFiyatiEntry.Text = urun.KgAlisFiyati.ToString();
                NormalFiyatPanel.IsVisible = false;
                KgFiyatPanel.IsVisible = true;
            }
            else
            {
                FiyatEntry.Text = urun.Fiyat.ToString();
                AlisFiyatiEntry.Text = urun.AlisFiyati.ToString();
                NormalFiyatPanel.IsVisible = true;
                KgFiyatPanel.IsVisible = false;
            }
        }

        // TELEFONUN FİZİKSEL GERİ TUŞU
        protected override bool OnBackButtonPressed()
        {
            Dispatcher.Dispatch(async () => await Shell.Current.GoToAsync("//UrunListesi"));
            return true;
        }

        private void GramajliToggled(object sender, ToggledEventArgs e)
        {
            KgFiyatPanel.IsVisible = e.Value;
            NormalFiyatPanel.IsVisible = !e.Value;
        }

        private async void KaydetClicked(object sender, EventArgs e)
        {
            var eskiUrun = UrunDuzenleServisi.SeciliUrun;
            if (eskiUrun == null) return;

            if (string.IsNullOrWhiteSpace(AdEntry.Text) || string.IsNullOrWhiteSpace(BarkodEntry.Text))
            {
                await DisplayAlert("Hata", "Ad ve barkod zorunludur!", "Tamam");
                return;
            }

            var yeniUrun = new Urun
            {
                Id = eskiUrun.Id,
                Ad = AdEntry.Text.Trim(),
                Barkod = BarkodEntry.Text.Trim(),
                Kategori = KategoriEntry.Text?.Trim() ?? "",
                GramajliMi = GramajliSwitch.IsToggled,
                Fiyat = GramajliSwitch.IsToggled ? 0 : decimal.Parse(FiyatEntry.Text ?? "0"),
                AlisFiyati = GramajliSwitch.IsToggled ? 0 : decimal.Parse(AlisFiyatiEntry.Text ?? "0"),
                KgFiyati = GramajliSwitch.IsToggled ? decimal.Parse(KgFiyatiEntry.Text ?? "0") : 0,
                KgAlisFiyati = GramajliSwitch.IsToggled ? decimal.Parse(KgAlisFiyatiEntry.Text ?? "0") : 0
            };

            await _db.InitAsync();
            await _db.UrunGuncelleAsync(yeniUrun, eskiUrun);

            await DisplayAlert("Başarılı", "Ürün bilgileri güncellendi.", "Tamam");
            await Shell.Current.GoToAsync("//UrunListesi");
        }

        private async void GeriClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//UrunListesi");
    }
}