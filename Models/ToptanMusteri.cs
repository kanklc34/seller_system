using SQLite;

namespace Saller_System.Models
{
    public class ToptanMusteri
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string SirketAdi { get; set; } = string.Empty;
        public decimal ToplamBorc { get; set; } = 0;
    }
}