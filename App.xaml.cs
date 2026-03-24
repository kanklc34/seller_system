using Saller_System.Services;

namespace Saller_System
{
    public partial class App : Application
    {
        private readonly AyarlarServisi _ayarlar;
        private readonly DatabaseService _db;
        private const int KontrolAraligi = 60_000;
        private IDispatcherTimer? _zamanasimiTimer;

        public App(AyarlarServisi ayarlar, DatabaseService db)
        {
            InitializeComponent();
            _ayarlar = ayarlar;
            _db = db;
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
                {
                    await Shell.Current.GoToAsync("//KurulumSihirbazi");
                    return;
                }

                // Arka planda arşivleme çalıştır
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _db.InitAsync();
                        await _db.EskiSatisleriArsivleAsync();
                    }
                    catch { /* Sessizce geç */ }
                });

                // Kaydedilmiş oturum varsa ana sayfaya
                if (OturumServisi.OturumuGeriYukle())
                    await Shell.Current.GoToAsync("//AnaSayfa");
                else
                    await Shell.Current.GoToAsync("//LoginPage");
            };

            return window;
        }

        private void ZamanasimiTimerBaslat()
        {
            _zamanasimiTimer = Dispatcher.CreateTimer();
            _zamanasimiTimer.Interval = TimeSpan.FromMilliseconds(KontrolAraligi);
            _zamanasimiTimer.Tick += async (s, e) => await ZamanasimiKontrolEt();
            _zamanasimiTimer.Start();
        }

        private async Task ZamanasimiKontrolEt()
        {
            if (!OturumServisi.GirisYapildiMi) return;
            if (!OturumServisi.OturumSuresiDolduMu()) return;

            OturumServisi.Cikis();

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert(
                        "Oturum Süresi Doldu",
                        "Güvenlik nedeniyle oturumunuz sonlandırıldı.",
                        "Tamam");
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