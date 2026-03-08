using SQLite;

namespace Saller_System.Models
{
    public class FiyatGecmisi
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int UrunId { get; set; }
        public string UrunAd { get; set; } = string.Empty;
        public decimal EskiFiyat { get; set; }
        public decimal YeniFiyat { get; set; }
        public DateTime Tarih { get; set; }
        public string DegistirenKullanici { get; set; } = string.Empty;
    }
}