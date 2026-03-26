using Saller_System.Models;
using System.Linq;
using System.Collections.Generic;

namespace Saller_System.Services
{
    public class SepetServisi
    {
        private readonly List<SepetItem> _items = new();

        public IReadOnlyList<SepetItem> Items => _items;

        public decimal Toplam => _items.Sum(i => i.Toplam);

        // DEĞİŞİKLİK: Toplam miktar int yerine decimal döndürüyor
        public decimal ToplamAdet => _items.Sum(i => i.Adet);

        // DEĞİŞİKLİK: adet parametresi decimal oldu
        public void Ekle(Urun urun, decimal adet = 1, decimal ozelFiyat = 0)
        {
            // Gramajlı ürünler farklı gramajlarda çıkabileceği için her zaman ayrı satır olarak eklenir
            if (urun.GramajliMi)
            {
                _items.Add(new SepetItem { Urun = urun, Adet = adet, OzelFiyat = ozelFiyat > 0 ? ozelFiyat : urun.KgFiyati });
                return;
            }

            var mevcut = _items.FirstOrDefault(i => i.Urun.Id == urun.Id && i.OzelFiyat == 0);
            if (mevcut != null)
            {
                mevcut.Adet += adet;
            }
            else
            {
                decimal fiyat = ozelFiyat > 0 ? ozelFiyat : urun.Fiyat;
                _items.Add(new SepetItem { Urun = urun, Adet = adet, OzelFiyat = fiyat });
            }
        }

        public void Cikar(SepetItem item)
        {
            _items.Remove(item);
        }

        public void Temizle()
        {
            _items.Clear();
        }
    }
}