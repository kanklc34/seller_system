using Saller_System.Models;
using Saller_System.Services;

namespace Saller_System.Views
{
    public partial class ToptanSatis : ContentPage
    {
        private readonly DatabaseService _db;
        private List<Urun> _tumUrunler = new();
        private Urun _secilenUrun;

        public ToptanSatis(DatabaseService db)
        {
            InitializeComponent();
            _db = db;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await VerileriYukle();
        }

        private async Task VerileriYukle()
        {
            await _db.InitAsync();
            var musteriler = await _db.TumMusterileriGetirAsync();
            MusteriPicker.ItemsSource = musteriler.OrderBy(m => m.AdSoyad).ToList();
            _tumUrunler = await _db.TumUrunleriGetirAsync();
        }

        private void UrunArama_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                UrunlerListesi.ItemsSource = null;
                return;
            }

            // Urun.cs içindeki isme uygun olarak u.Ad yapıldı
            var sonuc = _tumUrunler
                .Where(u => u.Ad.ToLower().Contains(e.NewTextValue.ToLower()))
                .ToList();

            UrunlerListesi.ItemsSource = sonuc;
        }

        private void UrunSecildi(object sender, SelectionChangedEventArgs e)
        {
            _secilenUrun = e.CurrentSelection.FirstOrDefault() as Urun;
            if (_secilenUrun != null)
            {
                MiktarEntry.Focus();
            }
        }

        private async void SatisTamamla_Clicked(object sender, EventArgs e)
        {
            var secilenMusteri = MusteriPicker.SelectedItem as Musteri;

            if (secilenMusteri == null || _secilenUrun == null || string.IsNullOrWhiteSpace(MiktarEntry.Text))
            {
                await DisplayAlert("Uyarı", "Lütfen Müşteri, Ürün ve Miktar alanlarını doldurun!", "Tamam");
                return;
            }

            if (!decimal.TryParse(MiktarEntry.Text, out decimal miktar) || miktar <= 0)
            {
                await DisplayAlert("Hata", "Geçerli bir miktar giriniz!", "Tamam");
                return;
            }

            try
            {
                decimal toplamTutar = _secilenUrun.Fiyat * miktar;

                // 1. Satışı Kaydet (Senin Satis modelindeki isimlerle)
                var yeniSatis = new Satis
                {
                    UrunId = _secilenUrun.Id,
                    UrunAd = _secilenUrun.Ad,   // UrunAdi değil UrunAd
                    Adet = miktar,              // Miktar değil Adet
                    Fiyat = toplamTutar,        // ToplamFiyat değil Fiyat
                    Tarih = DateTime.Now,
                    SatisTipi = "TOPTAN",       // Excel için Toptan etiketi
                    KasiyerAd = OturumServisi.AktifKullanici?.KullaniciAdi ?? "Bilinmiyor"
                };

                // ToptanSatisKaydet yerine genel SatisKaydet'i kullanıyoruz
                await _db.SatisKaydetAsync(yeniSatis);

                // 2. Müşterinin Veresiye Borcuna Ekle
                await _db.VeresiyeIslemKaydetAsync(new VeresiyeIslem
                {
                    MusteriId = secilenMusteri.Id,
                    Tutar = toplamTutar,
                    Tarih = DateTime.Now,
                    Aciklama = $"{miktar} kg {_secilenUrun.Ad} (Toptan)"
                });

                await DisplayAlert("Başarılı", $"{secilenMusteri.AdSoyad} hesabına ₺{toplamTutar:N2} borç kaydedildi.", "Tamam");

                // Formu temizle
                MiktarEntry.Text = "";
                UrunAramaBar.Text = "";
                UrunlerListesi.ItemsSource = null;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", "İşlem sırasında hata oluştu: " + ex.Message, "Tamam");
            }
        }
    }
}