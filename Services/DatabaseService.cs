using SQLite;
using Saller_System.Models;

namespace Saller_System.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _db;

        // ----------------------------------------------------------------
        // Varsayılan sayfa boyutu (pagination)
        // ----------------------------------------------------------------
        public const int SayfaBoyutu = 50;

        // ================================================================
        // BAŞLATMA
        // ================================================================
        public async Task InitAsync()
        {
            if (_db != null) return;

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "saller.db");
            _db = new SQLiteAsyncConnection(dbPath);

            // Tabloları oluştur (ESKİLER)
            await _db.CreateTableAsync<Urun>();
            await _db.CreateTableAsync<Satis>();
            await _db.CreateTableAsync<FiyatGecmisi>();
            await _db.CreateTableAsync<Kullanici>();
            await _db.CreateTableAsync<ArsivedSatis>();   // Arşiv tablosu

            // YENİ EKLENEN TABLOLAR (Veresiye)
            await _db.CreateTableAsync<Musteri>();
            await _db.CreateTableAsync<VeresiyeIslem>();

            // DÜZELTME: ToptanSatis bir arayüz olduğu için buradan SİLİNDİ.

            await VarsayilanKullanicilariOlusturAsync();

            // Uygulama her açıldığında 1 yıldan eski önemsiz verileri temizle
            await BirYillikEskiVerileriTemizleAsync();
        }

        // Varsayılan kullanıcıları oluştur (ilk kurulumda)
        private async Task VarsayilanKullanicilariOlusturAsync()
        {
            var admin = await _db!.Table<Kullanici>()
                .Where(k => k.KullaniciAdi == "admin")
                .FirstOrDefaultAsync();

            if (admin != null) return;  // Zaten kurulu

            // DEĞİŞİKLİK: Şifreler artık düz metin olarak kaydediliyor
            await _db.InsertAsync(new Kullanici
            {
                KullaniciAdi = "admin",
                Sifre = "1234",
                Rol = "Patron"
            });
            await _db.InsertAsync(new Kullanici
            {
                KullaniciAdi = "yonetici",
                Sifre = "1234",
                Rol = "Müdür"
            });
            await _db.InsertAsync(new Kullanici
            {
                KullaniciAdi = "kasiyer",
                Sifre = "1234",
                Rol = "Kasiyer"
            });
        }

        // ================================================================
        // GİRİŞ KONTROLÜ — Şifreleme (Hash) kaldırıldı, düz metin kontrolü
        // ================================================================
        public async Task<Kullanici?> GirisKontrolAsync(string ad, string sifre)
        {
            var kullanici = await _db!.Table<Kullanici>()
                .Where(k => k.KullaniciAdi == ad)
                .FirstOrDefaultAsync();

            if (kullanici == null) return null;

            // DEĞİŞİKLİK: Şifreler düz metin olarak karşılaştırılıyor
            if (sifre != kullanici.Sifre)
                return null;

            return kullanici;
        }

        // ================================================================
        // ÜRÜN İŞLEMLERİ
        // ================================================================
        public async Task<List<Urun>> TumUrunleriGetirAsync() =>
            await _db!.Table<Urun>().ToListAsync();

        // Sayfalı ürün listesi
        public async Task<List<Urun>> UrunleriSayfaliGetirAsync(int sayfa = 0, int boyut = SayfaBoyutu) =>
            await _db!.Table<Urun>().Skip(sayfa * boyut).Take(boyut).ToListAsync();

        // Arama destekli sayfalı liste
        public async Task<List<Urun>> UrunAraAsync(string aramaMetni, int sayfa = 0, int boyut = SayfaBoyutu)
        {
            var metin = aramaMetni.ToLower();
            return await _db!.Table<Urun>()
                .Where(u => u.Ad.ToLower().Contains(metin) || u.Barkod.Contains(aramaMetni))
                .Skip(sayfa * boyut)
                .Take(boyut)
                .ToListAsync();
        }

        public async Task<int> ToplamUrunSayisiAsync() =>
            await _db!.Table<Urun>().CountAsync();

        public async Task<Urun?> BarkodIleGetirAsync(string barkod) =>
            await _db!.Table<Urun>().Where(u => u.Barkod == barkod).FirstOrDefaultAsync();

        public async Task UrunEkleAsync(Urun urun) =>
            await _db!.InsertAsync(urun);

        public async Task UrunSilAsync(Urun urun) =>
            await _db!.DeleteAsync(urun);

        public async Task UrunGuncelleAsync(Urun yeniUrun, Urun eskiUrun)
        {
            // Fiyat değiştiyse geçmişe kaydet + ürünü güncelle — TRANSACTION ile
            bool fiyatDegisti = eskiUrun.Fiyat != yeniUrun.Fiyat
                             || eskiUrun.KgFiyati != yeniUrun.KgFiyati;

            await _db!.RunInTransactionAsync(db =>
            {
                if (fiyatDegisti)
                {
                    db.Insert(new FiyatGecmisi
                    {
                        UrunId = yeniUrun.Id,
                        UrunAd = yeniUrun.Ad,
                        EskiFiyat = eskiUrun.GramajliMi ? eskiUrun.KgFiyati : eskiUrun.Fiyat,
                        YeniFiyat = yeniUrun.GramajliMi ? yeniUrun.KgFiyati : yeniUrun.Fiyat,
                        Tarih = DateTime.Now,
                        DegistirenKullanici = OturumServisi.AktifKullanici?.KullaniciAdi ?? "Bilinmiyor"
                    });
                }
                db.Update(yeniUrun);
            });
        }

        // ================================================================
        // SATIŞ İŞLEMLERİ — TRANSACTION
        // ================================================================
        public async Task SatisleriTopluKaydetAsync(IEnumerable<Satis> satisler)
        {
            await _db!.RunInTransactionAsync(db =>
            {
                foreach (var satis in satisler)
                    db.Insert(satis);
            });
        }

        public async Task SatisKaydetAsync(Satis satis) =>
            await _db!.InsertAsync(satis);

        public async Task<List<Satis>> TumSatisleriGetirAsync(int sayfa = 0, int boyut = SayfaBoyutu) =>
            await _db!.Table<Satis>()
                .OrderByDescending(s => s.Tarih)
                .Skip(sayfa * boyut)
                .Take(boyut)
                .ToListAsync();

        public async Task<int> ToplamSatisSayisiAsync() =>
            await _db!.Table<Satis>().CountAsync();

        public async Task<List<Satis>> GunlukSatislerAsync(DateTime tarih)
        {
            var bas = tarih.Date;
            var bit = bas.AddDays(1);
            return await _db!.Table<Satis>()
                .Where(s => s.Tarih >= bas && s.Tarih < bit)
                .ToListAsync();
        }

        public async Task<decimal> GunlukCiroAsync(DateTime tarih) => (await GunlukSatislerAsync(tarih)).Sum(s => s.Fiyat);
        public async Task<decimal> GunlukKarAsync(DateTime tarih) => (await GunlukSatislerAsync(tarih)).Sum(s => s.Kar);
        public async Task<decimal> GunlukSatisSayisiAsync(DateTime tarih) => (await GunlukSatislerAsync(tarih)).Sum(s => s.Adet);

        public async Task<List<Satis>> AylikSatislerAsync(int yil, int ay)
        {
            var bas = new DateTime(yil, ay, 1);
            var bit = bas.AddMonths(1);
            return await _db!.Table<Satis>()
                .Where(s => s.Tarih >= bas && s.Tarih < bit)
                .ToListAsync();
        }

        public async Task<decimal> AylikCiroAsync(int yil, int ay) =>
            (await AylikSatislerAsync(yil, ay)).Sum(s => s.Fiyat);
        public async Task<decimal> AylikKarAsync(int yil, int ay) =>
            (await AylikSatislerAsync(yil, ay)).Sum(s => s.Kar);

        // ================================================================
        // ARŞİVLEME VE 1 YILLIK OTOMATİK TEMİZLİK
        // ================================================================
        public async Task EskiSatisleriArsivleAsync()
        {
            var sinir = DateTime.Now.AddDays(-30);
            var eskiler = await _db!.Table<Satis>()
                .Where(s => s.Tarih < sinir)
                .ToListAsync();

            if (!eskiler.Any()) return;

            await _db.RunInTransactionAsync(db =>
            {
                foreach (var satis in eskiler)
                {
                    db.Insert(new ArsivedSatis(satis));
                    db.Delete(satis);
                }
            });
        }

        // YENİ EKLENEN: 1 Yıldan eski ciro ve fiş verilerini siler (Müşteri ve Ürünler KALIR)
        public async Task BirYillikEskiVerileriTemizleAsync()
        {
            var birYilOnce = DateTime.Now.AddYears(-1);

            // ArsivedSatis tablosundaki 1 yıldan eski verileri sil
            var silinecekSatislar = await _db!.Table<ArsivedSatis>()
                .Where(s => s.Tarih < birYilOnce)
                .ToListAsync();

            if (silinecekSatislar.Any())
            {
                await _db.RunInTransactionAsync(db =>
                {
                    foreach (var satis in silinecekSatislar)
                        db.Delete(satis);
                });
            }
        }

        public async Task<List<ArsivedSatis>> ArsivedSatisleriGetirAsync(int sayfa = 0, int boyut = SayfaBoyutu) =>
            await _db!.Table<ArsivedSatis>()
                .OrderByDescending(s => s.Tarih)
                .Skip(sayfa * boyut)
                .Take(boyut)
                .ToListAsync();

        // ================================================================
        // PERSONEL PERFORMANS RAPORU
        // ================================================================
        public async Task<List<PersonelPerformans>> PersonelPerformansRaporuGetirAsync(DateTime tarih)
        {
            var satislar = await GunlukSatislerAsync(tarih);
            return satislar
                .GroupBy(s => s.KasiyerAd)
                .Select(g => new PersonelPerformans
                {
                    PersonelAdi = g.Key ?? "Bilinmeyen",
                    ToplamCiro = g.Sum(s => s.Fiyat),
                    SatisSayisi = g.Count()
                })
                .OrderByDescending(x => x.ToplamCiro)
                .ToList();
        }

        // ================================================================
        // KULLANICI İŞLEMLERİ — Şifreleme (Hash) kaldırıldı, düz metin kaydediliyor
        // ================================================================
        public async Task<List<Kullanici>> TumKullanicilariGetirAsync() =>
            await _db!.Table<Kullanici>().ToListAsync();

        public async Task KullaniciEkleAsync(Kullanici k)
        {
            // DEĞİŞİKLİK: Şifreleme kaldırıldı
            await _db!.InsertAsync(k);
        }

        public async Task KullaniciSilAsync(Kullanici k) =>
            await _db!.DeleteAsync(k);

        public async Task<Kullanici?> KullaniciGetirAsync(string ad) =>
            await _db!.Table<Kullanici>().Where(k => k.KullaniciAdi == ad).FirstOrDefaultAsync();

        public async Task KullaniciGuncelleAsync(Kullanici k)
        {
            // DEĞİŞİKLİK: Şifreleme kaldırıldı
            await _db!.UpdateAsync(k);
        }

        // ================================================================
        // FİYAT GEÇMİŞİ
        // ================================================================
        public async Task FiyatGecmisiKaydetAsync(FiyatGecmisi g) =>
            await _db!.InsertAsync(g);

        public async Task<List<FiyatGecmisi>> UrunFiyatGecmisiAsync(int id) =>
            await _db!.Table<FiyatGecmisi>().Where(f => f.UrunId == id).ToListAsync();


        // ================================================================
        // YENİ EKLENEN: MÜŞTERİ VE VERESİYE İŞLEMLERİ
        // ================================================================
        public async Task<List<Musteri>> TumMusterileriGetirAsync() =>
            await _db!.Table<Musteri>().ToListAsync();

        public async Task MusteriEkleAsync(Musteri m) =>
            await _db!.InsertAsync(m);

        public async Task VeresiyeIslemKaydetAsync(VeresiyeIslem islem)
        {
            await _db!.RunInTransactionAsync(db =>
            {
                db.Insert(islem);

                // Müşterinin toplam borcunu güncelle
                var musteri = db.Find<Musteri>(islem.MusteriId);
                if (musteri != null)
                {
                    musteri.ToplamBorc += islem.Tutar;
                    db.Update(musteri);
                }
            });
        }

        // DÜZELTME: ToptanSatisKaydetAsync silindi, çünkü satışları artık Satis tablosuna SatisTipi="TOPTAN" diyerek kaydediyoruz.
    }

    // ====================================================================
    // YARDIMCI SINIFLAR
    // ====================================================================
    public class PersonelPerformans
    {
        public string PersonelAdi { get; set; } = "";
        public decimal ToplamCiro { get; set; }
        public int SatisSayisi { get; set; }
    }
}