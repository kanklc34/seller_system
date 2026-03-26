using Saller_System.Services;
using Saller_System.Models;

namespace Saller_System.Views
{
    public partial class UrunListesi : ContentPage
    {
        private readonly DatabaseService _db;
        public bool IsYonetici { get; set; }

        // Debounce için
        private CancellationTokenSource? _aramaCts;

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

            if (await ZamanAsimKontrolAsync()) return;

            OturumServisi.AktiviteYenile();
            await _db.InitAsync();
            await ListeYukle();

            if (!string.IsNullOrEmpty(UrunDuzenleServisi.HizliEkleBarkod))
            {
                BarkodEntry.Text = UrunDuzenleServisi.HizliEkleBarkod;
                UrunDuzenleServisi.HizliEkleBarkod = null;
                AdEntry.Focus();
            }

            var rol = OturumServisi.AktifKullanici?.Rol;
            bool yoneticiMi = rol == "Patron" || rol == "Müdür"
                           || OturumServisi.AktifKullanici?.KullaniciAdi == "admin";

            YeniUrunPaneli.IsVisible = true;
            IsYonetici = yoneticiMi;
            OnPropertyChanged(nameof(IsYonetici));
        }

        protected override bool OnBackButtonPressed()
        {
            OturumServisi.AktiviteYenile();
            Dispatcher.Dispatch(async () => await Shell.Current.GoToAsync("//AnaSayfa"));
            return true;
        }

        private async Task<bool> ZamanAsimKontrolAsync()
        {
            if (!OturumServisi.OturumSuresiDolduMu()) return false;
            OturumServisi.Cikis();
            await DisplayAlert("Oturum Süresi Doldu", "Güvenlik nedeniyle oturumunuz sonlandırıldı.", "Tamam");
            await Shell.Current.GoToAsync("//LoginPage");
            return true;
        }

        private async Task ListeYukle() =>
            UrunlerListesi.ItemsSource = await _db.TumUrunleriGetirAsync();

        private async void KaydetClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AdEntry.Text))
            {
                await DisplayAlert("Hata", "Lütfen en azından bir ürün adı girin.", "Tamam");
                return;
            }

            OturumServisi.AktiviteYenile();

            // Fiyatları sayıya güvenli bir şekilde çevir
            decimal.TryParse(FiyatEntry.Text, out decimal girilenSatis);
            decimal.TryParse(AlisFiyatiEntry.Text, out decimal girilenAlis);
            bool isGramajli = GramajliSwitch.IsToggled;

            var urun = new Urun
            {
                Ad = AdEntry.Text,
                Barkod = BarkodEntry.Text,
                Kategori = KategoriEntry.Text,
                GramajliMi = isGramajli,

                // YENİ MANTIK: Eğer gramajlıysa değerleri Kg kısmına, değilse Normal kısma at.
                Fiyat = isGramajli ? 0 : girilenSatis,
                AlisFiyati = isGramajli ? 0 : girilenAlis,
                KgFiyati = isGramajli ? girilenSatis : 0,
                KgAlisFiyati = isGramajli ? girilenAlis : 0,

                StokMiktari = 0 // Yeni ürün stoğu her zaman 0 başlar
            };

            await _db.UrunEkleAsync(urun);

            // Formu temizle
            AdEntry.Text = BarkodEntry.Text = KategoriEntry.Text = FiyatEntry.Text = AlisFiyatiEntry.Text = "";
            GramajliSwitch.IsToggled = false;

            await ListeYukle();
        }

        // Debounce — 300ms bekle, sonra ara
        private async void UrunAraTextChanged(object sender, TextChangedEventArgs e)
        {
            OturumServisi.AktiviteYenile();

            _aramaCts?.Cancel();
            _aramaCts = new CancellationTokenSource();
            var token = _aramaCts.Token;

            try
            {
                await Task.Delay(300, token);

                var aramaMetni = e.NewTextValue?.Trim() ?? "";
                if (string.IsNullOrEmpty(aramaMetni))
                {
                    UrunlerListesi.ItemsSource = await _db.TumUrunleriGetirAsync();
                }
                else
                {
                    var liste = await _db.TumUrunleriGetirAsync();
                    UrunlerListesi.ItemsSource = liste
                        .Where(u => u.Ad.ToLower().Contains(aramaMetni.ToLower())
                                 || (u.Barkod?.Contains(aramaMetni) ?? false));
                }
            }
            catch (TaskCanceledException)
            {
                // Yeni tuşa basıldı, bu arama iptal edildi — normal
            }
        }

        private async void UrunSilClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
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
            OturumServisi.AktiviteYenile();
            if (((Button)sender).CommandParameter is Urun secilenUrun)
            {
                UrunDuzenleServisi.SeciliUrun = secilenUrun;
                await Shell.Current.GoToAsync("//UrunDuzenle");
            }
        }

        private async void GeriClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            await Shell.Current.GoToAsync("//AnaSayfa");
        }
    }
}