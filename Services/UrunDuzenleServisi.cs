using Saller_System.Models;

namespace Saller_System.Services
{
    public static class UrunDuzenleServisi
    {
        // Barkod sayfasından gelen hızlı ekleme barkodu için
        public static string? HizliEkleBarkod { get; set; }

        // Ürün listesinden düzenleme sayfasına gönderilen ürün için
        public static Urun? SeciliUrun { get; set; }
    }
}