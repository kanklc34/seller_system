using SQLite;
using Saller_System.Models;

namespace Saller_System.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _db;

        public async Task InitAsync()
        {
            if (_db != null) return;

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "saller.db");
            _db = new SQLiteAsyncConnection(dbPath);

            await _db.CreateTableAsync<Urun>();
            await _db.CreateTableAsync<Satis>();

            // 30 günden eski satışları temizle
            var otuzGunOnce = DateTime.Now.AddDays(-30);
            await _db.Table<Satis>()
                     .Where(s => s.Tarih < otuzGunOnce)
                     .DeleteAsync();
            await _db!.CreateTableAsync<Kullanici>();

            // Varsayılan kullanıcıları ekle (ilk çalıştırmada)
            var adminVar = await _db!.Table<Kullanici>()
                .Where(k => k.KullaniciAdi == "admin").FirstOrDefaultAsync();

            if (adminVar == null)
            {
                await _db!.InsertAsync(new Kullanici { KullaniciAdi = "admin", Sifre = "1234", Rol = "Admin" });
                await _db!.InsertAsync(new Kullanici { KullaniciAdi = "yonetici", Sifre = "1234", Rol = "Yonetici" });
                await _db!.InsertAsync(new Kullanici { KullaniciAdi = "kasiyer", Sifre = "1234", Rol = "Calisan" });
            }
        }

        // ── ÜRÜN İŞLEMLERİ ──────────────────────────
        public async Task<List<Urun>> TumUrunleriGetirAsync()
      => await _db!.Table<Urun>().ToListAsync();

        public async Task<Urun?> BarkodIleGetirAsync(string barkod)
            => await _db!.Table<Urun>().Where(u => u.Barkod == barkod).FirstOrDefaultAsync();

        public async Task UrunEkleAsync(Urun urun)
            => await _db!.InsertAsync(urun);

        public async Task UrunGuncelleAsync(Urun urun)
            => await _db!.UpdateAsync(urun);

        public async Task UrunSilAsync(Urun urun)
            => await _db!.DeleteAsync(urun);

        public async Task SatisKaydetAsync(Satis satis)
            => await _db!.InsertAsync(satis);

        public async Task<List<Satis>> TumSatisleriGetirAsync()
            => await _db!.Table<Satis>().ToListAsync();

        public async Task<List<Satis>> GunlukSatislerAsync(DateTime tarih)
            => await _db!.Table<Satis>()
                        .Where(s => s.Tarih.Date == tarih.Date)
                        .ToListAsync();
        // ── KULLANICI İŞLEMLERİ ─────────────────────────
        public async Task<Kullanici?> GirisKontrolAsync(string kullaniciAdi, string sifre)
            => await _db!.Table<Kullanici>()
                         .Where(k => k.KullaniciAdi == kullaniciAdi && k.Sifre == sifre)
                         .FirstOrDefaultAsync();

        public async Task<List<Kullanici>> TumKullanicilariGetirAsync()
            => await _db!.Table<Kullanici>().ToListAsync();

        public async Task KullaniciEkleAsync(Kullanici kullanici)
            => await _db!.InsertAsync(kullanici);

        public async Task KullaniciSilAsync(Kullanici kullanici)
            => await _db!.DeleteAsync(kullanici);
        // ── İSTATİSTİK İŞLEMLERİ ────────────────────────
        public async Task<decimal> GunlukCiroAsync(DateTime tarih)
        {
            var satislar = await GunlukSatislerAsync(tarih);
            return satislar.Sum(s => s.Fiyat * s.Adet);
        }

        public async Task<decimal> AylikCiroAsync(int yil, int ay)
        {
            var satislar = await _db!.Table<Satis>()
                .Where(s => s.Tarih.Year == yil && s.Tarih.Month == ay)
                .ToListAsync();
            return satislar.Sum(s => s.Fiyat * s.Adet);
        }

        public async Task<int> GunlukSatisSayisiAsync(DateTime tarih)
        {
            var satislar = await GunlukSatislerAsync(tarih);
            return satislar.Sum(s => s.Adet);
        }
    }
}