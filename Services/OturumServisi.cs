using Saller_System.Models;

namespace Saller_System.Services
{
    /// <summary>
    /// Aktif kullanıcı oturumunu yönetir.
    /// Uygulama kapanıp açılsa bile oturum korunur (Preferences).
    /// Çıkış yapılmadıkça hesap açık kalır.
    /// </summary>
    public static class OturumServisi
    {
        private const string KullaniciAdiKey = "oturum_kullanici_adi";
        private const string KullaniciRolKey = "oturum_kullanici_rol";
        private const string KullaniciIdKey = "oturum_kullanici_id";

        public static int ZamanAsimDakika { get; set; } = 15;

        private static Kullanici? _aktifKullanici;
        private static DateTime _sonAktivite = DateTime.MinValue;

        public static Kullanici? AktifKullanici => _aktifKullanici;

        // Giriş — bellekte ve Preferences'da sakla
        public static void Giris(Kullanici kullanici)
        {
            _aktifKullanici = kullanici;
            _sonAktivite = DateTime.Now;

            Preferences.Set(KullaniciAdiKey, kullanici.KullaniciAdi);
            Preferences.Set(KullaniciRolKey, kullanici.Rol ?? "");
            Preferences.Set(KullaniciIdKey, kullanici.Id);
        }

        // Uygulama açılışında kaydedilmiş oturumu geri yükle
        public static bool OturumuGeriYukle()
        {
            var adi = Preferences.Get(KullaniciAdiKey, "");
            var rol = Preferences.Get(KullaniciRolKey, "");
            var id = Preferences.Get(KullaniciIdKey, 0);

            if (string.IsNullOrEmpty(adi)) return false;

            _aktifKullanici = new Kullanici
            {
                Id = id,
                KullaniciAdi = adi,
                Rol = rol,
                Sifre = ""
            };
            _sonAktivite = DateTime.Now;
            return true;
        }

        // Çıkış — bellekten ve Preferences'dan sil
        public static void Cikis()
        {
            _aktifKullanici = null;
            _sonAktivite = DateTime.MinValue;

            Preferences.Remove(KullaniciAdiKey);
            Preferences.Remove(KullaniciRolKey);
            Preferences.Remove(KullaniciIdKey);
        }

        public static void AktiviteYenile()
        {
            if (_aktifKullanici != null)
                _sonAktivite = DateTime.Now;
        }

        public static bool OturumSuresiDolduMu()
        {
            if (_aktifKullanici == null) return false;
            return (DateTime.Now - _sonAktivite).TotalMinutes >= ZamanAsimDakika;
        }

        public static int KalanSaniye()
        {
            if (_aktifKullanici == null) return 0;
            var gecen = (DateTime.Now - _sonAktivite).TotalSeconds;
            var toplam = ZamanAsimDakika * 60;
            return Math.Max(0, (int)(toplam - gecen));
        }

        public static bool GirisYapildiMi => _aktifKullanici != null;
        public static bool AdminMi => _aktifKullanici?.Rol is "Patron";
        public static bool YoneticiMi => _aktifKullanici?.Rol is "Patron" or "Müdür";
        public static bool CalisanMi => _aktifKullanici != null;
    }
}