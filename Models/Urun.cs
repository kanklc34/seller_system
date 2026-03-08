using SQLite;

namespace Saller_System.Models
{
    public class Urun
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Barkod { get; set; } = string.Empty;
        public string Ad { get; set; } = string.Empty;
        public decimal Fiyat { get; set; }
        public string Kategori { get; set; } = string.Empty;
        public bool GramajliMi { get; set; } = false;
        public decimal KgFiyati { get; set; } = 0;
    }
}