using Saller_System.Services;

namespace Saller_System.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly DatabaseService _db;
        private readonly AyarlarServisi _ayarlar;

        private int _hataliDeneme = 0;
        private DateTime _kilitBitiZaman = DateTime.MinValue;
        private const int MaxDeneme = 5;
        private const int KilitSaniye = 30;

        public LoginPage(DatabaseService db, AyarlarServisi ayarlar)
        {
            InitializeComponent();
            _db = db;
            _ayarlar = ayarlar;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // 🖼️ GÖRSELİ YÜKLE
            var kaydedilenGorsel = await _ayarlar.GetAsync("DukkanArkaPlan", "dukkan_fotogece.jpg");

            if (LoginArkaPlanGorseli != null)
            {
                // Eğer varsayılan değer değilse, galeriden gelen bir yoldur (FromFile kullanmalıyız)
                if (kaydedilenGorsel != "dukkan_fotogece.jpg")
                {
                    // Dosya yolundan yüklediğimizi sisteme kesin olarak belirtiyoruz
                    LoginArkaPlanGorseli.Source = ImageSource.FromFile(kaydedilenGorsel);
                }
                else
                {
                    // Resources/Images içindeki varsayılan resmi yükle
                    LoginArkaPlanGorseli.Source = ImageSource.FromFile("dukkan_fotogece.jpg");
                }
            }

            var magazaAdi = await _ayarlar.GetAsync("MagazaAdi", "");
            MagazaAdiLabel.Text = string.IsNullOrWhiteSpace(magazaAdi)
                ? "KASAP PRO" : magazaAdi.ToUpper();
            TelTelLabel.Text = string.IsNullOrWhiteSpace(magazaAdi)
                ? "© 2026 Kasap Pro" : $"© 2026 {magazaAdi}";

            KullaniciAdiEntry.Text = string.Empty;
            SifreEntry.Text = string.Empty;
            HataBorder.IsVisible = false;
            _hataliDeneme = 0;
            _kilitBitiZaman = DateTime.MinValue;
        }

        private async void GirisYapClicked(object sender, EventArgs e)
        {
            HataBorder.IsVisible = false;

            if (DateTime.Now < _kilitBitiZaman)
            {
                var kalan = (int)(_kilitBitiZaman - DateTime.Now).TotalSeconds;
                HataLabel.Text = $"Çok fazla hatalı deneme. {kalan} saniye bekleyin.";
                HataBorder.IsVisible = true;
                return;
            }

            string kullanici = KullaniciAdiEntry.Text?.Trim() ?? "";
            string sifre = SifreEntry.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(kullanici) || string.IsNullOrEmpty(sifre))
            {
                HataLabel.Text = "Lütfen bilgilerinizi eksiksiz girin.";
                HataBorder.IsVisible = true;
                return;
            }

            try
            {
                await _db.InitAsync();
                var bulunanKullanici = await _db.GirisKontrolAsync(kullanici, sifre);

                if (bulunanKullanici != null)
                {
                    _hataliDeneme = 0;
                    OturumServisi.Giris(bulunanKullanici);
                    await Shell.Current.GoToAsync("//AnaSayfa");
                }
                else
                {
                    _hataliDeneme++;
                    int kalan = MaxDeneme - _hataliDeneme;

                    if (_hataliDeneme >= MaxDeneme)
                    {
                        _kilitBitiZaman = DateTime.Now.AddSeconds(KilitSaniye);
                        _hataliDeneme = 0;
                        HataLabel.Text = $"Çok fazla hatalı deneme! {KilitSaniye} saniye beklemeniz gerekiyor.";
                    }
                    else
                    {
                        HataLabel.Text = kalan == 1
                            ? "Hatalı giriş! Son 1 deneme hakkınız kaldı."
                            : $"Hatalı giriş! {kalan} deneme hakkınız kaldı.";
                    }

                    HataBorder.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Giriş Hatası", "Hata: " + ex.Message, "Tamam");
            }
        }
    }
}