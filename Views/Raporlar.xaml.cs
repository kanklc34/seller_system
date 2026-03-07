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

            var gunlukCiro = await _db.GunlukCiroAsync(DateTime.Today);
            var gunlukSayi = await _db.GunlukSatisSayisiAsync(DateTime.Today);
            var aylikCiro = await _db.AylikCiroAsync(DateTime.Today.Year, DateTime.Today.Month);

            GunlukCiroLabel.Text = $"₺{gunlukCiro:N2}";
            GunlukSayiLabel.Text = $"{gunlukSayi} adet";
            AylikCiroLabel.Text = $"₺{aylikCiro:N2}";
            AyLabel.Text = DateTime.Today.ToString("MMMM yyyy");
        }

        private async void BugunkuSatislarClicked(object sender, EventArgs e)
        {
            var satislar = await _db.GunlukSatislerAsync(DateTime.Today);
            SatisListesi.ItemsSource = satislar;
        }

        private async void TumSatislarClicked(object sender, EventArgs e)
        {
            var satislar = await _db.TumSatisleriGetirAsync();
            SatisListesi.ItemsSource = satislar;
        }

        private async void ExcelAktarClicked(object sender, EventArgs e)
        {
            await _db.InitAsync();
            var satislar = await _db.TumSatisleriGetirAsync();

            if (satislar.Count == 0)
            {
                await DisplayAlert("Uyarı", "Aktarılacak satış bulunamadı!", "Tamam");
                return;
            }

            string dosyaYolu = _excel.RaporOlustur(satislar, "Tüm Satışlar Raporu");
            await DisplayAlert("Başarılı", $"Excel dosyası oluşturuldu:\n{dosyaYolu}", "Tamam");
        }

        private async void GeriClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//AnaSayfa");
        }
    }
}