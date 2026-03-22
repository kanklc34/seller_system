using Saller_System.Models;
using System;

namespace Saller_System.Services
{
    /// <summary>
    /// Aktif kullanıcı oturumunu ve zamanaşımını yönetir.
    /// Varsayılan zamanaşımı: 15 dakika hareketsizlik.
    /// </summary>
    public static class OturumServisi
    {
        // ----------------------------------------------------------------
        // Zamanaşımı süresi (dakika)
        // ----------------------------------------------------------------
        public static int ZamanAsimDakika { get; set; } = 15;

        private static Kullanici? _aktifKullanici;
        private static DateTime _sonAktivite = DateTime.MinValue;

        // ----------------------------------------------------------------
        // Aktif kullanıcı — getter oturum süresi kontrolü yapar
        // ----------------------------------------------------------------
        public static Kullanici? AktifKullanici
        {
            get
            {
                if (_aktifKullanici == null) return null;

                if (OturumSuresiDolduMu())
                {
                    Cikis();     // Otomatik çıkış
                    return null;
                }

                return _aktifKullanici;
            }
        }

        // ----------------------------------------------------------------
        // Giriş
        // ----------------------------------------------------------------
        public static void Giris(Kullanici kullanici)
        {
            _aktifKullanici = kullanici;
            _sonAktivite = DateTime.Now;
        }

        // ----------------------------------------------------------------
        // Çıkış
        // ----------------------------------------------------------------
        public static void Cikis()
        {
            _aktifKullanici = null;
            _sonAktivite = DateTime.MinValue;
        }

        // ----------------------------------------------------------------
        // Aktiviteyi yenile — her ekran etkileşiminde çağrılmalı
        // ----------------------------------------------------------------
        public static void AktiviteYenile()
        {
            if (_aktifKullanici != null)
                _sonAktivite = DateTime.Now;
        }

        // ----------------------------------------------------------------
        // Zamanaşımı kontrolü
        // ----------------------------------------------------------------
        public static bool OturumSuresiDolduMu()
        {
            if (_aktifKullanici == null) return false;
            return (DateTime.Now - _sonAktivite).TotalMinutes >= ZamanAsimDakika;
        }

        // Kalan süre (saniye) — UI'da geri sayım için
        public static int KalanSaniye()
        {
            if (_aktifKullanici == null) return 0;
            var gecen = (DateTime.Now - _sonAktivite).TotalSeconds;
            var toplam = ZamanAsimDakika * 60;
            return Math.Max(0, (int)(toplam - gecen));
        }

        // ----------------------------------------------------------------
        // Rol kontrolleri
        // ----------------------------------------------------------------
        public static bool GirisYapildiMi => AktifKullanici != null;
        public static bool AdminMi => AktifKullanici?.Rol is "Patron";
        public static bool YoneticiMi => AktifKullanici?.Rol is "Patron" or "Müdür";
        public static bool CalisanMi => AktifKullanici != null;
    }
}