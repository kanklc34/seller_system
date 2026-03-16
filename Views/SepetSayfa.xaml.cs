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
            ArayuzuGuncelle();
        }

        protected override bool OnBackButtonPressed()
        {
            Dispatcher.Dispatch(async () => await Shell.Current.GoToAsync("//BarkodSayfa"));
            return true;
        }

        private void ArayuzuGuncelle()
        {
            SepetListesi.ItemsSource = null;
            SepetListesi.ItemsSource = _sepet.Items;
            ToplamLabel.Text = $"₺{_sepet.Toplam:N2}";
        }

        private async void SatisiTamamlaTapped(object sender, EventArgs e)
        {
            if (_sepet.Items.Count == 0) return;

            bool onay = await DisplayAlert("Satış Onayı", $"Toplam ₺{_sepet.Toplam:N2} onaylıyor musunuz?", "Evet", "Vazgeç");
            if (!onay) return;

            await _db.InitAsync();

            foreach (var item in _sepet.Items)
            {
                decimal maliyet = item.Urun.GramajliMi
                    ? (item.Toplam / (item.Urun.KgFiyati > 0 ? item.Urun.KgFiyati : 1)) * item.Urun.KgAlisFiyati
                    : item.Urun.AlisFiyati * item.Adet;

                var satis = new Satis
                {
                    UrunId = item.Urun.Id,
                    UrunAd = item.Urun.Ad,
                    Fiyat = item.Toplam,
                    AlisFiyati = maliyet,
                    Adet = item.Adet,
                    Tarih = DateTime.Now,
                    KasiyerAd = OturumServisi.AktifKullanici?.KullaniciAdi ?? "Kasiyer"
                };

                await _db.SatisKaydetAsync(satis);
            }

            _sepet.Temizle();
            await DisplayAlert("Başarılı", "Satış Tamamlandı.", "Tamam");
            await Shell.Current.GoToAsync("//BarkodSayfa");
        }

        private void ItemSilTapped(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is SepetItem item)
            {
                _sepet.Cikar(item);
                ArayuzuGuncelle();
            }
        }

        private async void SepetiTemizleTapped(object sender, EventArgs e)
        {
            if (_sepet.Items.Count == 0) return;
            if (await DisplayAlert("Sepet", "Boşaltılsın mı?", "Evet", "Hayır"))
            {
                _sepet.Temizle();
                ArayuzuGuncelle();
            }
        }

        private async void GeriClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//BarkodSayfa");
    }
}