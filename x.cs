using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Saller_System.Models;

namespace Saller_System.Services
{
    public class DataService
    {
        const string FileName = "sales_history.json";
        readonly string filePath;

        public DataService()
        {
            filePath = Path.Combine(FileSystem.AppDataDirectory, FileName);
        }

        async Task<List<SalesRecord>> LoadAllAsync()
        {
            if (!File.Exists(filePath)) return new List<SalesRecord>();
            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<List<SalesRecord>>(json) ?? new List<SalesRecord>();
        }

        async Task SaveAllAsync(List<SalesRecord> list)
        {
            // 30 günlük depolama: eski kayýtlarý sil
            var cutoff = DateTime.UtcNow.Date.AddDays(-29); // bugünden geriye 29 gün => toplam 30 gün dahil
            list = list.Where(r => r.Date >= cutoff).ToList();

            var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task AddSalesRecordAsync(SalesRecord record)
        {
            var all = await LoadAllAsync();
            all.Add(record);
            await SaveAllAsync(all);
        }

        public async Task<string> ExportSalesRecordToCsvAsync(SalesRecord record)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Tarih;ToplamTutar;MüþteriSayýsý");
            sb.AppendLine($"{record.Date:yyyy-MM-dd};{record.TotalAmount:F2};{record.CustomerCount}");
            sb.AppendLine();
            sb.AppendLine("Barkod;Ürün;Miktar;BirimFiyat;Tutar");
            foreach (var it in record.Items)
            {
                var line = $"{it.Barcode};{EscapeCsv(it.Name)};{it.Quantity};{it.Price:F2};{(it.Quantity * it.Price):F2}";
                sb.AppendLine(line);
            }

            var filename = $"günsonu_{record.Date:yyyyMMdd}.csv";
            var path = Path.Combine(FileSystem.AppDataDirectory, filename);
            await File.WriteAllTextAsync(path, sb.ToString(), Encoding.UTF8);
            return path;
        }

        string EscapeCsv(string s)
        {
            if (s == null) return "";
            if (s.Contains(";") || s.Contains("\"") || s.Contains("\n"))
                return $"\"{s.Replace("\"", "\"\"")}\"";
            return s;
        }
    }
}