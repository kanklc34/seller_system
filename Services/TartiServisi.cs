namespace Saller_System.Services
{
    public static class TartiServisi
    {
        // Gram için mantıklı aralık: 1g - 30kg
        private const decimal MinKg = 0.001m;
        private const decimal MaxKg = 30m;

        // Desteklenen formatlar: (prefixUzunluk, kodUzunluk, gramBaslangic, gramUzunluk)
        private static readonly (int prefix, int kod, int gramBas, int gramUzun)[] Formatlar =
        {
            (1, 6, 7, 5),   // 2 + 6 kod + 5 gram (Bizerba tipi)
            (1, 5, 6, 5),   // 2 + 5 kod + 5 gram (Mettler tipi)
            (1, 4, 5, 6),   // 2 + 4 kod + 6 gram
            (1, 5, 6, 4),   // 2 + 5 kod + 4 gram
            (1, 4, 5, 5),   // 2 + 4 kod + 5 gram
            (1, 6, 7, 4),   // 2 + 6 kod + 4 gram
            (2, 5, 7, 5),   // 20/21.. + 5 kod + 5 gram
            (2, 4, 6, 5),   // 20/21.. + 4 kod + 5 gram
        };

        public static (string UrunKodu, decimal Kg, bool TartiUrunuMu) BarkodCoz(string barkod, string? ozelPrefix = null)
        {
            if (barkod.Length != 13) return (barkod, 0, false);
            if (!barkod.StartsWith("2")) return (barkod, 0, false);

            // Özel prefix varsa önce onu dene
            if (!string.IsNullOrEmpty(ozelPrefix))
            {
                foreach (var fmt in Formatlar)
                {
                    if (fmt.prefix == ozelPrefix.Length)
                    {
                        var sonuc = FormatCoz(barkod, fmt.prefix, fmt.kod, fmt.gramBas, fmt.gramUzun);
                        if (sonuc.TartiUrunuMu) return sonuc;
                    }
                }
            }

            // Tüm formatları dene, mantıklı sonucu döndür
            foreach (var fmt in Formatlar)
            {
                var sonuc = FormatCoz(barkod, fmt.prefix, fmt.kod, fmt.gramBas, fmt.gramUzun);
                if (sonuc.TartiUrunuMu) return sonuc;
            }

            return (barkod, 0, false);
        }

        private static (string UrunKodu, decimal Kg, bool TartiUrunuMu) FormatCoz(
            string barkod, int prefixUzunluk, int kodUzunluk, int gramBaslangic, int gramUzunluk)
        {
            if (gramBaslangic + gramUzunluk > 12) return (barkod, 0, false);

            string urunKodu = barkod.Substring(prefixUzunluk, kodUzunluk);
            string gramStr = barkod.Substring(gramBaslangic, gramUzunluk);

            if (!decimal.TryParse(gramStr, out decimal gram)) return (barkod, 0, false);

            decimal kg = gram / 1000m;

            // Mantıklı aralık kontrolü
            if (kg < MinKg || kg > MaxKg) return (barkod, 0, false);

            return (urunKodu, kg, true);
        }

        public static string? PrefixAlgila(string barkod)
        {
            if (barkod.Length != 13) return null;
            if (barkod.StartsWith("2")) return "2";
            return null;
        }

        public static decimal FiyatHesapla(decimal kgFiyati, decimal kg)
            => Math.Round(kgFiyati * kg, 2);
    }
}