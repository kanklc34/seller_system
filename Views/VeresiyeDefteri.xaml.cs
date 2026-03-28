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
            await ListeyiGuncelle();
        }

        private async void GeriClicked(object sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync("//AnaSayfa");
        }

        private async Task ListeyiGuncelle(string arama = "")
        {
            await _db.InitAsync();
            var liste = await _db.TumMusterileriGetirAsync();

            if (!string.IsNullOrWhiteSpace(arama))
                liste = liste.Where(m => m.AdSoyad.ToLower().Contains(arama.ToLower())).ToList();

            // Musteri modelindeki ToplamBorc double ise decimal'a cast ederek sıralıyoruz
            MusterilerListesi.ItemsSource = liste.OrderByDescending(m => m.ToplamBorc).ToList();
        }

        private async void MusteriAraTextChanged(object sender, TextChangedEventArgs e)
        {
            await ListeyiGuncelle(e.NewTextValue);
        }

        private async void YeniMusteriClicked(object sender, EventArgs e)
        {
            string sonuc = await DisplayPromptAsync("Yeni Müşteri", "Müşteri Adı:");
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
                // Seçenek sunuyoruz: Borç mu yazacağız, para mı alacağız?
                string eylem = await DisplayActionSheet($"{m.AdSoyad} İşlemi", "Vazgeç", null, "Ödeme Al (Tahsilat)", "Borç Yaz (Veresiye)");

                if (eylem == "Vazgeç" || string.IsNullOrEmpty(eylem)) return;

                bool borcMu = eylem == "Borç Yaz (Veresiye)";
                string baslik = borcMu ? "Borç Yaz" : "Ödeme Al";
                string mesaj = borcMu ? "Müşteriye yazılacak borç miktarı:" : "Müşteriden alınan nakit miktar:";

                string tutarStr = await DisplayPromptAsync(baslik, mesaj, "Tamam", "İptal", "Miktar girin", -1, Keyboard.Numeric);

                if (decimal.TryParse(tutarStr, out decimal tutar) && tutar > 0)
                {
                    // ÖNEMLİ: Borç ise pozitif (+), Tahsilat ise negatif (-) tutar gönderiyoruz.
                    decimal islemTutari = borcMu ? tutar : -tutar;
                    string aciklama = borcMu ? "Elden Borç Yazıldı" : "Elden Tahsilat (Nakit)";

                    // 1. Veresiye hareketini kaydet (double cast ekledik)
                    await _db.SatisKaydetAsync(new Satis
                    {
                        UrunAd = $"Borç Tahsilatı: {m.AdSoyad}",
                        Fiyat = tutar,
                        SatisTipi = "TAHSILAT",
                        Tarih = DateTime.Now,
                        KasiyerAd = "Admin"
                    });

                    // 2. Eğer tahsilatsa (borç değilse) kasaya (Satis) işle
                    if (!borcMu)
                    {
                        await _db.SatisKaydetAsync(new Satis
                        {
                            UrunAd = $"Borç Tahsilatı: {m.AdSoyad}",
                            Fiyat = tutar, // Satis modelindeki Fiyat decimal ise sorunsuz geçer
                            SatisTipi = "TAHSILAT",
                            Tarih = DateTime.Now,
                            KasiyerAd = "Admin"
                        });
                    }

                    await ListeyiGuncelle();
                    await DisplayAlert("Başarılı", "İşlem kaydedildi.", "Tamam");
                }
            }
        }

        private async void HesabiKapatClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Musteri m)
            {
                // ToplamBorc double ise decimal'a cast edip kontrol ediyoruz
                decimal bakiye = (decimal)m.ToplamBorc;

                if (bakiye == 0)
                {
                    await DisplayAlert("Bilgi", "Müşterinin zaten bakiyesi sıfır.", "Tamam");
                    return;
                }

                bool onay = await DisplayAlert("Hesap Kapatma",
                    $"{m.AdSoyad} kişisinin hesabı {Math.Abs(bakiye):N2} TL ile sıfırlanacak. Onaylıyor musunuz?",
                    "Evet, Kapat", "Hayır");

                if (onay)
                {
                    // 1. Veresiye hareketine işle (Sıfırlamak için bakiye neyse tersini ekliyoruz)
                    await _db.SatisKaydetAsync(new Satis
                    {
                        UrunAd = $"Hesap Kapatma (Tahsilat): {m.AdSoyad}",
                        Fiyat = bakiye, // <-- BURAYA (double) EKLEDİK
                        SatisTipi = "TAHSILAT",
                        Tarih = DateTime.Now,
                        KasiyerAd = "Admin"
                    });

                    // 2. Eğer alacağımız varsa kasaya tahsilat olarak işle
                    if (bakiye > 0)
                    {
                        await _db.SatisKaydetAsync(new Satis
                        {
                            UrunAd = $"Hesap Kapatma (Tahsilat): {m.AdSoyad}",
                            Fiyat = bakiye,
                            SatisTipi = "TAHSILAT",
                            Tarih = DateTime.Now,
                            KasiyerAd = "Admin"
                        });
                    }

                    await ListeyiGuncelle();
                    await DisplayAlert("Başarılı", "Hesap kapatıldı.", "Tamam");
                }
            }
        }

        private async void EkstreClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Musteri m)
            {
                // AppShell'de register ettiğinden emin ol: Routing.RegisterRoute("MusteriEkstresi", typeof(MusteriEkstresi));
                await Shell.Current.GoToAsync($"MusteriEkstresi?MusteriId={m.Id}");
            }
        }
    }
}