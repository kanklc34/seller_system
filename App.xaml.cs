using Saller_System.Services;

namespace Saller_System
{
    public partial class App : Application
    {
        private readonly AyarlarServisi _ayarlar;

        // Her 60 saniyede bir zamanaşımı kontrolü
        private const int KontrolAraligi = 60_000;
        private IDispatcherTimer? _zamanasimiTimer;

        public App(AyarlarServisi ayarlar)
        {
            InitializeComponent();
            _ayarlar = ayarlar;
            ZamanasimiTimerBaslat();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var shell = new AppShell();
            var window = new Window(shell);

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

        // ----------------------------------------------------------------
        // Arka plan zamanaşımı timer'ı
        // Her dakika çalışır; oturum dolmuşsa login'e atar
        // ----------------------------------------------------------------
        private void ZamanasimiTimerBaslat()
        {
            _zamanasimiTimer = Dispatcher.CreateTimer();
            _zamanasimiTimer.Interval = TimeSpan.FromMilliseconds(KontrolAraligi);
            _zamanasimiTimer.Tick += async (s, e) => await ZamanasimiKontrolEt();
            _zamanasimiTimer.Start();
        }

        private async Task ZamanasimiKontrolEt()
        {
            // Oturum açık değilse kontrol etmeye gerek yok
            if (!OturumServisi.GirisYapildiMi) return;

            // Zamanaşımı dolmadıysa geç
            if (!OturumServisi.OturumSuresiDolduMu()) return;

            OturumServisi.Cikis();

            // UI thread'de çalıştır
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert(
                        "Oturum Süresi Doldu",
                        "Güvenlik nedeniyle oturumunuz sonlandırıldı.",
                        "Tamam"
                    );
                    await Shell.Current.GoToAsync("//LoginPage");
                }
            });
        }

        protected override void CleanUp()
        {
            _zamanasimiTimer?.Stop();
            base.CleanUp();
        }
    }
}