using SQLite;

namespace Saller_System.Models
{
    public class Kullanici
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string KullaniciAdi { get; set; } = string.Empty;
        public string Sifre { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        // Roller: "Admin", "Yonetici", "Calisan"
    }
}