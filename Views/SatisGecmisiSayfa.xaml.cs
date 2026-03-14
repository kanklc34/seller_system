using Saller_System.Services;
using Saller_System.Models; // Modelleri kullanabilmek için ekledik

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

        // ANDROID FİZİKSEL GERİ TUŞU DESTEĞİ
        protected override bool OnBackButtonPressed()
        {
            if (_seviye > 0)
            {
                UstSeviyeClicked(null, null);
                return true;
            }
            Dispatcher.Dispatch(async () => await Shell.Current.GoToAsync("//Raporlar"));
            return true;
        }

        private async Task YillariYukle()
        {
            _seviye = 0;
            UstSeviyeHeader.IsVisible = false;
            ExcelBtn.IsVisible = false;
            BreadcrumbLabel.Text = "Tüm Satışlar";

            var satislar = await _db.TumSatisleriGetirAsync();
            _mevcutListe = satislar.GroupBy(s => s.Tarih.Year).OrderByDescending(g => g.Key).Select(g => new GecmisItem
            {
                Baslik = $"📅 {g.Key} Yılı",
                AltBaslik = "Yıllık Toplam Özet",
                Ciro = g.Sum(s => s.Fiyat),
                Kar = g.Sum(s => s.Kar),
                SatisSayisi = g.Count(),
                Anahtar = g.Key
            }).ToList();
            GecmisListesi.ItemsSource = _mevcutListe;
        }

        private async Task AylariYukle(int yil)
        {
            _seviye = 1; _seciliYil = yil;
            UstSeviyeHeader.IsVisible = true;
            ExcelBtn.IsVisible = true;
            ExcelBtn.Text = "📥 Yılı Aktar";
            BreadcrumbLabel.Text = $"Tüm Satışlar › {yil}";

            var satislar = await _db.TumSatisleriGetirAsync();
            _mevcutListe = satislar.Where(s => s.Tarih.Year == yil).GroupBy(s => s.Tarih.Month).OrderByDescending(g => g.Key).Select(g => new GecmisItem
            {
                Baslik = $"🗓 {new DateTime(yil, g.Key, 1):MMMM}",
                AltBaslik = $"{yil} dönemi",
                Ciro = g.Sum(s => s.Fiyat),
                Kar = g.Sum(s => s.Kar),
                SatisSayisi = g.Count(),
                Anahtar = g.Key
            }).ToList();
            GecmisListesi.ItemsSource = _mevcutListe;
        }

        private async Task HaftalariYukle(int yil, int ay)
        {
            _seviye = 2; _seciliAy = ay;
            UstSeviyeHeader.IsVisible = true;
            ExcelBtn.Text = "📥 Ayı Aktar";
            BreadcrumbLabel.Text = $"Tüm Satışlar › {yil} › {new DateTime(yil, ay, 1):MMMM}";

            var satislar = await _db.TumSatisleriGetirAsync();
            _mevcutListe = satislar.Where(s => s.Tarih.Year == yil && s.Tarih.Month == ay).GroupBy(s => System.Globalization.ISOWeek.GetWeekOfYear(s.Tarih)).OrderByDescending(g => g.Key).Select(g => new GecmisItem
            {
                Baslik = $"📆 {g.Key}. Hafta",
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
            _seviye = 3; _seciliHafta = hafta;
            UstSeviyeHeader.IsVisible = true;
            ExcelBtn.Text = "📥 Haftayı Aktar";
            BreadcrumbLabel.Text = $"Tüm Satışlar › {yil} › {new DateTime(yil, ay, 1):MMMM} › {hafta}. Hafta";

            var satislar = await _db.TumSatisleriGetirAsync();
            _mevcutListe = satislar.Where(s => s.Tarih.Year == yil && s.Tarih.Month == ay && System.Globalization.ISOWeek.GetWeekOfYear(s.Tarih) == hafta).GroupBy(s => s.Tarih.Date).OrderByDescending(g => g.Key).Select(g => new GecmisItem
            {
                Baslik = $"📅 {g.Key:dd MMMM dddd}",
                AltBaslik = "Günlük Detay",
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
                case 3: await GunDetayGoster(secilen.Anahtar); break;
            }
        }

        private async Task GunDetayGoster(int gun)
        {
            var tarih = new DateTime(_seciliYil, _seciliAy, gun);
            var satislar = await _db.GunlukSatislerAsync(tarih);
            string detay = satislar.Count == 0 ? "Satış yok." : string.Join("\n", satislar.Select(s => $"• {s.UrunAd}: ₺{s.Fiyat:N2}"));
            await DisplayAlert($"{tarih:dd MMMM yyyy}", $"Toplam Ciro: ₺{satislar.Sum(s => s.Fiyat):N2}\nToplam Kâr: ₺{satislar.Sum(s => s.Kar):N2}\n\n{detay}", "Tamam");
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
                case 1: filtreli = satislar.Where(s => s.Tarih.Year == _seciliYil).ToList(); baslik = $"{_seciliYil}_Yili_Raporu"; break;
                case 2: filtreli = satislar.Where(s => s.Tarih.Year == _seciliYil && s.Tarih.Month == _seciliAy).ToList(); baslik = $"{_seciliAy}_{_seciliYil}_Aylik_Rapor"; break;
                case 3: filtreli = satislar.Where(s => s.Tarih.Year == _seciliYil && s.Tarih.Month == _seciliAy && System.Globalization.ISOWeek.GetWeekOfYear(s.Tarih) == _seciliHafta).ToList(); baslik = $"Hafta_{_seciliHafta}_Raporu"; break;
                default: filtreli = satislar; baslik = "Tum_Satislar_Raporu"; break;
            }

            if (filtreli.Count == 0) return;

            try
            {
                string dosyaYolu = _excel.RaporOlustur(filtreli, baslik);
                await Share.Default.RequestAsync(new ShareFileRequest { Title = baslik, File = new ShareFile(dosyaYolu) });
            }
            catch (Exception ex) { await DisplayAlert("Hata", ex.Message, "Tamam"); }
        }

        private async void GeriClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//Raporlar");
    }
}