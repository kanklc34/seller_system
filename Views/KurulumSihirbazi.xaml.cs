using Saller_System.Services;
using Saller_System.Models;

namespace Saller_System.Views
{
    public partial class KurulumSihirbazi : ContentPage
    {
        private readonly AyarlarServisi _ayarlar;
        private readonly DatabaseService _db;
        private string? _algılananPrefix;
        private string _secilenGorselYolu = "logo.jpg";
        public KurulumSihirbazi(AyarlarServisi ayarlar, DatabaseService db)
        {
            InitializeComponent();
            _ayarlar = ayarlar;
            _db = db;
        }

        // ----------------------------------------------------------------
        // Adım 1 — Mağaza bilgileri
        // ----------------------------------------------------------------
        private async void GorselSecClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Mağaza Görseli Seçin",
                    FileTypes = FilePickerFileType.Images
                });

                if (result != null)
                {
                    _secilenGorselYolu = result.FullPath;
                    SecilenGorselOnizleme.Source = ImageSource.FromFile(_secilenGorselYolu);

                    // Üstteki büyük görseli de anında güncellemek istersen:
                    KurulumGorseli.Source = ImageSource.FromFile(_secilenGorselYolu);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", "Görsel seçilemedi: " + ex.Message, "Tamam");
            }
        }
        private async void Adim1DevamClicked(object sender, EventArgs e)
        {
            string magazaAdi = MagazaAdiEntry.Text?.Trim() ?? "";
            string telefon = new string((TelefonEntry.Text ?? "").Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(magazaAdi))
            {
                await DisplayAlert("Hata", "Mağaza adı zorunludur!", "Tamam");
                return;
            }

            if (!string.IsNullOrEmpty(telefon) && telefon.Length != 11)
            {
                TelefonHataLabel.Text = "❌ Türkiye telefon numarası 11 hane olmalıdır.";
                TelefonHataLabel.IsVisible = true;
                return;
            }

            TelefonHataLabel.IsVisible = false;
            await _ayarlar.SetAsync("MagazaAdi", magazaAdi);
            await _ayarlar.SetAsync("Telefon", string.IsNullOrEmpty(telefon) ? "" :
                telefon[..4] + " " + telefon[4..7] + " " + telefon[7..9] + " " + telefon[9..]);
            await _ayarlar.SetAsync("DukkanArkaPlan", _secilenGorselYolu);
            Adim1Panel.IsVisible = false;
            Adim2Panel.IsVisible = true;
            Adim1Dot.Fill = new SolidColorBrush(Color.FromArgb("#166534"));
            Adim2Dot.Fill = new SolidColorBrush(Color.FromArgb("#E31E24"));
        }

        private void TelefonUnfocused(object sender, FocusEventArgs e)
        {
            string temiz = new string((TelefonEntry.Text ?? "").Where(char.IsDigit).ToArray());
            if (temiz.Length == 0) return;
            if (temiz.Length > 11) temiz = temiz[..11];

            string formatted = "";
            for (int i = 0; i < temiz.Length; i++)
            {
                if (i == 4 || i == 7 || i == 9) formatted += " ";
                formatted += temiz[i];
            }
            TelefonEntry.Text = formatted;
        }

        // ----------------------------------------------------------------
        // Adım 2 — Terazi
        // ----------------------------------------------------------------
        private void FormatAlgilaClicked(object sender, EventArgs e)
        {
            string barkod = TeraziBarkodEntry.Text?.Trim() ?? "";
            if (barkod.Length != 13)
            {
                FormatSonucLabel.Text = "❌ Geçersiz barkod (13 hane olmalı)";
                FormatSonucLabel.TextColor = Colors.Red;
                FormatSonucLabel.IsVisible = true;
                return;
            }

            _algılananPrefix = TartiServisi.PrefixAlgila(barkod);
            if (_algılananPrefix != null)
            {
                FormatSonucLabel.Text = "✅ Terazi formatı algılandı!";
                FormatSonucLabel.TextColor = Colors.Green;
                FormatSonucLabel.IsVisible = true;
                Adim2DevamBorder.Opacity = 1.0;
            }
            else
            {
                FormatSonucLabel.Text = "❌ Format algılanamadı.";
                FormatSonucLabel.TextColor = Colors.Red;
                FormatSonucLabel.IsVisible = true;
            }
        }

        private async void Adim2DevamClicked(object sender, EventArgs e)
        {
            if (_algılananPrefix != null)
                await _ayarlar.SetAsync("TaraziPrefix", _algılananPrefix);

            Adim2Panel.IsVisible = false;
            Adim3Panel.IsVisible = true;
            Adim2Dot.Fill = new SolidColorBrush(Color.FromArgb("#166534"));
            Adim3Dot.Fill = new SolidColorBrush(Color.FromArgb("#E31E24"));
        }

        private void TeraziAtlaClicked(object sender, EventArgs e)
        {
            Adim2Panel.IsVisible = false;
            Adim3Panel.IsVisible = true;
            Adim2Dot.Fill = new SolidColorBrush(Color.FromArgb("#166534"));
            Adim3Dot.Fill = new SolidColorBrush(Color.FromArgb("#E31E24"));
        }

        // ----------------------------------------------------------------
        // Adım 3 — İlk kullanıcı oluştur (serbest kullanıcı adı + rol)
        // ----------------------------------------------------------------
        private async void Adim3DevamClicked(object sender, EventArgs e)
        {
            string kullaniciAdi = KullaniciAdiEntry.Text?.Trim() ?? "";
            string sifre = YeniSifreEntry.Text ?? "";
            string sifreTekrar = YeniSifreTekrarEntry.Text ?? "";

            if (string.IsNullOrEmpty(kullaniciAdi))
            {
                SifreHataLabel.Text = "❌ Kullanıcı adı boş olamaz!";
                SifreHataLabel.IsVisible = true;
                return;
            }

            if (RolPicker.SelectedIndex == -1)
            {
                SifreHataLabel.Text = "❌ Lütfen bir rol seçin!";
                SifreHataLabel.IsVisible = true;
                return;
            }

            if (sifre.Length < 4)
            {
                SifreHataLabel.Text = "❌ Şifre en az 4 karakter olmalıdır!";
                SifreHataLabel.IsVisible = true;
                return;
            }

            if (sifre != sifreTekrar)
            {
                SifreHataLabel.Text = "❌ Şifreler eşleşmiyor!";
                SifreHataLabel.IsVisible = true;
                return;
            }

            SifreHataLabel.IsVisible = false;

            await _db.InitAsync();

            // Mevcut varsayılan kullanıcıları temizle, yeni kullanıcıyı ekle
            var mevcutlar = await _db.TumKullanicilariGetirAsync();
            foreach (var k in mevcutlar)
                await _db.KullaniciSilAsync(k);

            var yeniKullanici = new Kullanici
            {
                KullaniciAdi = kullaniciAdi,
                Sifre = GuvenlikServisi.Hashle(sifre),
                Rol = RolPicker.SelectedItem.ToString()!
            };
            await _db.KullaniciEkleAsync(yeniKullanici);

            var magazaAdi = await _ayarlar.GetAsync("MagazaAdi", "");
            KurulumOzetLabel.Text =
                $"Mağaza: {magazaAdi}\n" +
                $"Kullanıcı: {kullaniciAdi} ({RolPicker.SelectedItem})";

            Adim3Panel.IsVisible = false;
            Adim4Panel.IsVisible = true;
            Adim3Dot.Fill = new SolidColorBrush(Color.FromArgb("#166534"));
            Adim4Dot.Fill = new SolidColorBrush(Color.FromArgb("#E31E24"));
        }

        // ----------------------------------------------------------------
        // Adım 4 — Başlayalım
        // ----------------------------------------------------------------
        private async void BaslayalimClicked(object sender, EventArgs e)
        {
            await _ayarlar.SetAsync("KurulumTamamlandi", "1");
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}