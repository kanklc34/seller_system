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
        }

        // ── ÜRÜN İŞLEMLERİ ──────────────────────────
        public async Task<List<Urun>> TumUrunleriGetirAsync()
      => await _db!.Table<Urun>().ToListAsync();

        public async Task<Urun> BarkodIleGetirAsync(string barkod)
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
    }
}