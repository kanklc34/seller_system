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
            => await Shell.Current.GoToAsync("//UrunEkle");

        private async void UrunDuzenleClicked(object sender, EventArgs e)
        {
            if (!OturumServisi.YoneticiMi)
            {
                await DisplayAlert("Yetkisiz", "Düzenleme için yönetici yetkisi gerekli!", "Tamam");
                return;
            }
            if (sender is Button btn && btn.CommandParameter is Urun urun)
            {
                UrunDuzenleServisi.SeciliUrun = urun;
                await Shell.Current.GoToAsync("//UrunDuzenle");
            }
        }

        private async void UrunSilClicked(object sender, EventArgs e)
        {
            if (!OturumServisi.YoneticiMi)
            {
                await DisplayAlert("Yetkisiz", "Silme için yönetici yetkisi gerekli!", "Tamam");
                return;
            }
            if (sender is Button btn && btn.CommandParameter is Urun urun)
            {
                bool onay = await DisplayAlert("Onay", $"{urun.Ad} silinsin mi?", "Evet", "Hayýr");
                if (onay)
                {
                    await _db.UrunSilAsync(urun);
                    UrunlerListesi.ItemsSource = await _db.TumUrunleriGetirAsync();
                }
            }
        }

        private async void FiyatGecmisiClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Urun urun)
            {
                UrunDuzenleServisi.SeciliUrun = urun;
                await Shell.Current.GoToAsync("//FiyatGecmisiSayfa");
            }
        }

        private async void GeriClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//AnaSayfa");
    }
}