using Saller_System.Services;

namespace Saller_System.Views
{
    public partial class Raporlar : ContentPage
    {
        private readonly DatabaseService _db;
        private readonly ExcelServisi _excel;

        public Raporlar(DatabaseService db, ExcelServisi excel)
        {
            InitializeComponent();
            _db = db;
            _excel = excel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _db.InitAsync();
            var gunlukKar = await _db.GunlukKarAsync(DateTime.Today);
            var aylikKar = await _db.AylikKarAsync(DateTime.Today.Year, DateTime.Today.Month);
            var gunlukCiro = await _db.GunlukCiroAsync(DateTime.Today);
            var gunlukSayi = await _db.GunlukSatisSayisiAsync(DateTime.Today);
            var aylikCiro = await _db.AylikCiroAsync(DateTime.Today.Year, DateTime.Today.Month);
            GunlukKarLabel.Text = $"₺{gunlukKar:N2}";
            AylikKarLabel.Text = $"₺{aylikKar:N2}";
            GunlukCiroLabel.Text = $"₺{gunlukCiro:N2}";
            GunlukSayiLabel.Text = $"{gunlukSayi} adet";
            AylikCiroLabel.Text = $"₺{aylikCiro:N2}";
            AyLabel.Text = DateTime.Today.ToString("MMMM yyyy");
        }

        private async void SatisGecmisiClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//SatisGecmisiSayfa");

        private async void ExcelAktarClicked(object sender, EventArgs e)
        {
            string secim = await DisplayActionSheet(
                "Excel'e Aktar", "İptal", null,
                "📅 Bugünkü Satışlar",
                "📆 Bu Ay");

            if (secim == null || secim == "İptal") return;

            await _db.InitAsync();
            List<Saller_System.Models.Satis> satislar;
            string baslik;

            if (secim == "📅 Bugünkü Satışlar")
            {
                satislar = await _db.GunlukSatislerAsync(DateTime.Today);
                baslik = $"Günlük Rapor {DateTime.Today:dd.MM.yyyy}";
            }
            else
            {
                satislar = await _db.AylikSatislerAsync(DateTime.Today.Year, DateTime.Today.Month);
                baslik = $"Aylık Rapor {DateTime.Today:MMMM yyyy}";
            }

            if (satislar.Count == 0)
            {
                await DisplayAlert("Uyarı", "Aktarılacak satış bulunamadı!", "Tamam");
                return;
            }

            string dosyaYolu = _excel.RaporOlustur(satislar, baslik);
            await DisplayAlert("Başarılı", $"Excel dosyası oluşturuldu:\n{dosyaYolu}", "Tamam");
        }

        private async void GeriClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//AnaSayfa");
    }
}