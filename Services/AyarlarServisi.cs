using Saller_System.Models;
using SQLite;

namespace Saller_System.Services
{
    public class AyarlarServisi
    {
        private SQLiteAsyncConnection? _db;

        public async Task InitAsync()
        {
            if (_db != null) return;
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "saller.db");
            _db = new SQLiteAsyncConnection(dbPath);
            await _db.CreateTableAsync<Ayarlar>();
        }

        public async Task<string> GetAsync(string anahtar, string varsayilan = "")
        {
            await InitAsync();
            var ayar = await _db!.Table<Ayarlar>()
                                  .Where(a => a.Anahtar == anahtar)
                                  .FirstOrDefaultAsync();
            return ayar?.Deger ?? varsayilan;
        }

        public async Task SetAsync(string anahtar, string deger)
        {
            await InitAsync();
            var mevcut = await _db!.Table<Ayarlar>()
                                    .Where(a => a.Anahtar == anahtar)
                                    .FirstOrDefaultAsync();
            if (mevcut != null)
            {
                mevcut.Deger = deger;
                await _db!.UpdateAsync(mevcut);
            }
            else
            {
                await _db!.InsertAsync(new Ayarlar { Anahtar = anahtar, Deger = deger });
            }
        }
    }
}