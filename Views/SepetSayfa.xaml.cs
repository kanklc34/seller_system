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
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (await ZamanAsimKontrolAsync()) return;
            OturumServisi.AktiviteYenile();
            await _db.InitAsync();
            ArayuzuGuncelle();
        }

        protected override bool OnBackButtonPressed()
        {
            OturumServisi.AktiviteYenile();
            Dispatcher.Dispatch(async () => await Shell.Current.GoToAsync("//BarkodSayfa"));
            return true;
        }

        private async Task<bool> ZamanAsimKontrolAsync()
        {
            if (!OturumServisi.OturumSuresiDolduMu()) return false;
            OturumServisi.Cikis();
            await DisplayAlert("Oturum Süresi Doldu", "Güvenlik nedeniyle oturumunuz sonlandırıldı.", "Tamam");
            await Shell.Current.GoToAsync("//LoginPage");
            return true;
        }

        private void ArayuzuGuncelle()
        {
            SepetListesi.ItemsSource = null;
            SepetListesi.ItemsSource = _sepet.Items;
            ToplamLabel.Text = $"₺{_sepet.Toplam:N2}";

            bool dolu = _sepet.Items.Count > 0;
            // XAML'da OdemeAlBtn yerine artık OdemeButonlariGrid var
            OdemeButonlariGrid.IsVisible = dolu;
        }

        private void ItemSilTapped(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            if (sender is Button btn && btn.CommandParameter is SepetItem item)
            {
                _sepet.Cikar(item);
                ArayuzuGuncelle();
            }
        }

        private async void SepetiTemizleTapped(object sender, EventArgs e)
        {
            if (_sepet.Items.Count == 0) return;
            OturumServisi.AktiviteYenile();
            if (await DisplayAlert("Sepet", "Boşaltılsın mı?", "Evet", "Hayır"))
            {
                _sepet.Temizle();
                ArayuzuGuncelle();
            }
        }

        // ================================================================
        // ÖDEME VE SATIŞ İŞLEMLERİ (NAKİT VE KART)
        // ================================================================
        private async void NakitOdemeClicked(object sender, EventArgs e) => await SatisIsleminiTamamla("Nakit");
        private async void KartOdemeClicked(object sender, EventArgs e) => await SatisIsleminiTamamla("Kredi Kartı");

        private async Task SatisIsleminiTamamla(string odemeYontemi, int? musteriId = null)
        {
            if (_sepet.Items.Count == 0) return;
            OturumServisi.AktiviteYenile();

            bool onay = await DisplayAlert("Satış Onayı",
                $"Toplam ₺{_sepet.Toplam:N2} ({odemeYontemi}) onaylıyor musunuz?", "Evet", "Vazgeç");
            if (!onay) return;

            var satislar = new List<Satis>();

            foreach (var item in _sepet.Items)
            {
                // KAR HESAPLAMASI (Senin yazdığın hesaplama korundu)
                decimal maliyet = item.Urun.GramajliMi
                    ? (item.Toplam / (item.Urun.KgFiyati > 0 ? item.Urun.KgFiyati : 1)) * item.Urun.KgAlisFiyati
                    : item.Urun.AlisFiyati * item.Adet;

                satislar.Add(new Satis
                {
                    UrunId = item.Urun.Id,
                    UrunAd = item.Urun.Ad,
                    Fiyat = item.Toplam,
                    AlisFiyati = maliyet,
                    Kar = item.Toplam - maliyet,
                    Adet = item.Adet, // ARTIK DECIMAL OLDUĞU İÇİN KIZARMAYACAK
                    Tarih = DateTime.Now,
                    KasiyerAd = OturumServisi.AktifKullanici?.KullaniciAdi ?? "Kasiyer"
                });

                // STOKTAN DÜŞME İŞLEMİ
                var gercekUrun = await _db.BarkodIleGetirAsync(item.Urun.Barkod);
                if (gercekUrun != null)
                {
                    gercekUrun.StokMiktari -= item.Adet; // Eksiye düşebilir
                    var kopya = new Urun { Fiyat = gercekUrun.Fiyat, KgFiyati = gercekUrun.KgFiyati };
                    await _db.UrunGuncelleAsync(gercekUrun, kopya);
                }
            }

            // Satışları kaydet
            await _db.SatisleriTopluKaydetAsync(satislar);

            // EĞER VERESİYE İSE BORCUNU YAZ
            if (odemeYontemi == "Veresiye" && musteriId.HasValue)
            {
                var islem = new VeresiyeIslem
                {
                    MusteriId = musteriId.Value,
                    Tutar = _sepet.Toplam,
                    Tarih = DateTime.Now,
                    Aciklama = "Satış",
                    OdendiMi = false
                };
                await _db.VeresiyeIslemKaydetAsync(islem);
            }

            _sepet.Temizle();
            ArayuzuGuncelle();

            VeresiyePaneli.IsVisible = false;
            OdemeButonlariGrid.IsVisible = true;

            await DisplayAlert("Başarılı", "Satış tamamlandı!", "Tamam");
            await Shell.Current.GoToAsync("//BarkodSayfa");
        }

        // ================================================================
        // VERESİYE İŞLEMLERİ (YENİ)
        // ================================================================
        private async void VeresiyeModuAcClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();

            // Kayıtlı müşterileri listeye bağla
            MusteriPicker.ItemsSource = await _db.TumMusterileriGetirAsync();

            OdemeButonlariGrid.IsVisible = false;
            VeresiyePaneli.IsVisible = true;
        }

        private void VeresiyeIptalClicked(object sender, EventArgs e)
        {
            VeresiyePaneli.IsVisible = false;
            OdemeButonlariGrid.IsVisible = true;
            MusteriPicker.SelectedItem = null;
            YeniMusteriEntry.Text = string.Empty;
        }

        private async void BorcaYazVeTamamlaClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();

            Musteri? secilen = MusteriPicker.SelectedItem as Musteri;
            string yeniAd = YeniMusteriEntry.Text?.Trim() ?? "";

            if (secilen == null && string.IsNullOrEmpty(yeniAd))
            {
                await DisplayAlert("Hata", "Müşteri seçin veya yeni ad yazın.", "Tamam");
                return;
            }

            int id;
            if (!string.IsNullOrEmpty(yeniAd) && secilen == null)
            {
                await _db.MusteriEkleAsync(new Musteri { AdSoyad = yeniAd, ToplamBorc = 0 });
                var list = await _db.TumMusterileriGetirAsync();
                id = list.Last().Id;
            }
            else
            {
                id = secilen!.Id;
            }

            await SatisIsleminiTamamla("Veresiye", id);
        }

        private async void GeriClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            await Shell.Current.GoToAsync("//BarkodSayfa");
        }
    }
}