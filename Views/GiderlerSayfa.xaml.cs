using Saller_System.Models;
using Saller_System.Services;

namespace Saller_System.Views
{
    public partial class GiderlerSayfa : ContentPage
    {
        private readonly DatabaseService _db;

        public GiderlerSayfa(DatabaseService db)
        {
            InitializeComponent();
            _db = db;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            OturumServisi.AktiviteYenile();
            await ListeyiGuncelle();
        }

        // HATA ÇÖZÜMÜ: Geri Dön Butonu
        private async void GeriClicked(object sender, TappedEventArgs e)
        {
            OturumServisi.AktiviteYenile();
            await Shell.Current.GoToAsync("//Raporlar");
        }

        private async Task ListeyiGuncelle()
        {
            await _db.InitAsync();
            var liste = await _db.GunlukGiderlerAsync(DateTime.Today);
            GiderListesi.ItemsSource = liste;
        }

        private async void GiderKaydetClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BaslikEntry.Text) || !decimal.TryParse(TutarEntry.Text, out decimal tutar))
            {
                await DisplayAlert("Uyarı", "Lütfen bir açıklama ve geçerli bir tutar girin.", "Tamam");
                return;
            }

            try
            {
                await _db.GiderEkleAsync(new Gider
                {
                    Baslik = BaslikEntry.Text.Trim(),
                    Tutar = tutar,
                    Tarih = DateTime.Now
                });

                BaslikEntry.Text = "";
                TutarEntry.Text = "";

                await ListeyiGuncelle();
                await DisplayAlert("Başarılı", "Gider sisteme işlendi.", "Tamam");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", "Gider kaydedilemedi: " + ex.Message, "Tamam");
            }
        }
    }
}