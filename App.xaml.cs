using Saller_System.Services;

namespace Saller_System
{
    public partial class App : Application
    {
        private readonly AyarlarServisi _ayarlar;

        public App(AyarlarServisi ayarlar)
        {
            InitializeComponent();
            _ayarlar = ayarlar;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var shell = new AppShell();
            var window = new Window(shell);

            // Shell hazır olduktan sonra kontrol et
            shell.Loaded += async (s, e) =>
            {
                var darkMode = await _ayarlar.GetAsync("DarkMode", "0");
                UserAppTheme = darkMode == "1" ? AppTheme.Dark : AppTheme.Light;

                var kurulum = await _ayarlar.GetAsync("KurulumTamamlandi", "0");

                if (kurulum != "1")
                    await Shell.Current.GoToAsync("//KurulumSihirbazi");
                else
                    await Shell.Current.GoToAsync("//LoginPage");
            };

            return window;
        }
    }

}
