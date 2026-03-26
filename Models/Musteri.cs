using SQLite;

namespace Saller_System.Models
{
    // VERESİYE İÇİN MÜŞTERİ MODELİ
    public class Musteri
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string AdSoyad { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public decimal ToplamBorc { get; set; } = 0;

        // EKRANDA GÖRÜNECEK ÖZEL CÜMLE (Veritabanına kaydedilmez, sadece ekranda görünür)
        [Ignore]
        public string DurumMetni
        {
            get
            {
                if (ToplamBorc > 0)
                    return $"Müşteriden {ToplamBorc:N2} TL alınacak.";
                else if (ToplamBorc < 0)
                    return $"Müşteriye {Math.Abs(ToplamBorc):N2} TL borçluyuz.";
                else
                    return "Hesap Kapalı (Alacak/Borç Yok)";
            }
        }

        // EKRANDA GÖRÜNECEK RENK
        [Ignore]
        public Color DurumRengi => ToplamBorc >= 0 ? Color.FromArgb("#16A34A") : Color.FromArgb("#E31E24");
    }

    public class VeresiyeIslem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int MusteriId { get; set; }
        public decimal Tutar { get; set; }
        public DateTime Tarih { get; set; }
        public string Aciklama { get; set; } = string.Empty;
        public bool OdendiMi { get; set; } = false;
    }

    public class ToptanSatis
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string AliciFirma { get; set; } = string.Empty;
        public decimal ToplamTutar { get; set; }
        public decimal ToplamKg { get; set; }
        public DateTime Tarih { get; set; }
        public string OdemeYontemi { get; set; } = string.Empty;
    }
}