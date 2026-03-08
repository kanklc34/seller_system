using Saller_System.Models;
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
            string kullanici = KullaniciAdiEntry.Text?.Trim() ?? "";
            string sifre = SifreEntry.Text?.Trim() ?? "";

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
            }
        }
    }
}