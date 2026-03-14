using ClosedXML.Excel;
using Saller_System.Models;

namespace Saller_System.Services
{
    public class ExcelServisi
    {
        public string RaporOlustur(List<Satis> satislar, string baslik)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Satışlar");

            // Başlık
            ws.Cell(1, 1).Value = baslik;
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Range(1, 1, 1, 5).Merge();

            // Sütun başlıkları
            ws.Cell(2, 1).Value = "Ürün Adı";
            ws.Cell(2, 2).Value = "Adet";
            ws.Cell(2, 3).Value = "Birim Fiyat";
            ws.Cell(2, 4).Value = "Toplam";
            ws.Cell(2, 5).Value = "Tarih";
            ws.Cell(2, 6).Value = "Kasiyer";

            var baslikSatiri = ws.Range(2, 1, 2, 6);
            baslikSatiri.Style.Font.Bold = true;
            baslikSatiri.Style.Fill.BackgroundColor = XLColor.FromHtml("#2E75B6");
            baslikSatiri.Style.Font.FontColor = XLColor.White;

            // Veriler
            int satir = 3;
            foreach (var satis in satislar)
            {
                ws.Cell(satir, 1).Value = satis.UrunAd;
                ws.Cell(satir, 2).Value = satis.Adet;
                ws.Cell(satir, 3).Value = (double)satis.Fiyat;
                ws.Cell(satir, 4).Value = (double)(satis.Fiyat * satis.Adet);
                ws.Cell(satir, 5).Value = satis.Tarih.ToString("dd/MM/yyyy HH:mm");
                ws.Cell(satir, 6).Value = satis.KasiyerAd;
                satir++;
            }

            // Toplam satırı
            ws.Cell(satir, 3).Value = "TOPLAM:";
            ws.Cell(satir, 3).Style.Font.Bold = true;
            ws.Cell(satir, 4).Value = (double)satislar.Sum(s => s.Fiyat * s.Adet);
            ws.Cell(satir, 4).Style.Font.Bold = true;

            // Sütun genişliklerini ayarla
            ws.Columns().AdjustToContents();

            // Dosyayı kaydet
            string dosyaAdi = $"Rapor_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            string dosyaYolu = Path.Combine(FileSystem.AppDataDirectory, dosyaAdi);
            workbook.SaveAs(dosyaYolu);

            return dosyaYolu;
        }
    }
}