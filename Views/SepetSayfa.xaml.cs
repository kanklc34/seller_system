using Saller_System.Models;
using Saller_System.Services;

namespace Saller_System.Views
{
    public partial class SepetSayfa : ContentPage
    {
        private readonly DatabaseService _db;
        private readonly SepetServisi _sepet;

        public SepetSayfa(DatabaseService db, SepetServisi sepet)
        {
            InitializeComponent();
            _db = db;
            _sepet = sepet;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            SepetListesi.ItemsSource = null;
            SepetListesi.ItemsSource = _sepet.Items;
            ToplamLabel.Text = $"₺{_sepet.Toplam:N2}";
        }

        private async void SatisiTamamlaClicked(object sender, EventArgs e)
        {
            if (_sepet.Items.Count == 0)
            {
                await DisplayAlert("Uyarı", "Sepet boş!", "Tamam");
                return;
            }

            await _db.InitAsync();

            foreach (var item in _sepet.Items)
            {
                var satis = new Satis
                {
                    UrunId = item.Urun.Id,
                    UrunAd = item.Urun.Ad,
                    Fiyat = item.Urun.Fiyat,
                    Adet = item.Adet,
                    Tarih = DateTime.Now,
                    KasiyerAd = OturumServisi.AktifKullanici?.KullaniciAdi ?? "Kasiyer"
                };
                await _db.SatisKaydetAsync(satis);
            }

            decimal toplam = _sepet.Toplam;
            _sepet.Temizle();

            await DisplayAlert("Başarılı", $"Satış tamamlandı!\nToplam: ₺{toplam:N2}", "Tamam");
            await Shell.Current.GoToAsync("//BarkodSayfa");
        }

        private void ItemSilClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is SepetItem item)
            {
                _sepet.Cikar(item);
                SepetListesi.ItemsSource = null;
                SepetListesi.ItemsSource = _sepet.Items;
                ToplamLabel.Text = $"₺{_sepet.Toplam:N2}";
            }
        }

        private void SepetiTemizleClicked(object sender, EventArgs e)
        {
            _sepet.Temizle();
            SepetListesi.ItemsSource = null;
            SepetListesi.ItemsSource = _sepet.Items;
            ToplamLabel.Text = "₺0,00";
        }

        private async void GeriClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//BarkodSayfa");
    }
}