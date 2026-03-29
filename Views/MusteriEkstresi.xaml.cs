using Saller_System.Services;

namespace Saller_System.Views
{
    [QueryProperty(nameof(MusteriIdStr), "MusteriId")]
    public partial class MusteriEkstresi : ContentPage
    {
        // 1. DatabaseService'i "null atanabilir değil" olarak bıraktık 
        // ama constructor'da atanacağını garanti ettik.
        private readonly DatabaseService _db;

        // 2. _musteriIdStr değişkenine varsayılan olarak boş metin verdik.
        // Böylece constructor bittiğinde null kalmamış olur.
        private string _musteriIdStr = string.Empty;

        public string MusteriIdStr
        {
            get => _musteriIdStr;
            set => _musteriIdStr = value ?? string.Empty; // Gelen değer null ise boş string ata
        }

        public MusteriEkstresi(DatabaseService db)
        {
            InitializeComponent();
            // Constructor'da atama yapıldığı için _db uyarısı kalkacak.
            _db = db;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await VerileriYukleAsync();
        }

        private async Task VerileriYukleAsync()
        {
            // string.IsNullOrWhiteSpace kontrolü eklemek daha güvenlidir.
            if (string.IsNullOrWhiteSpace(_musteriIdStr) || !int.TryParse(_musteriIdStr, out int musteriId))
                return;

            await _db.InitAsync();
            var musteri = await _db.MusteriGetirAsync(musteriId);

            if (musteri != null)
            {
                // UI güncellemelerini MainThread'e aldık
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    IsimLabel.Text = musteri.AdSoyad;

                    if (musteri.ToplamBorc > 0)
                    {
                        DurumLabel.Text = $"Müşteriden {musteri.ToplamBorc:N2} Türk Lirası alınacak.";
                        DurumLabel.TextColor = Color.FromArgb("#16A34A");
                    }
                    else if (musteri.ToplamBorc < 0)
                    {
                        DurumLabel.Text = $"Müşteriye {Math.Abs(musteri.ToplamBorc):N2} Türk Lirası borçluyuz.";
                        DurumLabel.TextColor = Color.FromArgb("#E31E24");
                    }
                    else
                    {
                        DurumLabel.Text = "Hesap Kapalı (Alacak veya Borç Yok).";
                        DurumLabel.TextColor = Colors.Gray;
                    }
                });

                var islemler = await _db.MusteriIslemleriGetirAsync(musteriId);
                // İşlem listesi boş olsa bile atama yapıyoruz.
                IslemlerListesi.ItemsSource = islemler;
            }
        }

        private async void GeriClicked(object sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}