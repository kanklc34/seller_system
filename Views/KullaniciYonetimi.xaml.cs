using Saller_System.Models;
using Saller_System.Services;

namespace Saller_System.Views
{
    public partial class KullaniciYonetimi : ContentPage
    {
        private readonly DatabaseService _db;

        public KullaniciYonetimi(DatabaseService db)
        {
            InitializeComponent();
            _db = db;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _db.InitAsync();
            await ListeYukle();
        }

        // TELEFONUN FİZİKSEL GERİ TUŞUNU ÇALIŞTIRAN KOD
        protected override bool OnBackButtonPressed()
        {
            Dispatcher.Dispatch(async () => await Shell.Current.GoToAsync("//AnaSayfa"));
            return true;
        }

        private async Task ListeYukle()
        {
            try
            {
                var liste = await _db.TumKullanicilariGetirAsync();
                KullaniciListesi.ItemsSource = liste;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", "Liste yüklenirken bir sorun oluştu: " + ex.Message, "Tamam");
            }
        }

        private async void KullaniciEkleClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(YeniKullaniciAdiEntry.Text) ||
                string.IsNullOrWhiteSpace(YeniSifreEntry.Text) ||
                RolPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Uyarı", "Lütfen tüm alanları (Ad, Şifre, Rol) eksiksiz doldurun.", "Tamam");
                return;
            }

            var yeniKullanici = new Kullanici
            {
                KullaniciAdi = YeniKullaniciAdiEntry.Text.Trim(),
                Sifre = YeniSifreEntry.Text.Trim(),
                Rol = RolPicker.SelectedItem.ToString()
            };

            await _db.KullaniciEkleAsync(yeniKullanici);

            // Giriş alanlarını temizle
            YeniKullaniciAdiEntry.Text = string.Empty;
            YeniSifreEntry.Text = string.Empty;
            RolPicker.SelectedIndex = -1;

            await ListeYukle();
        }

        private async void KullaniciSilClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Kullanici kullanici)
            {
                // Güvenlik kontrolü: Patron silmek onay gerektirir
                string mesaj = kullanici.Rol == "Patron"
                    ? "UYARI: Bir Patron hesabını silmek üzeresiniz. Onaylıyor musunuz?"
                    : $"{kullanici.KullaniciAdi} kullanıcısı silinecektir. Onaylıyor musunuz?";

                bool onay = await DisplayAlert("Silme Onayı", mesaj, "Evet, Sil", "Vazgeç");
                if (onay)
                {
                    await _db.KullaniciSilAsync(kullanici);
                    await ListeYukle();
                }
            }
        }

        private async void GeriClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//AnaSayfa");
        }
    }
}