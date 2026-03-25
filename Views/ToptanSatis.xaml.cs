using System.Collections.ObjectModel;
using Saller_System.Models;
using Saller_System.Services;

namespace Saller_System.Views
{
    public partial class ToptanSatis : ContentPage
    {
        private readonly DatabaseService _db;
        private List<Urun> _tumUrunler = new();
        private Urun _secilenUrun;
        private ObservableCollection<Satis> _sepet = new();
        private decimal _toplamTutar = 0;

        public ToptanSatis(DatabaseService db)
        {
            InitializeComponent();
            _db = db;
            SepetListesi.ItemsSource = _sepet;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await VerileriYukle();
        }

        private async void GeriClicked(object sender, TappedEventArgs e)
        {
            OturumServisi.AktiviteYenile();
            await Shell.Current.GoToAsync("//AnaSayfa");
        }

        private async Task VerileriYukle()
        {
            await _db.InitAsync();
            var toptancilar = await _db.TumToptanMusterileriGetirAsync();
            MusteriPicker.ItemsSource = toptancilar.OrderBy(m => m.SirketAdi).ToList();
            _tumUrunler = await _db.TumUrunleriGetirAsync();
        }

        private async void YeniMusteri_Clicked(object sender, EventArgs e)
        {
            string sonuc = await DisplayPromptAsync("Yeni Firma", "Restoran/Otel Adı:");
            if (!string.IsNullOrWhiteSpace(sonuc))
            {
                await _db.ToptanMusteriEkleAsync(new ToptanMusteri { SirketAdi = sonuc.Trim() });
                await VerileriYukle();
            }
        }

        private void UrunArama_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                UrunlerListesi.IsVisible = false;
                return;
            }

            var arama = e.NewTextValue.ToLower();
            var sonuc = _tumUrunler.Where(u => u.Ad != null && u.Ad.ToLower().Contains(arama)).ToList();

            UrunlerListesi.ItemsSource = sonuc;
            UrunlerListesi.IsVisible = sonuc.Any();
        }

        private void UrunSecildi(object sender, SelectionChangedEventArgs e)
        {
            _secilenUrun = e.CurrentSelection.FirstOrDefault() as Urun;
            if (_secilenUrun != null)
            {
                SecilenUrunLabel.Text = $"Seçilen: {_secilenUrun.Ad}";
                SecilenUrunLabel.TextColor = Colors.Green;

                decimal alis = _secilenUrun.GramajliMi ? _secilenUrun.KgAlisFiyati : _secilenUrun.AlisFiyati;
                decimal satis = _secilenUrun.GramajliMi ? _secilenUrun.KgFiyati : _secilenUrun.Fiyat;

                if (alis == 0) alis = _secilenUrun.AlisFiyati;
                if (satis == 0) satis = _secilenUrun.Fiyat;

                MaliyetEntry.Text = alis.ToString("0.##");
                SatisFiyatiEntry.Text = satis.ToString("0.##");

                UrunAramaBar.Text = "";
                UrunlerListesi.IsVisible = false;
                MiktarEntry.Focus();
            }
        }

        private void SepeteEkle_Clicked(object sender, EventArgs e)
        {
            if (_secilenUrun == null)
            {
                DisplayAlert("Uyarı", "Lütfen ürün seçin.", "Tamam");
                return;
            }

            if (!decimal.TryParse(MiktarEntry.Text, out decimal miktar) || miktar <= 0) return;
            if (!decimal.TryParse(MaliyetEntry.Text, out decimal maliyet) || maliyet < 0) return;
            if (!decimal.TryParse(SatisFiyatiEntry.Text, out decimal satisFiyat) || satisFiyat < 0) return;

            decimal tutar = satisFiyat * miktar;
            decimal kar = (satisFiyat - maliyet) * miktar;

            _sepet.Add(new Satis
            {
                UrunId = _secilenUrun.Id,
                UrunAd = _secilenUrun.Ad,
                Adet = miktar,
                Fiyat = tutar,
                AlisFiyati = maliyet * miktar,
                Kar = kar,
                SatisTipi = "TOPTAN",
                Tarih = DateTime.Now,
                KasiyerAd = OturumServisi.AktifKullanici?.KullaniciAdi ?? "Bilinmiyor"
            });

            _toplamTutar += tutar;
            ToplamTutarLabel.Text = $"Toplam: ₺{_toplamTutar:N2}";

            _secilenUrun = null;
            SecilenUrunLabel.Text = "Seçilen: (Yok)";
            SecilenUrunLabel.TextColor = Color.FromArgb("#E31E24");
            MiktarEntry.Text = "";
            MaliyetEntry.Text = "";
            SatisFiyatiEntry.Text = "";
        }

        private async void SatisTamamla_Clicked(object sender, EventArgs e)
        {
            var musteri = MusteriPicker.SelectedItem as ToptanMusteri;

            if (musteri == null || !_sepet.Any())
            {
                await DisplayAlert("Uyarı", "Firma seçin ve sepete ürün ekleyin.", "Tamam");
                return;
            }

            try
            {
                // YENİ: Seçilen firmayı sepetin KasiyerAd bilgisine mühürlüyoruz (Excel için)
                string aktifKasiyer = OturumServisi.AktifKullanici?.KullaniciAdi ?? "Bilinmiyor";
                foreach (var item in _sepet)
                {
                    item.KasiyerAd = $"{aktifKasiyer} (Firma: {musteri.SirketAdi})";
                }

                await _db.SatisleriTopluKaydetAsync(_sepet);
                await _db.ToptanMusteriBorcEkleAsync(musteri.Id, _toplamTutar);

                await DisplayAlert("Başarılı", $"{musteri.SirketAdi} borcuna ₺{_toplamTutar:N2} eklendi.", "Tamam");

                _sepet.Clear();
                _toplamTutar = 0;
                ToplamTutarLabel.Text = "Toplam: ₺0.00";
                MusteriPicker.SelectedItem = null;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }
    }
}