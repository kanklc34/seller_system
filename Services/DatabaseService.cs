using SQLite;
using Saller_System.Models;

namespace Saller_System.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _db;
        public const int SayfaBoyutu = 50;

        public async Task InitAsync()
        {
            if (_db != null) return;
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "saller.db");
            _db = new SQLiteAsyncConnection(dbPath);

            // Tabloları oluşturuyoruz
            await _db.CreateTableAsync<Urun>();
            await _db.CreateTableAsync<Satis>();
            await _db.CreateTableAsync<FiyatGecmisi>();
            await _db.CreateTableAsync<Kullanici>();
            await _db.CreateTableAsync<ArsivedSatis>();
            await _db.CreateTableAsync<Musteri>();
            await _db.CreateTableAsync<VeresiyeIslem>();
            await _db.CreateTableAsync<ToptanMusteri>();
            await _db.CreateTableAsync<Gider>();

            await VarsayilanKullanicilariOlusturAsync();
            await BirYillikEskiVerileriTemizleAsync();
        }

        private async Task VarsayilanKullanicilariOlusturAsync()
        {
            var admin = await _db!.Table<Kullanici>().Where(k => k.KullaniciAdi == "admin").FirstOrDefaultAsync();
            if (admin != null) return;
            await _db.InsertAsync(new Kullanici { KullaniciAdi = "admin", Sifre = "1234", Rol = "Patron" });
            await _db.InsertAsync(new Kullanici { KullaniciAdi = "yonetici", Sifre = "1234", Rol = "Müdür" });
            await _db.InsertAsync(new Kullanici { KullaniciAdi = "kasiyer", Sifre = "1234", Rol = "Kasiyer" });
        }

        public async Task<Kullanici?> GirisKontrolAsync(string ad, string sifre)
        {
            var kullanici = await _db!.Table<Kullanici>().Where(k => k.KullaniciAdi == ad).FirstOrDefaultAsync();
            return (kullanici != null && sifre == kullanici.Sifre) ? kullanici : null;
        }

        // --- KULLANICI İŞLEMLERİ (KullaniciYonetimi.xaml için) ---
        public async Task<List<Kullanici>> TumKullanicilariGetirAsync() => await _db!.Table<Kullanici>().ToListAsync();
        public async Task KullaniciEkleAsync(Kullanici k) => await _db!.InsertAsync(k);
        public async Task KullaniciSilAsync(Kullanici k) => await _db!.DeleteAsync(k);

        // --- ÜRÜN İŞLEMLERİ ---
        public async Task<List<Urun>> TumUrunleriGetirAsync() => await _db!.Table<Urun>().ToListAsync();
        public async Task<Urun?> BarkodIleGetirAsync(string barkod) => await _db!.Table<Urun>().Where(u => u.Barkod == barkod).FirstOrDefaultAsync();
        public async Task<List<Urun>> UrunAraAsync(string aramaMetni, int sayfa = 0, int boyut = SayfaBoyutu)
        {
            var metin = aramaMetni.ToLower();
            return await _db!.Table<Urun>().Where(u => u.Ad.ToLower().Contains(metin) || u.Barkod.Contains(aramaMetni)).Skip(sayfa * boyut).Take(boyut).ToListAsync();
        }
        public async Task UrunEkleAsync(Urun urun) => await _db!.InsertAsync(urun);
        public async Task UrunSilAsync(Urun urun) => await _db!.DeleteAsync(urun);
        public async Task UrunGuncelleAsync(Urun yeniUrun, Urun eskiUrun)
        {
            bool fiyatDegisti = eskiUrun.Fiyat != yeniUrun.Fiyat || eskiUrun.KgFiyati != yeniUrun.KgFiyati;
            await _db!.RunInTransactionAsync(db =>
            {
                if (fiyatDegisti)
                {
                    db.Insert(new FiyatGecmisi { UrunId = yeniUrun.Id, UrunAd = yeniUrun.Ad, EskiFiyat = eskiUrun.GramajliMi ? eskiUrun.KgFiyati : eskiUrun.Fiyat, YeniFiyat = yeniUrun.GramajliMi ? yeniUrun.KgFiyati : yeniUrun.Fiyat, Tarih = DateTime.Now, DegistirenKullanici = "Sistem" });
                }
                db.Update(yeniUrun);
            });
        }

        // FiyatGecmisiSayfa.xaml için gerekli metod
        public async Task<List<FiyatGecmisi>> UrunFiyatGecmisiAsync(int urunId) => await _db!.Table<FiyatGecmisi>().Where(f => f.UrunId == urunId).OrderByDescending(f => f.Tarih).ToListAsync();

        // --- SATIŞ VE ARŞİVLEME ---
        public async Task SatisKaydetAsync(Satis satis) => await _db!.InsertAsync(satis);
        public async Task SatisleriTopluKaydetAsync(IEnumerable<Satis> satisler) { await _db!.RunInTransactionAsync(db => { foreach (var s in satisler) db.Insert(s); }); }

        // App.xaml.cs'deki arşivleme hatasını çözen metod
        public async Task EskiSatisleriArsivleAsync()
        {
            var sinirTarih = DateTime.Now.AddDays(-30); // 30 günden eskiyi arşivle
            var eskiSatislar = await _db!.Table<Satis>().Where(s => s.Tarih < sinirTarih).ToListAsync();
            if (eskiSatislar.Any())
            {
                await _db.RunInTransactionAsync(db =>
                {
                    foreach (var s in eskiSatislar)
                    {
                        db.Insert(new ArsivedSatis { Barkod = s.Barkod, Ad = s.Ad, Fiyat = s.Fiyat, Adet = s.Adet, Tarih = s.Tarih, SatisTipi = s.SatisTipi });
                        db.Delete(s);
                    }
                });
            }
        }

        public async Task<List<Satis>> TumSatisleriGetirAsync() => await _db!.Table<Satis>().OrderByDescending(s => s.Tarih).ToListAsync();
        public async Task<List<Satis>> GunlukSatislerAsync(DateTime tarih) { var b = tarih.Date; var bit = b.AddDays(1); return await _db!.Table<Satis>().Where(s => s.Tarih >= b && s.Tarih < bit).ToListAsync(); }

        public async Task<decimal> GunlukGercekCiroAsync(DateTime tarih)
        {
            var satislar = await GunlukSatislerAsync(tarih);
            return satislar.Where(s => s.SatisTipi != "TAHSILAT" && s.SatisTipi != "TOPTAN").Sum(s => s.Fiyat);
        }
        public async Task<decimal> GunlukKarAsync(DateTime tarih) => (await GunlukSatislerAsync(tarih)).Sum(s => s.Kar);
        public async Task<decimal> GunlukSatisSayisiAsync(DateTime tarih) => (await GunlukSatislerAsync(tarih)).Sum(s => s.Adet);
        public async Task<decimal> AylikCiroAsync(int yil, int ay)
        {
            var bas = new DateTime(yil, ay, 1); var bit = bas.AddMonths(1);
            var satislar = await _db!.Table<Satis>().Where(s => s.Tarih >= bas && s.Tarih < bit).ToListAsync();
            return satislar.Where(s => s.SatisTipi != "TAHSILAT" && s.SatisTipi != "TOPTAN").Sum(s => s.Fiyat);
        }
        public async Task<decimal> AylikKarAsync(int yil, int ay)
        {
            var bas = new DateTime(yil, ay, 1); var bit = bas.AddMonths(1);
            var satislar = await _db!.Table<Satis>().Where(s => s.Tarih >= bas && s.Tarih < bit).ToListAsync();
            return satislar.Sum(s => s.Kar);
        }

        // --- GİDER İŞLEMLERİ ---
        public async Task GiderEkleAsync(Gider g) => await _db!.InsertAsync(g);
        public async Task<List<Gider>> GunlukGiderlerAsync(DateTime tarih) { var b = tarih.Date; var bit = b.AddDays(1); return await _db!.Table<Gider>().Where(g => g.Tarih >= b && g.Tarih < bit).ToListAsync(); }
        public async Task<decimal> GunlukGiderToplamiAsync(DateTime tarih) => (await GunlukGiderlerAsync(tarih)).Sum(g => g.Tutar);

        // --- VERESİYE VE TOPTAN MÜŞTERİ ---
        public async Task<List<Musteri>> TumMusterileriGetirAsync() => await _db!.Table<Musteri>().ToListAsync();
        public async Task MusteriEkleAsync(Musteri m) => await _db!.InsertAsync(m);
        public async Task VeresiyeIslemKaydetAsync(VeresiyeIslem islem)
        {
            await _db!.RunInTransactionAsync(db =>
            {
                db.Insert(islem);
                var musteri = db.Find<Musteri>(islem.MusteriId);
                if (musteri != null) { musteri.ToplamBorc += islem.Tutar; db.Update(musteri); }
            });
        }
        public async Task<Musteri?> MusteriGetirAsync(int id) => await _db!.Table<Musteri>().Where(m => m.Id == id).FirstOrDefaultAsync();
        public async Task<List<VeresiyeIslem>> MusteriIslemleriGetirAsync(int musteriId) => await _db!.Table<VeresiyeIslem>().Where(i => i.MusteriId == musteriId).OrderByDescending(i => i.Tarih).ToListAsync();
        public async Task<List<ToptanMusteri>> TumToptanMusterileriGetirAsync() => await _db!.Table<ToptanMusteri>().ToListAsync();
        public async Task ToptanMusteriEkleAsync(ToptanMusteri m) => await _db!.InsertAsync(m);
        public async Task ToptanMusteriBorcEkleAsync(int musteriId, decimal tutar)
        {
            var musteri = await _db!.FindAsync<ToptanMusteri>(musteriId);
            if (musteri != null) { musteri.ToplamBorc += tutar; await _db!.UpdateAsync(musteri); }
        }

        // --- DİĞER ---
        public async Task<List<VeresiyeIslem>> GunlukVeresiyeDetaylariAsync(DateTime tarih) { var b = tarih.Date; var bit = b.AddDays(1); return await _db!.Table<VeresiyeIslem>().Where(i => i.Tarih >= b && i.Tarih < bit).OrderByDescending(i => i.Tarih).ToListAsync(); }
        public async Task<decimal> GunlukTahsilatToplamiAsync(DateTime tarih) { var satislar = await GunlukSatislerAsync(tarih); return satislar.Where(s => s.SatisTipi == "TAHSILAT").Sum(s => s.Fiyat); }
        public async Task<List<PersonelPerformans>> PersonelPerformansRaporuGetirAsync(DateTime tarih) { var satislar = await GunlukSatislerAsync(tarih); return satislar.GroupBy(s => s.KasiyerAd).Select(g => new PersonelPerformans { PersonelAdi = g.Key ?? "Bilinmeyen", ToplamCiro = g.Sum(s => s.Fiyat), SatisSayisi = g.Count() }).OrderByDescending(x => x.ToplamCiro).ToList(); }
        public async Task BirYillikEskiVerileriTemizleAsync() { var b = DateTime.Now.AddYears(-1); var sil = await _db!.Table<ArsivedSatis>().Where(s => s.Tarih < b).ToListAsync(); if (sil.Any()) await _db.RunInTransactionAsync(db => { foreach (var s in sil) db.Delete(s); }); }
    }

    public class PersonelPerformans
    {
        public string PersonelAdi { get; set; } = "";
        public decimal ToplamCiro { get; set; }
        public int SatisSayisi { get; set; }
    }
}