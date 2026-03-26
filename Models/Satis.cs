using SQLite;

namespace Saller_System.Models
{
    public class Satis
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int UrunId { get; set; }

        // HATA ÇÖZÜMÜ: DatabaseService'in aradığı eksik özellikler eklendi
        public string Barkod { get; set; } = string.Empty;
        public string Ad { get; set; } = string.Empty;

        public string UrunAd { get; set; } = string.Empty;
        public decimal Fiyat { get; set; }
        public decimal AlisFiyati { get; set; }
        public decimal Kar { get; set; }
        public decimal Adet { get; set; } // Kilo/Miktar
        public DateTime Tarih { get; set; }
        public string KasiyerAd { get; set; } = string.Empty;

        // BUNU YENİ EKLEDİK (Toptan mı Perakende mi ayırmak için)
        public string SatisTipi { get; set; } = "PERAKENDE";
    }
}