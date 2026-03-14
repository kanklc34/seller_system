using Saller_System.Services;
using Saller_System.Models;

namespace Saller_System.Views
{
    public partial class Raporlar : ContentPage
    {
        private readonly DatabaseService _db;
        private readonly ExcelServisi _excel;

        public Raporlar(DatabaseService db, ExcelServisi excel)
        {
            InitializeComponent();
            _db = db;
            _excel = excel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await VerileriYukle();
        }

        // TELEFONUN GERİ TUŞU DESTEĞİ
        protected override bool OnBackButtonPressed()
        {
            Dispatcher.Dispatch(async () => await Shell.Current.GoToAsync("//AnaSayfa"));
            return true;
        }

        private async Task VerileriYukle()
        {
            await _db.InitAsync();
            var gunlukKar = await _db.GunlukKarAsync(DateTime.Today);
            var aylikKar = await _db.AylikKarAsync(DateTime.Today.Year, DateTime.Today.Month);
            var gunlukCiro = await _db.GunlukCiroAsync(DateTime.Today);
            var gunlukSayi = await _db.GunlukSatisSayisiAsync(DateTime.Today);
            var aylikCiro = await _db.AylikCiroAsync(DateTime.Today.Year, DateTime.Today.Month);

            GunlukKarLabel.Text = $"₺{gunlukKar:N2}";
            AylikKarLabel.Text = $"₺{aylikKar:N2}";
            GunlukCiroLabel.Text = $"₺{gunlukCiro:N2}";
            GunlukSayiLabel.Text = $"{gunlukSayi} Adet";
            AylikCiroLabel.Text = $"₺{aylikCiro:N2}";
            AyLabel.Text = DateTime.Today.ToString("MMMM yyyy").ToUpper();
        }

        private async void SatisGecmisiClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//SatisGecmisiSayfa");

        private async void ExcelAktarClicked(object sender, EventArgs e)
        {
            string secim = await DisplayActionSheet(
                "Rapor Türü Seçin", "İptal", null,
                "📅 Bugünkü Satışlar",
                "📆 Bu Ayın Tüm Satışları");

            if (secim == null || secim == "İptal") return;

            await _db.InitAsync();
            List<Satis> satislar;
            string baslik;

            if (secim == "📅 Bugünkü Satışlar")
            {
                satislar = await _db.GunlukSatislerAsync(DateTime.Today);
                baslik = $"Gunluk_Rapor_{DateTime.Today:dd_MM_yyyy}";
            }
            else
            {
                satislar = await _db.AylikSatislerAsync(DateTime.Today.Year, DateTime.Today.Month);
                baslik = $"Aylik_Rapor_{DateTime.Today:MMMM_yyyy}";
            }

            if (satislar.Count == 0)
            {
                await DisplayAlert("Uyarı", "Aktarılacak veri bulunamadı.", "Tamam");
                return;
            }

            try
            {
                // Dosyayı oluştur
                string dosyaYolu = _excel.RaporOlustur(satislar, baslik);

                // HATAYI ÇÖZEN KISIM: DOSYAYI PAYLAŞ
                // Bu sayede "dosya bulunamadı" veya "yol hatası" olmadan WhatsApp/Mail ile gönderebilirsin.
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Satış Raporu",
                    File = new ShareFile(dosyaYolu)
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", "Dosya paylaşılırken bir sorun oluştu: " + ex.Message, "Tamam");
            }
        }

        private async void GeriClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//AnaSayfa");
    }
}