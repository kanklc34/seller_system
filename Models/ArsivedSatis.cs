using SQLite;

namespace Saller_System.Models
{
    [Table("ArsivedSatislar")]
    public class ArsivedSatis
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int OrijinalId { get; set; }

        // HATA ÇÖZÜMÜ: Arşivleme sırasında aranan eksik özellikler eklendi
        public string Barkod { get; set; } = string.Empty;
        public string Ad { get; set; } = string.Empty;
        public string SatisTipi { get; set; } = "PERAKENDE";

        public string UrunAd { get; set; } = "";

        // HATA ÇÖZÜMÜ: int yerine decimal yapıldı (Terazi gramajları için)
        public decimal Adet { get; set; }

        public decimal Fiyat { get; set; }
        public decimal Kar { get; set; }
        public DateTime Tarih { get; set; }
        public string? KasiyerAd { get; set; }
        public DateTime ArsivedAt { get; set; } = DateTime.Now;

        public ArsivedSatis() { }

        public ArsivedSatis(Satis s)
        {
            OrijinalId = s.Id;
            UrunAd = s.UrunAd ?? "";

            // Yeni eklenenlerin eşleştirmesi (Veri kaybı olmasın diye)
            Barkod = s.Barkod ?? "";
            Ad = s.Ad ?? "";
            SatisTipi = s.SatisTipi ?? "PERAKENDE";

            // Buradaki atama artık hata vermeyecek
            Adet = s.Adet;

            Fiyat = s.Fiyat;
            Kar = s.Kar;
            Tarih = s.Tarih;
            KasiyerAd = s.KasiyerAd;
            ArsivedAt = DateTime.Now;
        }
    }
}