using Saller_System.Models;

namespace Saller_System.Services
{
    public class SepetServisi
    {
        private readonly List<SepetItem> _items = new();

        public IReadOnlyList<SepetItem> Items => _items;

        public decimal Toplam => _items.Sum(i => i.Toplam);
        public int ToplamAdet => _items.Sum(i => i.Adet);

        public void Ekle(Urun urun, int adet = 1, decimal ozelFiyat = 0)
        {
            decimal fiyat = ozelFiyat > 0 ? ozelFiyat : urun.Fiyat;

            // Gramajlı ürünler her zaman ayrı satır olarak eklenir
            if (urun.GramajliMi)
            {
                _items.Add(new SepetItem { Urun = urun, Adet = 1, OzelFiyat = fiyat });
                return;
            }

            var mevcut = _items.FirstOrDefault(i => i.Urun.Id == urun.Id && i.OzelFiyat == 0);
            if (mevcut != null)
                mevcut.Adet += adet;
            else
                _items.Add(new SepetItem { Urun = urun, Adet = adet, OzelFiyat = fiyat });
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