using Saller_System.Services;

namespace Saller_System.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly DatabaseService _db;

        public LoginPage(DatabaseService db)
        {
            InitializeComponent();
            _db = db;
        }

        private async void GirisYapClicked(object sender, EventArgs e)
        {
            HataBorder.IsVisible = false;

            string kullanici = KullaniciAdiEntry.Text?.Trim();
            string sifre = SifreEntry.Text?.Trim();

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
                    OturumServisi.AktifKullanici = bulunanKullanici;
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Shell.Current.GoToAsync("//AnaSayfa");
                    });
                }
                else
                {
                    HataLabel.Text = "Kullanıcı adı veya şifre hatalı!";
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