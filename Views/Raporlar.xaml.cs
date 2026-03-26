using Saller_System.Services;
using Saller_System.Models;
using Microcharts;       // GRAFİK İÇİN EKLENDİ
using SkiaSharp;         // GRAFİK MOTORU İÇİN EKLENDİ

namespace Saller_System.Views
{
    public partial class Raporlar : ContentPage
    {
        private readonly DatabaseService _db;
        private readonly ExcelServisi _excel;
        private readonly AyarlarServisi _ayarlar;

        public Raporlar(DatabaseService db, ExcelServisi excel, AyarlarServisi ayarlar)
        {
            InitializeComponent();
            _db = db;
            _excel = excel;
            _ayarlar = ayarlar;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (await ZamanAsimKontrolAsync()) return;

            OturumServisi.AktiviteYenile();

            var magazaAdi = await _ayarlar.GetAsync("MagazaAdi", "");
            if (!string.IsNullOrWhiteSpace(magazaAdi))
                RaporBaslikLabel.Text = $"{magazaAdi} — Finans";

            await VerileriYukle();
        }

        private async Task<bool> ZamanAsimKontrolAsync()
        {
            if (!OturumServisi.OturumSuresiDolduMu()) return false;

            OturumServisi.Cikis();
            await DisplayAlert("Oturum Süresi Doldu", "Güvenlik nedeniyle oturumunuz sonlandırıldı.", "Tamam");
            await Shell.Current.GoToAsync("//LoginPage");
            return true;
        }

        private async Task VerileriYukle()
        {
            await _db.InitAsync();
            var bugun = DateTime.Today;

            var bugunkuSatislar = await _db.GunlukSatislerAsync(bugun); // Grafiğe veri sağlamak için
            var gunlukKar = await _db.GunlukKarAsync(bugun);
            var gunlukCiro = await _db.GunlukGercekCiroAsync(bugun);
            var gunlukSayi = await _db.GunlukSatisSayisiAsync(bugun);
            var aylikCiro = await _db.AylikCiroAsync(bugun.Year, bugun.Month);
            var performans = await _db.PersonelPerformansRaporuGetirAsync(bugun);

            decimal tahsilat = await _db.GunlukTahsilatToplamiAsync(bugun);
            var veresiyeIslemleri = await _db.GunlukVeresiyeDetaylariAsync(bugun);
            decimal veresiyeCikan = veresiyeIslemleri.Where(v => v.Tutar > 0).Sum(v => v.Tutar);
            decimal netKasa = gunlukCiro - veresiyeCikan + tahsilat;

            PersonelPerformansListesi.ItemsSource = performans;
            GunlukKarLabel.Text = $"₺{gunlukKar:N2}";
            GunlukSayiLabel.Text = $"{gunlukSayi} Adet";
            AylikCiroLabel.Text = $"₺{aylikCiro:N2}";
            AyLabel.Text = bugun.ToString("MMMM yyyy").ToUpper();

            // Kasa Değerleri
            GunlukCiroLabel.Text = $"₺{gunlukCiro:N2}";
            VeresiyeCikanLabel.Text = $"- ₺{veresiyeCikan:N2}";
            TahsilatGirenLabel.Text = $"+ ₺{tahsilat:N2}";
            NetKasaLabel.Text = $"₺{netKasa:N2}";

            // GRAFİĞİ ÇİZ
            GrafikOlustur(bugunkuSatislar);
        }

        // YENİ: PASTA GRAFİĞİNİ HESAPLAYAN VE ÇİZEN METOT
        private void GrafikOlustur(List<Satis> satislar)
        {
            // Tahsilatları grafiğe dahil etmiyoruz, sadece et satışı lazım
            var gercekSatislar = satislar.Where(s => s.SatisTipi != "TAHSILAT").ToList();

            if (!gercekSatislar.Any())
            {
                SatisDagilimiChart.Chart = null;
                return;
            }

            // Aynı ürünleri birleştir ve toplam ciroya göre en çok satandan aza doğru sırala
            var gruplanmisSatislar = gercekSatislar
                .GroupBy(s => s.UrunAd)
                .Select(g => new { UrunAd = g.Key, ToplamFiyat = g.Sum(x => x.Fiyat) })
                .OrderByDescending(x => x.ToplamFiyat)
                .ToList();

            var chartEntries = new List<ChartEntry>();

            // Öz Biga Et Kurumsal Renk Paleti (Kırmızı, Altın Sarısı, Turuncu, Yeşil, Mavi vb.)
            string[] renkler = { "#E31E24", "#D4AF37", "#F59E0B", "#16A34A", "#3B82F6", "#64748B" };

            for (int i = 0; i < gruplanmisSatislar.Count; i++)
            {
                if (i < 4) // En çok ciro yapan ilk 4 ürünü göster
                {
                    chartEntries.Add(new ChartEntry((float)gruplanmisSatislar[i].ToplamFiyat)
                    {
                        Label = gruplanmisSatislar[i].UrunAd,
                        ValueLabel = $"₺{gruplanmisSatislar[i].ToplamFiyat:N0}",
                        Color = SKColor.Parse(renkler[i]),
                        ValueLabelColor = SKColor.Parse(renkler[i])
                    });
                }
                else if (i == 4) // Geri kalan ürünleri "Diğer" adı altında topla
                {
                    var digerToplam = gruplanmisSatislar.Skip(4).Sum(x => x.ToplamFiyat);
                    chartEntries.Add(new ChartEntry((float)digerToplam)
                    {
                        Label = "Diğer",
                        ValueLabel = $"₺{digerToplam:N0}",
                        Color = SKColor.Parse("#94A3B8"), // Gri
                        ValueLabelColor = SKColor.Parse("#94A3B8")
                    });
                    break;
                }
            }

            // Şık bir "Halka" (Donut) grafiği oluşturuyoruz
            SatisDagilimiChart.Chart = new DonutChart()
            {
                Entries = chartEntries,
                LabelTextSize = 28f,
                BackgroundColor = SKColors.Transparent,
                HoleRadius = 0.55f, // Grafiğin ortasındaki boşluğun oranı
                Margin = 10
            };
        }

        private async void SatisGecmisiClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            await Shell.Current.GoToAsync("//SatisGecmisiSayfa");
        }

        private async void ExcelAktarClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            var satislar = await _db.GunlukSatislerAsync(DateTime.Today);
            if (satislar.Count == 0) return;

            string dosyaYolu = await _excel.RaporOlustur(satislar, "Gunluk_Rapor", DateTime.Today);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Rapor",
                File = new ShareFile(dosyaYolu)
            });
        }

        private async void GeriClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            await Shell.Current.GoToAsync("//AnaSayfa");
        }
    }
}