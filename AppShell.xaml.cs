namespace Saller_System
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        protected override bool OnBackButtonPressed()
        {
            var currentRoute = Shell.Current.CurrentState.Location.ToString();

            if (currentRoute.Contains("KurulumSihirbazi") || currentRoute.Contains("SplashSayfa"))
                return base.OnBackButtonPressed();

            // Login ve Ana sayfada çıkış sor
            if (currentRoute.Contains("LoginPage") || currentRoute.Contains("AnaSayfa"))
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    bool cikis = await Shell.Current.DisplayAlert(
                        "Çıkış", "Uygulamadan çıkmak istiyor musunuz?", "Evet", "Hayır");
                    if (cikis)
                        Application.Current?.Quit();
                });
                return true;
            }

            // Diğer sayfalarda bir önceki sayfaya dön
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.GoToAsync("..");
            });
            return true;
        }
    }
}