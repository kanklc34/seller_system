using Saller_System.Services;

namespace Saller_System.Views
{
    public partial class FiyatGecmisiSayfa : ContentPage
    {
        private readonly DatabaseService _db;

        public FiyatGecmisiSayfa(DatabaseService db)
        {
            InitializeComponent();
            _db = db;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            var urun = UrunDuzenleServisi.SeciliUrun;
            if (urun == null) return;

            UrunAdLabel.Text = $"{urun.Ad} fiyat deðiþimleri";
            await _db.InitAsync();
            var gecmis = await _db.UrunFiyatGecmisiAsync(urun.Id);
            GecmisListesi.ItemsSource = gecmis;
        }

        private async void GeriClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//UrunListesi");
    }
}