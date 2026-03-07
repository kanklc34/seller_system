namespace Saller_System.Views
{
    public partial class AnaSayfa : ContentPage
    {
        public AnaSayfa()
        {
            InitializeComponent();
        }

        private async void BarkodOkutClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//BarkodSayfa");
        }

        private async void UrunListesiClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//UrunListesi");
        }

        private async void RaporlarClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Raporlar");
        }

        private async void CikisClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}