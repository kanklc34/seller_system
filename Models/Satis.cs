using SQLite;

namespace Saller_System.Models
{
    public class Satis
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int UrunId { get; set; }
        public string UrunAd { get; set; } = string.Empty;
        public decimal Fiyat { get; set; }      // Satış fiyatı (toplam)
        public decimal AlisFiyati { get; set; } = 0; // Alış fiyatı (toplam)
        public int Adet { get; set; }
        public DateTime Tarih { get; set; }
        public string KasiyerAd { get; set; } = string.Empty;

        // Hesaplanan kar
        public decimal Kar => Fiyat - AlisFiyati;
    }
}