namespace Saller_System.Models
{
    public class SepetItem
    {
        public Urun Urun { get; set; } = new();

        // DEĞİŞİKLİK: Gramajlı olabilmesi için int yerine decimal oldu
        public decimal Adet { get; set; } = 1;

        public decimal OzelFiyat { get; set; } = 0;

        // DEĞİŞİKLİK: Fiyat (veya Kg Fiyatı) x Miktar olarak tam doğru çarpım yapıyor
        public decimal Toplam => OzelFiyat > 0 ? OzelFiyat * Adet : Urun.Fiyat * Adet;
    }
}