namespace Saller_System.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void GirisYapClicked(object sender, EventArgs e)
        {
            string kullanici = KullaniciAdiEntry.Text?.Trim() ?? "";
            string sifre = SifreEntry.Text?.Trim() ?? "";

            // Þimdilik sabit kullanýcýlar, ilerleyen adýmda veritabanýna taþýrýz
            if (kullanici == "admin" && sifre == "1234")
            {
                await Shell.Current.GoToAsync("//AnaSayfa");
            }
            else
            {
                HataLabel.Text = "Kullanýcý adý veya þifre hatalý!";
                HataLabel.IsVisible = true;
            }
        }
    }
}