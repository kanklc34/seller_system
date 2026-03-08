using SQLite;

namespace Saller_System.Models
{
    public class Ayarlar
    {
        [PrimaryKey]
        public string Anahtar { get; set; } = string.Empty;
        public string Deger { get; set; } = string.Empty;
    }
}