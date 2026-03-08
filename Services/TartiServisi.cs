namespace Saller_System.Services
{
    public static class TartiServisi
    {
        // Bilinen prefix listesi
        private static readonly string[] BilinenpPrefixler = { "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "2" };

        public static (string UrunKodu, decimal Kg, bool TartiUrunuMu) BarkodCoz(string barkod, string? ozelPrefix = null)
        {
            if (barkod.Length != 13) return (barkod, 0, false);

            // Özel prefix varsa onu dene
            if (!string.IsNullOrEmpty(ozelPrefix))
            {
                var sonuc = PrefixIleCoz(barkod, ozelPrefix);
                if (sonuc.TartiUrunuMu) return sonuc;
            }

            // Bilinen prefixleri otomatik dene
            foreach (var prefix in BilinenpPrefixler)
            {
                var sonuc = PrefixIleCoz(barkod, prefix);
                if (sonuc.TartiUrunuMu) return sonuc;
            }

            return (barkod, 0, false);
        }

        private static (string UrunKodu, decimal Kg, bool TartiUrunuMu) PrefixIleCoz(string barkod, string prefix)
        {
            if (!barkod.StartsWith(prefix)) return (barkod, 0, false);

            int kodBaslangic = prefix.Length;
            int kodUzunluk = 5;
            int gramBaslangic = kodBaslangic + kodUzunluk;
            int gramUzunluk = 5;

            if (gramBaslangic + gramUzunluk > 12) return (barkod, 0, false);

            string urunKodu = barkod.Substring(kodBaslangic, kodUzunluk);
            string gramStr = barkod.Substring(gramBaslangic, gramUzunluk);

            if (decimal.TryParse(gramStr, out decimal gram) && gram > 0)
                return (urunKodu, gram / 1000m, true);

            return (barkod, 0, false);
        }

        // Barkoddan prefix'i otomatik algıla
        public static string? PrefixAlgila(string barkod)
        {
            if (barkod.Length != 13) return null;

            foreach (var prefix in BilinenpPrefixler)
            {
                var sonuc = PrefixIleCoz(barkod, prefix);
                if (sonuc.TartiUrunuMu) return prefix;
            }
            return null;
        }

        public static decimal FiyatHesapla(decimal kgFiyati, decimal kg)
            => Math.Round(kgFiyati * kg, 2);
    }
}