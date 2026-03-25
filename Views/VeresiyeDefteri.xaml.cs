using Saller_System.Models;
using Saller_System.Services;

namespace Saller_System.Views
{
    public partial class VeresiyeDefteri : ContentPage
    {
        private readonly DatabaseService _db;

        public VeresiyeDefteri(DatabaseService db)
        {
            InitializeComponent();
            _db = db;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            OturumServisi.AktiviteYenile();

            // ÇÖKMEYİ (BEYAZ EKRANI) ENGELLEYEN KRİTİK NEFES PAYI
            await Task.Delay(100);

            await ListeyiGuncelle();
        }

        private async Task ListeyiGuncelle(string arama = "")
        {
            await _db.InitAsync();
            var liste = await _db.TumMusterileriGetirAsync();

            if (!string.IsNullOrWhiteSpace(arama))
                liste = liste.Where(m => m.AdSoyad.ToLower().Contains(arama.ToLower())).ToList();

            MusterilerListesi.ItemsSource = liste.OrderByDescending(m => m.ToplamBorc).ToList();
        }

        private async void MusteriAraTextChanged(object sender, TextChangedEventArgs e)
        {
            OturumServisi.AktiviteYenile();
            await ListeyiGuncelle(e.NewTextValue);
        }

        private async void YeniMusteriClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            string sonuc = await DisplayPromptAsync("Yeni Müşteri", "Müşteri Adı:", "Kaydet", "İptal");
            if (!string.IsNullOrWhiteSpace(sonuc))
            {
                await _db.MusteriEkleAsync(new Musteri { AdSoyad = sonuc.Trim(), ToplamBorc = 0 });
                await ListeyiGuncelle();
            }
        }

        private async void TahsilatAlClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Musteri m)
            {
                string tutarStr = await DisplayPromptAsync("Tahsilat", $"{m.AdSoyad} Borcu: ₺{m.ToplamBorc:N2}", "Tamam", "İptal", "Tutar girin", -1, Keyboard.Numeric);
                if (decimal.TryParse(tutarStr, out decimal tutar) && tutar > 0)
                {
                    await _db.VeresiyeIslemKaydetAsync(new VeresiyeIslem { MusteriId = m.Id, Tutar = -tutar, Tarih = DateTime.Now, Aciklama = "Elden Tahsilat" });

                    // Tahsilat sonrası listeyi tazelemek için
                    await ListeyiGuncelle();
                }
            }
        }

        private async void GeriClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("..");
    }
}