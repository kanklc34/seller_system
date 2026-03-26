using SQLite;

namespace Saller_System.Models
{
    public class Gider
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Baslik { get; set; } = string.Empty; // Kira, Elektrik, Poşet vb.
        public decimal Tutar { get; set; }
        public DateTime Tarih { get; set; }
        public string Aciklama { get; set; } = string.Empty;
    }
}