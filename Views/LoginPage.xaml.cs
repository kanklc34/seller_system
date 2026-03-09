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
            HataLabel.IsVisible = false;
            HataBorder.IsVisible = false;

            string kullanici = KullaniciAdiEntry.Text?.Trim() ?? "";
            string sifre = SifreEntry.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(kullanici) || string.IsNullOrWhiteSpace(sifre))
            {
                HataLabel.Text = "Kullanıcı adı ve şifre boş bırakılamaz!";
                HataLabel.IsVisible = true;
                HataBorder.IsVisible = true;
                return;
            }

            await _db.InitAsync();
            var bulunanKullanici = await _db.GirisKontrolAsync(kullanici, sifre);

            if (bulunanKullanici != null)
            {
                OturumServisi.AktifKullanici = bulunanKullanici;
                await Shell.Current.GoToAsync("//AnaSayfa");
            }
            else
            {
                HataLabel.Text = "Kullanıcı adı veya şifre hatalı!";
                HataLabel.IsVisible = true;
                HataBorder.IsVisible = true;
            }
        }
    }
}