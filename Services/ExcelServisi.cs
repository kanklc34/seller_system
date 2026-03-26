using ClosedXML.Excel;
using Saller_System.Models;

namespace Saller_System.Services
{
    public class ExcelServisi
    {
        private readonly DatabaseService _db;
        public ExcelServisi(DatabaseService db) { _db = db; }

        public async Task<string> RaporOlustur(List<Satis> satislar, string baslik, DateTime tarih)
        {
            await _db.InitAsync();
            using var workbook = new XLWorkbook();

            // 1. SEKME: PERAKENDE SATIŞLAR
            var ws = workbook.Worksheets.Add("Perakende Satışlar");
            ws.Cell(1, 1).Value = "ÖZ BİGA ET - GÜNLÜK SATIŞ RAPORU";
            ws.Cell(2, 1).Value = "Tarih: " + tarih.ToString("dd.MM.yyyy");

            string[] headers = { "Ürün Adı", "Adet/Kg", "Fiyat", "Toplam", "Saat", "Kasiyer" };
            for (int i = 0; i < headers.Length; i++) ws.Cell(4, i + 1).Value = headers[i];
            ws.Range(4, 1, 4, 6).Style.Font.Bold = true;

            int row = 5;
            var perakende = satislar.Where(s => s.SatisTipi != "TOPTAN" && s.SatisTipi != "TAHSILAT").ToList();
            foreach (var s in perakende)
            {
                ws.Cell(row, 1).Value = s.UrunAd;
                ws.Cell(row, 2).Value = s.Adet;
                ws.Cell(row, 3).Value = (double)s.Fiyat;
                ws.Cell(row, 4).Value = (double)(s.Fiyat * s.Adet);
                ws.Cell(row, 5).Value = s.Tarih.ToString("HH:mm");
                ws.Cell(row, 6).Value = s.KasiyerAd;
                row++;
            }

            // 2. SEKME: TOPTAN SATIŞLAR (FİRMA ADI EKLENDİ)
            var wsToptan = workbook.Worksheets.Add("Toptan Satışlar");
            wsToptan.Cell(1, 1).Value = "TOPTAN SATIŞ DÖKÜMÜ";
            string[] tHeaders = { "Ürün Adı", "Miktar (Kg)", "Toplam Tutar", "Alıcı Firma / Kasiyer" };
            for (int i = 0; i < tHeaders.Length; i++) wsToptan.Cell(3, i + 1).Value = tHeaders[i];

            var toptanSatislar = satislar.Where(s => s.SatisTipi == "TOPTAN").ToList();
            int tRow = 4;
            foreach (var s in toptanSatislar)
            {
                wsToptan.Cell(tRow, 1).Value = s.UrunAd;
                wsToptan.Cell(tRow, 2).Value = s.Adet;
                wsToptan.Cell(tRow, 3).Value = (double)s.Fiyat;
                wsToptan.Cell(tRow, 4).Value = s.KasiyerAd; // Firma adı burada yazacak
                tRow++;
            }

            // 3. SEKME: GİDERLER
            var wsGider = workbook.Worksheets.Add("Dükkan Giderleri");
            var giderler = await _db.GunlukGiderlerAsync(tarih);
            wsGider.Cell(1, 1).Value = "Gider Başlığı"; wsGider.Cell(1, 2).Value = "Tutar (TL)";
            int gRow = 2;
            foreach (var g in giderler) { wsGider.Cell(gRow, 1).Value = g.Baslik; wsGider.Cell(gRow, 2).Value = (double)g.Tutar; gRow++; }

            // 4. SEKME: NET KAR ÖZETİ
            var wsOzet = workbook.Worksheets.Add("Net Kar Özeti");
            decimal toplamKar = satislar.Sum(s => s.Kar);
            decimal toplamGider = giderler.Sum(g => g.Tutar);
            decimal netKazanc = toplamKar - toplamGider;

            wsOzet.Cell(1, 1).Value = "Satışlardan Elde Edilen Toplam Brüt Kar:"; wsOzet.Cell(1, 2).Value = (double)toplamKar;
            wsOzet.Cell(2, 1).Value = "Toplam Dükkan Giderleri (Masraf):"; wsOzet.Cell(2, 2).Value = (double)toplamGider;
            wsOzet.Cell(4, 1).Value = "NET KAZANÇ (CEBE KALAN):"; wsOzet.Cell(4, 2).Value = (double)netKazanc;
            wsOzet.Range(4, 1, 4, 2).Style.Font.Bold = true;

            ws.Columns().AdjustToContents();
            wsToptan.Columns().AdjustToContents();
            wsGider.Columns().AdjustToContents();
            wsOzet.Columns().AdjustToContents();

            string fileName = $"OzBigaEt_Rapor_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            workbook.SaveAs(filePath);
            return filePath;
        }
    }
}