using Saller_System.Models;
using Saller_System.Services;

namespace Saller_System.Views
{
    public partial class UrunListesi : ContentPage
    {
        private readonly DatabaseService _db;
        private List<Urun> _tumUrunler;

        public UrunListesi(DatabaseService db)
        {
            InitializeComponent();
            _db = db;
            TarihLabel.Text = DateTime.Now.Year.ToString();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await VerileriYukle();
        }

        protected override bool OnBackButtonPressed()
        {
            Dispatcher.Dispatch(async () => await Shell.Current.GoToAsync("//AnaSayfa"));
            return true;
        }

        private async Task VerileriYukle()
        {
            await _db.InitAsync();
            _tumUrunler = await _db.TumUrunleriGetirAsync();
            UrunlerListesi.ItemsSource = _tumUrunler;
        }

        private void UrunAraTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_tumUrunler == null) return;
            var arama = e.NewTextValue?.ToLower() ?? "";
            UrunlerListesi.ItemsSource = _tumUrunler.Where(u => u.Ad.ToLower().Contains(arama) || u.Barkod.Contains(arama)).ToList();
        }

        private void GramajliToggled(object sender, ToggledEventArgs e)
        {
            FiyatEntry.Placeholder = e.Value ? "Kg Satış ₺" : "Satış ₺";
            AlisFiyatiEntry.Placeholder = e.Value ? "Kg Alış ₺" : "Alış ₺";
        }

        private async void KaydetClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AdEntry.Text) || string.IsNullOrWhiteSpace(BarkodEntry.Text))
            {
                await DisplayAlert("Hata", "Ad ve Barkod boş geçilemez!", "Tamam");
                return;
            }

            var urun = new Urun
            {
                Ad = AdEntry.Text,
                Barkod = BarkodEntry.Text,
                Kategori = KategoriEntry.Text ?? "Genel",
                GramajliMi = GramajliSwitch.IsToggled,
                Fiyat = GramajliSwitch.IsToggled ? 0 : decimal.Parse(FiyatEntry.Text ?? "0"),
                KgFiyati = GramajliSwitch.IsToggled ? decimal.Parse(FiyatEntry.Text ?? "0") : 0,
                AlisFiyati = GramajliSwitch.IsToggled ? 0 : decimal.Parse(AlisFiyatiEntry.Text ?? "0"),
                KgAlisFiyati = GramajliSwitch.IsToggled ? decimal.Parse(AlisFiyatiEntry.Text ?? "0") : 0
            };

            await _db.UrunEkleAsync(urun);
            AdEntry.Text = BarkodEntry.Text = KategoriEntry.Text = FiyatEntry.Text = AlisFiyatiEntry.Text = "";
            await VerileriYukle();
            await DisplayAlert("Başarılı", "Ürün eklendi.", "Tamam");
        }

        private async void UrunDuzenleClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Urun urun)
            {
                UrunDuzenleServisi.SeciliUrun = urun;
                await Shell.Current.GoToAsync("//UrunDuzenle");
            }
        }

        private async void UrunSilClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Urun urun)
            {
                bool onay = await DisplayAlert("Sil", $"{urun.Ad} silinecek?", "Evet", "Hayır");
                if (onay) { await _db.UrunSilAsync(urun); await VerileriYukle(); }
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

        private async void GeriClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//AnaSayfa");
    }
}