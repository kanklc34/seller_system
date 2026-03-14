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
            await _db.CreateTableAsync<FiyatGecmisi>();
            await _db.CreateTableAsync<Kullanici>();

            var adminVar = await _db.Table<Kullanici>().Where(k => k.KullaniciAdi == "admin").FirstOrDefaultAsync();
            if (adminVar == null)
            {
                // ARTIK HASH YOK, DİREKT 1234 OLARAK KAYDEDİYORUZ
                await _db.InsertAsync(new Kullanici { KullaniciAdi = "admin", Sifre = "1234", Rol = "Patron" });
                await _db.InsertAsync(new Kullanici { KullaniciAdi = "yonetici", Sifre = "1234", Rol = "Müdür" });
            }
            else
            {
                // ESKİ UZUN ŞİFREYİ SİLİP DÜMDÜZ 1234 YAPIYORUZ
                adminVar.Sifre = "1234";
                adminVar.Rol = "Patron";
                await _db.UpdateAsync(adminVar);
            }
        }

        public async Task<List<Urun>> TumUrunleriGetirAsync() => await _db!.Table<Urun>().ToListAsync();
        public async Task<Urun?> BarkodIleGetirAsync(string barkod) => await _db!.Table<Urun>().Where(u => u.Barkod == barkod).FirstOrDefaultAsync();
        public async Task UrunEkleAsync(Urun urun) => await _db!.InsertAsync(urun);
        public async Task UrunSilAsync(Urun urun) => await _db!.DeleteAsync(urun);

        public async Task UrunGuncelleAsync(Urun yeniUrun, Urun eskiUrun)
        {
            if (eskiUrun.Fiyat != yeniUrun.Fiyat || eskiUrun.KgFiyati != yeniUrun.KgFiyati)
            {
                await FiyatGecmisiKaydetAsync(new FiyatGecmisi
                {
                    UrunId = yeniUrun.Id,
                    UrunAd = yeniUrun.Ad,
                    EskiFiyat = eskiUrun.GramajliMi ? eskiUrun.KgFiyati : eskiUrun.Fiyat,
                    YeniFiyat = yeniUrun.GramajliMi ? yeniUrun.KgFiyati : yeniUrun.Fiyat,
                    Tarih = DateTime.Now,
                    DegistirenKullanici = OturumServisi.AktifKullanici?.KullaniciAdi ?? "Bilinmiyor"
                });
            }
            await _db!.UpdateAsync(yeniUrun);
        }

        public async Task SatisKaydetAsync(Satis satis) => await _db!.InsertAsync(satis);
        public async Task<List<Satis>> TumSatisleriGetirAsync() => await _db!.Table<Satis>().ToListAsync();
        public async Task<List<Satis>> GunlukSatislerAsync(DateTime tarih)
        {
            var bas = tarih.Date; var bit = bas.AddDays(1);
            return await _db!.Table<Satis>().Where(s => s.Tarih >= bas && s.Tarih < bit).ToListAsync();
        }

        public async Task<decimal> GunlukCiroAsync(DateTime tarih) => (await GunlukSatislerAsync(tarih)).Sum(s => s.Fiyat);
        public async Task<decimal> GunlukKarAsync(DateTime tarih) => (await GunlukSatislerAsync(tarih)).Sum(s => s.Kar);
        public async Task<int> GunlukSatisSayisiAsync(DateTime tarih) => (await GunlukSatislerAsync(tarih)).Sum(s => s.Adet);

        public async Task<List<Satis>> AylikSatislerAsync(int yil, int ay)
        {
            var bas = new DateTime(yil, ay, 1); var bit = bas.AddMonths(1);
            return await _db!.Table<Satis>().Where(s => s.Tarih >= bas && s.Tarih < bit).ToListAsync();
        }
        public async Task<decimal> AylikCiroAsync(int yil, int ay) => (await AylikSatislerAsync(yil, ay)).Sum(s => s.Fiyat);
        public async Task<decimal> AylikKarAsync(int yil, int ay) => (await AylikSatislerAsync(yil, ay)).Sum(s => s.Kar);

        public async Task<List<PersonelPerformans>> PersonelPerformansRaporuGetirAsync(DateTime tarih)
        {
            var satislar = await GunlukSatislerAsync(tarih);
            return satislar.GroupBy(s => s.KasiyerAd).Select(g => new PersonelPerformans
            {
                PersonelAdi = g.Key ?? "Bilinmeyen",
                ToplamCiro = g.Sum(s => s.Fiyat),
                SatisSayisi = g.Count()
            }).OrderByDescending(x => x.ToplamCiro).ToList();
        }

        // HASHLEME SİSTEMİ KALKTI - DİREKT DÜZ METİN OLARAK KONTROL EDİYORUZ
        public async Task<Kullanici?> GirisKontrolAsync(string ad, string sifre)
        {
            return await _db!.Table<Kullanici>().Where(k => k.KullaniciAdi == ad && k.Sifre == sifre).FirstOrDefaultAsync();
        }

        public async Task<List<Kullanici>> TumKullanicilariGetirAsync() => await _db!.Table<Kullanici>().ToListAsync();
        public async Task KullaniciEkleAsync(Kullanici k) => await _db!.InsertAsync(k);
        public async Task KullaniciSilAsync(Kullanici k) => await _db!.DeleteAsync(k);
        public async Task<Kullanici?> KullaniciGetirAsync(string ad) => await _db!.Table<Kullanici>().Where(k => k.KullaniciAdi == ad).FirstOrDefaultAsync();
        public async Task KullaniciGuncelleAsync(Kullanici k) => await _db!.UpdateAsync(k);

        public async Task FiyatGecmisiKaydetAsync(FiyatGecmisi g) => await _db!.InsertAsync(g);
        public async Task<List<FiyatGecmisi>> UrunFiyatGecmisiAsync(int id) => await _db!.Table<FiyatGecmisi>().Where(f => f.UrunId == id).ToListAsync();
    }

    public class PersonelPerformans
    {
        public string PersonelAdi { get; set; } = "";
        public decimal ToplamCiro { get; set; }
        public int SatisSayisi { get; set; }
    }
}