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
                string tutarStr = await DisplayPromptAsync("Tahsilat", $"{m.AdSoyad} kişisinden ne kadar nakit aldınız?", "Tamam", "İptal", "Miktar girin", -1, Keyboard.Numeric);

                if (decimal.TryParse(tutarStr, out decimal tutar) && tutar > 0)
                {
                    await _db.VeresiyeIslemKaydetAsync(new VeresiyeIslem { MusteriId = m.Id, Tutar = -tutar, Tarih = DateTime.Now, Aciklama = "Elden Tahsilat (Nakit)" });

                    await _db.SatisKaydetAsync(new Satis
                    {
                        UrunAd = $"Borç Tahsilatı: {m.AdSoyad}",
                        Fiyat = tutar,
                        SatisTipi = "TAHSILAT",
                        Tarih = DateTime.Now,
                        KasiyerAd = "Admin"
                    });

                    await ListeyiGuncelle();
                    await DisplayAlert("Başarılı", $"{tutar:N2} TL alındı ve borçtan düşüldü.", "Tamam");
                }
            }
        }

        // YENİ: HESABI KAPAT / HELALLEŞME MANTIĞI
        private async void HesabiKapatClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Musteri m)
            {
                if (m.ToplamBorc == 0)
                {
                    await DisplayAlert("Bilgi", "Müşterinin zaten bakiyesi sıfır.", "Tamam");
                    return;
                }

                string eylem = m.ToplamBorc > 0 ? "Tahsil Edilecek" : "Ödenecek";
                bool onay = await DisplayAlert("Hesap Kapatma",
                    $"{m.AdSoyad} kişisinin hesabı {Math.Abs(m.ToplamBorc):N2} TL ile sıfırlanacak. Onaylıyor musunuz?",
                    "Evet, Kapat", "Hayır");

                if (onay)
                {
                    decimal sifirlamaTutari = -m.ToplamBorc; // Borcu sıfırlamak için gereken zıt değer
                    decimal kasaGirisi = m.ToplamBorc; // Kasaya girecek/çıkacak net miktar

                    // 1. Veresiye hareketine işle
                    await _db.VeresiyeIslemKaydetAsync(new VeresiyeIslem
                    {
                        MusteriId = m.Id,
                        Tutar = sifirlamaTutari,
                        Tarih = DateTime.Now,
                        Aciklama = "Hesap Sıfırlandı / Kapatıldı"
                    });

                    // 2. Eğer müşteriden alacağımız varsa (Bakiye > 0), bu bir TAHSILAT'tır. Kasaya işleyelim.
                    if (m.ToplamBorc > 0)
                    {
                        await _db.SatisKaydetAsync(new Satis
                        {
                            UrunAd = $"Hesap Kapatma (Tahsilat): {m.AdSoyad}",
                            Fiyat = kasaGirisi,
                            SatisTipi = "TAHSILAT",
                            Tarih = DateTime.Now,
                            KasiyerAd = "Admin"
                        });
                    }

                    await ListeyiGuncelle();
                    await DisplayAlert("Başarılı", "Hesap başarıyla kapatıldı ve bakiye sıfırlandı.", "Tamam");
                }
            }
        }

        private async void EkstreClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Musteri m)
            {
                await Shell.Current.GoToAsync($"MusteriEkstresi?MusteriId={m.Id}");
            }
        }
    }
}