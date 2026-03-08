using Saller_System.Services;
using Saller_System.Models;

namespace Saller_System.Views
{
    public partial class SatisGecmisiSayfa : ContentPage
    {
        private readonly DatabaseService _db;
        private readonly ExcelServisi _excel;

        // Navigasyon seviyesi: 0=Yıl, 1=Ay, 2=Hafta, 3=Gün
        private int _seviye = 0;
        private int _seciliYil = 0;
        private int _seciliAy = 0;
        private int _seciliHafta = 0;

        private List<GecmisItem> _mevcutListe = new();

        public SatisGecmisiSayfa(DatabaseService db, ExcelServisi excel)
        {
            InitializeComponent();
            _db = db;
            _excel = excel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _db.InitAsync();
            await YillariYukle();
        }

        private async Task YillariYukle()
        {
            _seviye = 0;
            UstSeviyeBtn.IsVisible = false;
            ExcelBtn.IsVisible = false;
            BreadcrumbLabel.Text = "Tüm Satışlar";

            var satislar = await _db.TumSatisleriGetirAsync();

            _mevcutListe = satislar
                .GroupBy(s => s.Tarih.Year)
                .OrderByDescending(g => g.Key)
                .Select(g => new GecmisItem
                {
                    Baslik = $"📅 {g.Key}",
                    AltBaslik = $"{g.Count()} satış",
                    Ciro = g.Sum(s => s.Fiyat),
                    Kar = g.Sum(s => s.Kar),
                    SatisSayisi = g.Count(),
                    Anahtar = g.Key
                }).ToList();

            GecmisListesi.ItemsSource = _mevcutListe;
        }

        private async Task AylariYukle(int yil)
        {
            _seviye = 1;
            _seciliYil = yil;
            UstSeviyeBtn.IsVisible = true;
            ExcelBtn.IsVisible = true;
            ExcelBtn.Text = $"📥 {yil} Yılını Aktar";
            BreadcrumbLabel.Text = $"Tüm Satışlar › {yil}";

            var satislar = await _db.TumSatisleriGetirAsync();

            _mevcutListe = satislar
                .Where(s => s.Tarih.Year == yil)
                .GroupBy(s => s.Tarih.Month)
                .OrderByDescending(g => g.Key)
                .Select(g => new GecmisItem
                {
                    Baslik = $"🗓 {new DateTime(yil, g.Key, 1):MMMM}",
                    AltBaslik = $"{g.Count()} satış",
                    Ciro = g.Sum(s => s.Fiyat),
                    Kar = g.Sum(s => s.Kar),
                    SatisSayisi = g.Count(),
                    Anahtar = g.Key
                }).ToList();

            GecmisListesi.ItemsSource = _mevcutListe;
        }

        private async Task HaftalariYukle(int yil, int ay)
        {
            _seviye = 2;
            _seciliAy = ay;
            UstSeviyeBtn.IsVisible = true;
            ExcelBtn.IsVisible = true;
            ExcelBtn.Text = $"📥 {new DateTime(yil, ay, 1):MMMM}'ı Aktar";
            BreadcrumbLabel.Text = $"Tüm Satışlar › {yil} › {new DateTime(yil, ay, 1):MMMM}";

            var satislar = await _db.TumSatisleriGetirAsync();

            _mevcutListe = satislar
                .Where(s => s.Tarih.Year == yil && s.Tarih.Month == ay)
                .GroupBy(s => System.Globalization.ISOWeek.GetWeekOfYear(s.Tarih))
                .OrderByDescending(g => g.Key)
                .Select(g => new GecmisItem
                {
                    Baslik = $"📆 Hafta {g.Key}",
                    AltBaslik = $"{g.Min(s => s.Tarih):dd MMM} – {g.Max(s => s.Tarih):dd MMM}",
                    Ciro = g.Sum(s => s.Fiyat),
                    Kar = g.Sum(s => s.Kar),
                    SatisSayisi = g.Count(),
                    Anahtar = g.Key
                }).ToList();

            GecmisListesi.ItemsSource = _mevcutListe;
        }

