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
            NormalFiyatPanel.IsVisible = !e.Value;
            KgFiyatPanel.IsVisible = e.Value;

            // Bilgi balonunu göster/gizle
            TeraziBilgiLabel.IsVisible = e.Value;
        }

        // Barkod yazılırken çalışan yeni denetleme metodu
        private void BarkodEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (GramajliSwitch.IsToggled && e.NewTextValue?.Length > 5)
            {
                // Eğer gramajlı ürün seçiliyse ve 5 haneden fazla giriliyorsa rengi değiştirerek uyar
                TeraziBilgiLabel.TextColor = Color.FromArgb("#E31E24"); // Kırmızı (Hata vurgusu)
                TeraziBilgiLabel.FontAttributes = FontAttributes.Bold;
            }
            else
            {
                TeraziBilgiLabel.TextColor = Color.FromArgb("#64748B"); // Gri (Standart bilgi)
                TeraziBilgiLabel.FontAttributes = FontAttributes.None;
            }
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
                decimal.TryParse(KgFiyatiEntry.Text, out decimal kgFiyat);
                decimal.TryParse(KgAlisFiyatiEntry.Text, out decimal kgAlis);
                decimal.TryParse(FiyatEntry.Text, out decimal normalFiyat);
                decimal.TryParse(AlisFiyatiEntry.Text, out decimal normalAlis);

                var urun = new Urun
                {
                    Ad = AdEntry.Text.Trim(),
                    Barkod = BarkodEntry.Text.Trim(),
                    Kategori = KategoriEntry.Text?.Trim() ?? "Genel",
                    GramajliMi = GramajliSwitch.IsToggled,
                    Fiyat = GramajliSwitch.IsToggled ? 0 : normalFiyat,
                    AlisFiyati = GramajliSwitch.IsToggled ? 0 : normalAlis,
                    KgFiyati = GramajliSwitch.IsToggled ? kgFiyat : 0,
                    KgAlisFiyati = GramajliSwitch.IsToggled ? kgAlis : 0
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