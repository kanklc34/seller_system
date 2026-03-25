using Saller_System.Models;
using Saller_System.Services;

namespace Saller_System.Views
{
    public partial class StokYonetimi : ContentPage
    {
        private readonly DatabaseService _db;
        private List<Urun> _tumUrunler = new();

        public StokYonetimi(DatabaseService db)
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

        private async Task ListeyiGuncelle()
        {
            await _db.InitAsync();
            _tumUrunler = await _db.TumUrunleriGetirAsync();
            StokListesi.ItemsSource = _tumUrunler.OrderBy(u => u.Ad).ToList();
        }

        private void UrunArama_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                StokListesi.ItemsSource = _tumUrunler.OrderBy(u => u.Ad).ToList();
                return;
            }

            var aramaMetni = e.NewTextValue.ToLower();
            var sonuc = _tumUrunler
                .Where(u => u.Ad.ToLower().Contains(aramaMetni) || u.Barkod.Contains(aramaMetni))
                .OrderBy(u => u.Ad)
                .ToList();

            StokListesi.ItemsSource = sonuc;
        }

        private async void UrunSecildi(object sender, SelectionChangedEventArgs e)
        {
            var secilenUrun = e.CurrentSelection.FirstOrDefault() as Urun;
            if (secilenUrun == null) return;

            // Seçimi temizle (aynı ürüne tekrar tıklanabilsin diye)
            StokListesi.SelectedItem = null;

            // Klavyeden girilecek yeni stoğu sor
            string sonuc = await DisplayPromptAsync(
                "Stok Güncelle",
                $"{secilenUrun.Ad} için yeni stok miktarını girin.\nMevcut Stok: {secilenUrun.StokMiktari:N2}",
                "Kaydet",
                "İptal",
                "Yeni Miktar",
                -1,
                Keyboard.Numeric,
                secilenUrun.StokMiktari.ToString());

            if (!string.IsNullOrWhiteSpace(sonuc) && decimal.TryParse(sonuc, out decimal yeniStok))
            {
                // UrunGuncelleAsync metodu fiyat değişimi kontrolü için eskiUrun'ü istiyor,
                // Stok güncellemesi yapacağımız için fiyatları aynı tutan sahte bir eski ürün gönderiyoruz.
                var eskiUrun = new Urun
                {
                    Fiyat = secilenUrun.Fiyat,
                    KgFiyati = secilenUrun.KgFiyati,
                    GramajliMi = secilenUrun.GramajliMi
                };

                secilenUrun.StokMiktari = yeniStok;

                try
                {
                    await _db.UrunGuncelleAsync(secilenUrun, eskiUrun);
                    await ListeyiGuncelle(); // Listeyi tazele
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Hata", "Stok güncellenirken bir sorun oluştu: " + ex.Message, "Tamam");
                }
            }
        }
    }
}