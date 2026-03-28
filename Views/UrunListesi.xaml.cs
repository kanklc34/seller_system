using Saller_System.Services;
using Saller_System.Models;

namespace Saller_System.Views
{
    public partial class UrunListesi : ContentPage
    {
        private readonly DatabaseService _db;

        private bool _isYonetici;
        public bool IsYonetici
        {
            get => _isYonetici;
            set { _isYonetici = value; OnPropertyChanged(); }
        }

        private CancellationTokenSource? _aramaCts;

        public UrunListesi(DatabaseService db)
        {
            InitializeComponent();
            _db = db;
            TarihLabel.Text = DateTime.Now.Year.ToString();
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Güvenlik Kontrolü
            if (OturumServisi.OturumSuresiDolduMu())
            {
                OturumServisi.Cikis();
                await Shell.Current.GoToAsync("//LoginPage");
                return;
            }

            OturumServisi.AktiviteYenile();
            await _db.InitAsync();
            await ListeYukle();

            // Hızlı Ekleme Kontrolü
            if (!string.IsNullOrEmpty(UrunDuzenleServisi.HizliEkleBarkod))
            {
                BarkodEntry.Text = UrunDuzenleServisi.HizliEkleBarkod;
                UrunDuzenleServisi.HizliEkleBarkod = null;
                AdEntry.Focus();
            }

            // Yetki Kontrolü
            var rol = OturumServisi.AktifKullanici?.Rol;
            IsYonetici = rol == "Patron" || rol == "Müdür" || OturumServisi.AktifKullanici?.KullaniciAdi == "admin";
        }

        private async Task ListeYukle()
        {
            try
            {
                var urunler = await _db.TumUrunleriGetirAsync();
                UrunlerListesi.ItemsSource = urunler;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", "Liste yüklenirken sorun oluştu: " + ex.Message, "Tamam");
            }
        }

        private async void KaydetClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AdEntry.Text))
            {
                await DisplayAlert("Hata", "Lütfen ürün adı girin.", "Tamam");
                return;
            }

            decimal.TryParse(FiyatEntry.Text, out decimal girilenSatis);
            decimal.TryParse(AlisFiyatiEntry.Text, out decimal girilenAlis);
            bool isGramajli = GramajliSwitch.IsToggled;

            var urun = new Urun
            {
                Ad = AdEntry.Text,
                Barkod = BarkodEntry.Text,
                Kategori = KategoriEntry.Text,
                GramajliMi = isGramajli,
                Fiyat = isGramajli ? 0 : girilenSatis,
                AlisFiyati = isGramajli ? 0 : girilenAlis,
                KgFiyati = isGramajli ? girilenSatis : 0,
                KgAlisFiyati = isGramajli ? girilenAlis : 0,
                StokMiktari = 0
            };

            await _db.UrunEkleAsync(urun);

            // Temizlik
            AdEntry.Text = BarkodEntry.Text = KategoriEntry.Text = FiyatEntry.Text = AlisFiyatiEntry.Text = "";
            GramajliSwitch.IsToggled = false;

            await ListeYukle();
        }

        private async void UrunAraTextChanged(object sender, TextChangedEventArgs e)
        {
            _aramaCts?.Cancel();
            _aramaCts = new CancellationTokenSource();
            var token = _aramaCts.Token;

            try
            {
                await Task.Delay(300, token);
                var aramaMetni = e.NewTextValue?.Trim().ToLower() ?? "";
                var tumUrunler = await _db.TumUrunleriGetirAsync();

                if (string.IsNullOrEmpty(aramaMetni))
                    UrunlerListesi.ItemsSource = tumUrunler;
                else
                    UrunlerListesi.ItemsSource = tumUrunler.Where(u => u.Ad.ToLower().Contains(aramaMetni) || (u.Barkod?.Contains(aramaMetni) ?? false));
            }
            catch (TaskCanceledException) { }
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