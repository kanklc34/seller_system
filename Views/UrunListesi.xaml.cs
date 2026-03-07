using Saller_System.Models;
using Saller_System.Services;

namespace Saller_System.Views
{
    public partial class UrunListesi : ContentPage
    {
        private readonly DatabaseService _db;

        public UrunListesi(DatabaseService db)
        {
            InitializeComponent();
            _db = db;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _db.InitAsync();
            var urunler = await _db.TumUrunleriGetirAsync();
            UrunlerListesi.ItemsSource = urunler;
        }

        private async void YeniUrunClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//UrunEkle");
        }

        private async void GeriClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//AnaSayfa");
        }
    }
}