        private async Task GunleriYukle(int yil, int ay, int hafta)
        {
            _seviye = 3;
            _seciliHafta = hafta;
            UstSeviyeBtn.IsVisible = true;
            ExcelBtn.IsVisible = true;
            ExcelBtn.Text = $"📥 Hafta {hafta}'yi Aktar";
            BreadcrumbLabel.Text = $"Tüm Satışlar › {yil} › {new DateTime(yil, ay, 1):MMMM} › Hafta {hafta}";

            var satislar = await _db.TumSatisleriGetirAsync();

            _mevcutListe = satislar
                .Where(s => s.Tarih.Year == yil && s.Tarih.Month == ay &&
                            System.Globalization.ISOWeek.GetWeekOfYear(s.Tarih) == hafta)
                .GroupBy(s => s.Tarih.Date)
                .OrderByDescending(g => g.Key)
                .Select(g => new GecmisItem
                {
                    Baslik = $"📅 {g.Key:dd MMMM dddd}",
                    AltBaslik = $"{g.Count()} satış",
                    Ciro = g.Sum(s => s.Fiyat),
                    Kar = g.Sum(s => s.Kar),
                    SatisSayisi = g.Count(),
                    Anahtar = g.Key.Day
                }).ToList();

            GecmisListesi.ItemsSource = _mevcutListe;
        }

        private async void OnSecimDegisti(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is not GecmisItem secilen) return;
            GecmisListesi.SelectedItem = null;

            switch (_seviye)
            {
                case 0: await AylariYukle(secilen.Anahtar); break;
                case 1: await HaftalariYukle(_seciliYil, secilen.Anahtar); break;
                case 2: await GunleriYukle(_seciliYil, _seciliAy, secilen.Anahtar); break;
                case 3:
                    // Gün detayı — o günün satışlarını göster
                    await GunDetayGoster(secilen.Anahtar);
                    break;
            }
        }

        private async Task GunDetayGoster(int gun)
        {
            var tarih = new DateTime(_seciliYil, _seciliAy, gun);
            var satislar = await _db.GunlukSatislerAsync(tarih);

            string detay = satislar.Count == 0
                ? "Bu gün satış yok."
                : string.Join("\n", satislar.Select(s =>
                    $"• {s.UrunAd} — ₺{s.Fiyat:N2} (Kar: ₺{s.Kar:N2})"));

            await DisplayAlert(
                $"📅 {tarih:dd MMMM yyyy}",
                $"Toplam: ₺{satislar.Sum(s => s.Fiyat):N2}\n" +
                $"Kar: ₺{satislar.Sum(s => s.Kar):N2}\n\n{detay}",
                "Tamam");
        }

        private async void UstSeviyeClicked(object sender, EventArgs e)
        {
            switch (_seviye)
            {
                case 1: await YillariYukle(); break;
                case 2: await AylariYukle(_seciliYil); break;
                case 3: await HaftalariYukle(_seciliYil, _seciliAy); break;
            }
        }

        private async void ExcelAktarClicked(object sender, EventArgs e)
        {
            var satislar = await _db.TumSatisleriGetirAsync();
            List<Satis> filtreli;
            string baslik;

            switch (_seviye)
            {
                case 1:
                    filtreli = satislar.Where(s => s.Tarih.Year == _seciliYil).ToList();
                    baslik = $"{_seciliYil} Yılı Satışları";
                    break;
                case 2:
                    filtreli = satislar.Where(s => s.Tarih.Year == _seciliYil && s.Tarih.Month == _seciliAy).ToList();
                    baslik = $"{new DateTime(_seciliYil, _seciliAy, 1):MMMM yyyy} Satışları";
                    break;
                case 3:
                    filtreli = satislar.Where(s => s.Tarih.Year == _seciliYil && s.Tarih.Month == _seciliAy &&
                                                   System.Globalization.ISOWeek.GetWeekOfYear(s.Tarih) == _seciliHafta).ToList();
                    baslik = $"Hafta {_seciliHafta} Satışları";
                    break;
                default:
                    filtreli = satislar;
                    baslik = "Tüm Satışlar";
                    break;
            }

            if (filtreli.Count == 0)
            {
                await DisplayAlert("Uyarı", "Aktarılacak satış bulunamadı!", "Tamam");
                return;
            }

            string dosyaYolu = _excel.RaporOlustur(filtreli, baslik);
            await DisplayAlert("Başarılı", $"Excel dosyası oluşturuldu:\n{dosyaYolu}", "Tamam");
        }

        private async void GeriClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//Raporlar");
    }

    public class GecmisItem
    {
        public string Baslik { get; set; } = "";
        public string AltBaslik { get; set; } = "";
        public decimal Ciro { get; set; }
        public decimal Kar { get; set; }
        public int SatisSayisi { get; set; }
        public int Anahtar { get; set; }
    }
}