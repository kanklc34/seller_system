namespace Saller_System.Models
{
    public class SepetItem
    {
        public Urun Urun { get; set; } = new();
        public int Adet { get; set; } = 1;
        public decimal OzelFiyat { get; set; } = 0;
        public decimal Toplam => OzelFiyat > 0 ? OzelFiyat : Urun.Fiyat * Adet;
    }
}