using Saller_System.Services;

namespace Saller_System.Views
{
    [QueryProperty(nameof(MusteriIdStr), "MusteriId")]
    public partial class MusteriEkstresi : ContentPage
    {
        private readonly DatabaseService _db;
        private string _musteriIdStr;

        public string MusteriIdStr
        {
            get => _musteriIdStr;
            set { _musteriIdStr = value; VerileriYukle(); }
        }

        public MusteriEkstresi(DatabaseService db)
        {
            InitializeComponent();
            _db = db;
        }

        private async void GeriClicked(object sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

        private async void VerileriYukle()
        {
            if (!int.TryParse(_musteriIdStr, out int musteriId)) return;

            await _db.InitAsync();
            var musteri = await _db.MusteriGetirAsync(musteriId);

            if (musteri != null)
            {
                IsimLabel.Text = musteri.AdSoyad;

                // SENİN İSTEDİĞİN NET CÜMLELER BURAYA GELDİ
                if (musteri.ToplamBorc > 0)
                {
                    DurumLabel.Text = $"Müşteriden {musteri.ToplamBorc:N2} Türk Lirası alınacak.";
                    DurumLabel.TextColor = Color.FromArgb("#16A34A"); // Yeşil
                }
                else if (musteri.ToplamBorc < 0)
                {
                    DurumLabel.Text = $"Müşteriye {Math.Abs(musteri.ToplamBorc):N2} Türk Lirası borçluyuz.";
                    DurumLabel.TextColor = Color.FromArgb("#E31E24"); // Kırmızı
                }
                else
                {
                    DurumLabel.Text = "Hesap Kapalı (Alacak veya Borç Yok).";
                    DurumLabel.TextColor = Colors.Gray;
                }

                var islemler = await _db.MusteriIslemleriGetirAsync(musteriId);
                IslemlerListesi.ItemsSource = islemler;
            }
        }
    }
}