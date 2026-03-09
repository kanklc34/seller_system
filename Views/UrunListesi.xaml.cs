using Saller_System.Models;
using Saller_System.Services;

namespace Saller_System.Views
{
    public partial class UrunListesi : ContentPage
    {
        private readonly DatabaseService _db;
        public bool YoneticiMi => OturumServisi.YoneticiMi;

        public UrunListesi(DatabaseService db)
        {
            InitializeComponent();
            _db = db;
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _db.InitAsync();
            var urunler = await _db.TumUrunleriGetirAsync();
            UrunlerListesi.ItemsSource = urunler;
            YeniUrunBtn.IsVisible = OturumServisi.YoneticiMi;
            OnPropertyChanged(nameof(YoneticiMi));
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
            if (sender is TapGestureRecognizer tap && tap.CommandParameter is Urun urun)
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
            if (sender is TapGestureRecognizer tap && tap.CommandParameter is Urun urun)
            {
                bool onay = await DisplayAlert("Onay", $"{urun.Ad} silinsin mi?", "Evet", "Hayır");
                if (onay)
                {
                    await _db.UrunSilAsync(urun);
                    UrunlerListesi.ItemsSource = await _db.TumUrunleriGetirAsync();
                }
            }
        }

        private async void FiyatGecmisiClicked(object sender, EventArgs e)
        {
            if (sender is TapGestureRecognizer tap && tap.CommandParameter is Urun urun)
            {
                UrunDuzenleServisi.SeciliUrun = urun;
                await Shell.Current.GoToAsync("//FiyatGecmisiSayfa");
            }
        }

        private async void GeriClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//AnaSayfa");
    }
}