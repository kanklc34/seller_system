using Saller_System.Services;
using Saller_System.Models;

namespace Saller_System.Views
{
    public partial class UrunListesi : ContentPage
    {
        private readonly DatabaseService _db;
        public bool IsYonetici { get; set; }

        public UrunListesi(DatabaseService db)
        {
            InitializeComponent();
            _db = db;
            TarihLabel.Text = DateTime.Now.ToString("yyyy");
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _db.InitAsync();
            await ListeYukle();

            if (!string.IsNullOrEmpty(UrunDuzenleServisi.HizliEkleBarkod))
            {
                BarkodEntry.Text = UrunDuzenleServisi.HizliEkleBarkod;
                UrunDuzenleServisi.HizliEkleBarkod = null;
                AdEntry.Focus();
            }

            var rol = OturumServisi.AktifKullanici?.Rol;
            bool yoneticiMi = (rol == "Patron" || rol == "Müdür" || OturumServisi.AktifKullanici?.KullaniciAdi == "admin");

            // Personel hızlı ekleme yapsın diye paneli herkese açıyoruz 
            // ama silme ikonlarını hala IsYonetici ile kısıtlıyoruz.
            YeniUrunPaneli.IsVisible = true;
            IsYonetici = yoneticiMi;
            OnPropertyChanged(nameof(IsYonetici));
        }

        private async Task ListeYukle()
        {
            UrunlerListesi.ItemsSource = await _db.TumUrunleriGetirAsync();
        }

        private async void KaydetClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AdEntry.Text)) return;

            var urun = new Urun
            {
                Ad = AdEntry.Text,
                Barkod = BarkodEntry.Text,
                Kategori = KategoriEntry.Text,
                Fiyat = decimal.TryParse(FiyatEntry.Text, out var f) ? f : 0,
                AlisFiyati = decimal.TryParse(AlisFiyatiEntry.Text, out var a) ? a : 0,
                GramajliMi = GramajliSwitch.IsToggled
            };

            await _db.UrunEkleAsync(urun);
            AdEntry.Text = BarkodEntry.Text = KategoriEntry.Text = FiyatEntry.Text = AlisFiyatiEntry.Text = "";
            await ListeYukle();
        }

        private async void UrunAraTextChanged(object sender, TextChangedEventArgs e)
        {
            var liste = await _db.TumUrunleriGetirAsync();
            UrunlerListesi.ItemsSource = string.IsNullOrWhiteSpace(e.NewTextValue)
                ? liste : liste.Where(u => u.Ad.ToLower().Contains(e.NewTextValue.ToLower()));
        }

        private async void UrunSilClicked(object sender, EventArgs e)
        {
            if (((Button)sender).CommandParameter is Urun urun)
            {
                if (await DisplayAlert("SİL", $"{urun.Ad} silinsin mi?", "Evet", "Hayır"))
                {
                    await _db.UrunSilAsync(urun);
                    await ListeYukle();
                }
            }
        }

        private async void UrunDuzenleClicked(object sender, EventArgs e)
        {
            if (((Button)sender).CommandParameter is Urun secilenUrun)
            {
                UrunDuzenleServisi.SeciliUrun = secilenUrun;
                await Shell.Current.GoToAsync("//UrunDuzenle");
            }
        }

        private async void GeriClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//AnaSayfa");
    }
}