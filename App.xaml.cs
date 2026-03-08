namespace Saller_System
{
    public partial class App : Application
    {
        public App(Saller_System.Services.AyarlarServisi ayarlar)
        {
            InitializeComponent();
            _ = TemaYukleAsync(ayarlar);
        }

        private async Task TemaYukleAsync(Saller_System.Services.AyarlarServisi ayarlar)
        {
            var darkMode = await ayarlar.GetAsync("DarkMode", "0");
            UserAppTheme = darkMode == "1" ? AppTheme.Dark : AppTheme.Light;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}