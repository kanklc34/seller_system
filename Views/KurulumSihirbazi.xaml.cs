using Saller_System.Services;

namespace Saller_System.Views
{
    public partial class KurulumSihirbazi : ContentPage
    {
        private readonly AyarlarServisi _ayarlar;
        private readonly DatabaseService _db;
        private string? _algılananPrefix;

        public KurulumSihirbazi(AyarlarServisi ayarlar, DatabaseService db)
        {
            InitializeComponent();
            _ayarlar = ayarlar;
            _db = db;
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
                TelefonHataLabel.Text = "❌ Türkiye telefon numarası 11 hane olmalıdır (Örn: 0532 123 45 67)";
                TelefonHataLabel.IsVisible = true;
                return;
            }

            TelefonHataLabel.IsVisible = false;
            await _ayarlar.SetAsync("MagazaAdi", magazaAdi);
            await _ayarlar.SetAsync("Telefon", string.IsNullOrEmpty(telefon) ? "" :
     telefon[..4] + " " + telefon[4..7] + " " + telefon[7..9] + " " + telefon[9..]);

            Adim1Panel.IsVisible = false;
            Adim2Panel.IsVisible = true;
            Adim1Dot.Fill = new SolidColorBrush(Color.FromArgb("#166534"));
            Adim2Dot.Fill = new SolidColorBrush(Color.FromArgb("#2E75B6"));
        }
        private void TelefonUnfocused(object sender, FocusEventArgs e)
        {
            string temiz = "";
            foreach (char c in TelefonEntry.Text ?? "")
                if (char.IsDigit(c) && temiz.Length < 11)
                    temiz += c;

            if (temiz.Length == 0) return;

            string formatted = "";
            for (int i = 0; i < temiz.Length; i++)
            {
                if (i == 4 || i == 7 || i == 9) formatted += " ";
                formatted += temiz[i];
            }

            TelefonEntry.Text = formatted;
        }
        private void FormatAlgilaClicked(object sender, EventArgs e)
        {
            string barkod = TeraziBarkodEntry.Text?.Trim() ?? "";

            if (barkod.Length != 13)
            {
                FormatSonucLabel.Text = "❌ Geçersiz barkod (13 hane olmalı)";
                FormatSonucLabel.TextColor = Colors.Red;
                FormatSonucLabel.IsVisible = true;
                Adim2DevamBorder.Opacity = 0.5;
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
                FormatSonucLabel.Text = "❌ Format algılanamadı, farklı bir barkod deneyin.";
                FormatSonucLabel.TextColor = Colors.Red;
                FormatSonucLabel.IsVisible = true;
                Adim2DevamBorder.Opacity = 0.5;
            }
        }

        private async void Adim2DevamClicked(object sender, EventArgs e)
        {
            if (_algılananPrefix != null)
                await _ayarlar.SetAsync("TaraziPrefix", _algılananPrefix);

            Adim2Panel.IsVisible = false;
            Adim3Panel.IsVisible = true;
            Adim2Dot.Fill = new SolidColorBrush(Color.FromArgb("#166534"));
            Adim3Dot.Fill = new SolidColorBrush(Color.FromArgb("#2E75B6"));
        }

        private async void TeraziAtlaClicked(object sender, EventArgs e)
        {
            Adim2Panel.IsVisible = false;
            Adim3Panel.IsVisible = true;
            Adim2Dot.Fill = new SolidColorBrush(Color.FromArgb("#166534"));
            Adim3Dot.Fill = new SolidColorBrush(Color.FromArgb("#2E75B6"));
        }

        private async void Adim3DevamClicked(object sender, EventArgs e)
        {
            string sifre = YeniSifreEntry.Text ?? "";
            string sifreTekrar = YeniSifreTekrarEntry.Text ?? "";

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
            var admin = await _db.KullaniciGetirAsync("admin");
            if (admin != null)
            {
                admin.Sifre = GuvenlikServisi.Hashle(sifre);
                await _db.KullaniciGuncelleAsync(admin);
            }

            string magazaAdi = await _ayarlar.GetAsync("MagazaAdi", "");
            string telefon = await _ayarlar.GetAsync("Telefon", "");
            string prefix = await _ayarlar.GetAsync("TaraziPrefix", "Tanımsız");

            KurulumOzetLabel.Text =
                $"🏪 {magazaAdi}\n" +
                $"📞 {(string.IsNullOrEmpty(telefon) ? "Telefon girilmedi" : telefon)}\n" +
                $"⚖️ Terazi Prefix: {prefix}\n" +
                $"🔐 Admin şifresi belirlendi";

            Adim3Panel.IsVisible = false;
            Adim4Panel.IsVisible = true;
            Adim3Dot.Fill = new SolidColorBrush(Color.FromArgb("#166534"));
            Adim4Dot.Fill = new SolidColorBrush(Color.FromArgb("#2E75B6"));
        }

        private async void BaslayalimClicked(object sender, EventArgs e)
        {
            await _ayarlar.SetAsync("KurulumTamamlandi", "1");
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}