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
            // ENGEL KALDIRILDI: Personel de artık hızlı ekleme yapabilir.
            if (string.IsNullOrWhiteSpace(AdEntry.Text) || string.IsNullOrWhiteSpace(BarkodEntry.Text))
            {
                await DisplayAlert("Hata", "Ad ve barkod alanları boş geçilemez!", "Tamam");
                return;
            }

            try
            {
                var urun = new Urun
                {
                    Ad = AdEntry.Text.Trim(),
                    Barkod = BarkodEntry.Text.Trim(),
                    Kategori = KategoriEntry.Text?.Trim() ?? "Genel",
                    GramajliMi = GramajliSwitch.IsToggled,
                    Fiyat = GramajliSwitch.IsToggled ? 0 : decimal.Parse(FiyatEntry.Text ?? "0"),
                    AlisFiyati = GramajliSwitch.IsToggled ? 0 : decimal.Parse(AlisFiyatiEntry.Text ?? "0"),
                    KgFiyati = GramajliSwitch.IsToggled ? decimal.Parse(KgFiyatiEntry.Text ?? "0") : 0,
                    KgAlisFiyati = GramajliSwitch.IsToggled ? decimal.Parse(KgAlisFiyatiEntry.Text ?? "0") : 0
                };

                await _db.UrunEkleAsync(urun);

                MesajLabel.Text = "✅ Ürün başarıyla eklendi!";
                MesajBorder.IsVisible = true;
                await Task.Delay(1500);
                await Shell.Current.GoToAsync("//UrunListesi");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Kayıt Hatası", "Ürün eklenirken bir sorun oluştu: " + ex.Message, "Tamam");
            }
        }

        private async void GeriClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//UrunListesi");
    }
}