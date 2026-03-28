using SQLite;

namespace Saller_System.Models
{
    [Table("ArsivedSatislar")]
    public class ArsivedSatis
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int OrijinalId { get; set; }

        public string Barkod { get; set; } = string.Empty;
        public string Ad { get; set; } = string.Empty;
        public string SatisTipi { get; set; } = "PERAKENDE";

        public string UrunAd { get; set; } = "";

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

            Barkod = s.Barkod ?? "";
            Ad = s.Ad ?? "";
            SatisTipi = s.SatisTipi ?? "PERAKENDE";

            Adet = s.Adet;

            Fiyat = s.Fiyat;
            Kar = s.Kar;
            Tarih = s.Tarih;
            KasiyerAd = s.KasiyerAd;
            ArsivedAt = DateTime.Now;
        }
    }
}