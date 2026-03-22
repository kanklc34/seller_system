using System;
using System.Security.Cryptography;
using System.Text;

namespace Saller_System.Services
{
    /// <summary>
    /// Parola güvenliği servisi.
    /// PBKDF2 (SHA-256, 310.000 iterasyon, 16 byte salt) kullanır.
    /// NIST SP 800-132 ve OWASP önerilerine uygundur.
    /// </summary>
    public static class GuvenlikServisi
    {
        private const int SaltBoyutu = 16;       // 128 bit
        private const int HashBoyutu = 32;       // 256 bit
        private const int Iterasyon = 310_000;   // OWASP 2023 önerisi
        private const char Ayirici = ':';

        // ----------------------------------------------------------------
        // PBKDF2 ile hash oluştur
        // Dönen format: "salt_hex:hash_hex"
        // ----------------------------------------------------------------
        public static string Hashle(string sifre)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltBoyutu);

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password: Encoding.UTF8.GetBytes(sifre),
                salt: salt,
                iterations: Iterasyon,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: HashBoyutu
            );

            return $"{Convert.ToHexString(salt).ToLower()}{Ayirici}{Convert.ToHexString(hash).ToLower()}";
        }

        // ----------------------------------------------------------------
        // Parola doğrulama (timing-safe karşılaştırma)
        // ----------------------------------------------------------------
        public static bool Dogrula(string sifre, string kayitliHash)
        {
            if (string.IsNullOrWhiteSpace(kayitliHash))
                return false;

            // Eski düz-metin kayıtları için geçici uyumluluk katmanı.
            // Kullanıcı başarıyla giriş yapınca hash otomatik güncellenir.
            if (!kayitliHash.Contains(Ayirici))
                return sifre == kayitliHash;   // düz metin karşılaştır

            var parcalar = kayitliHash.Split(Ayirici);
            if (parcalar.Length != 2) return false;

            byte[] salt, beklenenHash;
            try
            {
                salt = Convert.FromHexString(parcalar[0]);
                beklenenHash = Convert.FromHexString(parcalar[1]);
            }
            catch
            {
                return false;
            }

            var girilenHash = Rfc2898DeriveBytes.Pbkdf2(
                password: Encoding.UTF8.GetBytes(sifre),
                salt: salt,
                iterations: Iterasyon,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: HashBoyutu
            );

            // Zamanlama saldırısına karşı sabit süreli karşılaştırma
            return CryptographicOperations.FixedTimeEquals(girilenHash, beklenenHash);
        }

        // ----------------------------------------------------------------
        // Mevcut hash'in PBKDF2 formatında olup olmadığını kontrol et
        // (Düz-metin veya eski SHA-256 kayıtlarını tespit için kullanılır)
        // ----------------------------------------------------------------
        public static bool HashGuncelMi(string kayitliHash) =>
            !string.IsNullOrWhiteSpace(kayitliHash) && kayitliHash.Contains(Ayirici);
    }
}