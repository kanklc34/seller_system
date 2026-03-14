using Saller_System.Models;

namespace Saller_System.Services
{
    public static class OturumServisi
    {
        public static Kullanici? AktifKullanici { get; set; }

        public static bool AdminMi => AktifKullanici?.Rol == "Admin";
        public static bool YoneticiMi => AktifKullanici?.Rol is "Admin" or "Yonetici";
        public static bool CalisanMi => AktifKullanici != null;
    }
}