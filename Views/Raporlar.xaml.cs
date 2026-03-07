using Saller_System.Services;

namespace Saller_System.Views
{
    public partial class Raporlar : ContentPage
    {
        private readonly DatabaseService _db;

        public Raporlar(DatabaseService db)
        {
            InitializeComponent();
            _db = db;
        }

        private async void BugunkuSatislarClicked(object sender, EventArgs e)
        {
            await _db.InitAsync();
            var satislar = await _db.GunlukSatislerAsync(DateTime.Today);
            SatisListesi.ItemsSource = satislar;
        }

        private async void TumSatislarClicked(object sender, EventArgs e)
        {
            await _db.InitAsync();
            var satislar = await _db.TumSatisleriGetirAsync();
            SatisListesi.ItemsSource = satislar;
        }

        private async void GeriClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//AnaSayfa");
        }
    }
}