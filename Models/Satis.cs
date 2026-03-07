using SQLite;

namespace Saller_System.Models
{
    public class Satis
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int UrunId { get; set; }
        public string UrunAd { get; set; } = string.Empty;
        public decimal Fiyat { get; set; }
        public int Adet { get; set; }
        public DateTime Tarih { get; set; }
        public string KasiyerAd { get; set; } = string.Empty;
    }
}