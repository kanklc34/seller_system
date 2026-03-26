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
            if (await ZamanAsimKontrolAsync()) return;
            OturumServisi.AktiviteYenile();
            await _db.InitAsync();
            await ListeYukle();
        }

        protected override bool OnBackButtonPressed()
        {
            OturumServisi.AktiviteYenile();
            Dispatcher.Dispatch(async () => await Shell.Current.GoToAsync("//AnaSayfa"));
            return true;
        }

        private async Task<bool> ZamanAsimKontrolAsync()
        {
            if (!OturumServisi.OturumSuresiDolduMu()) return false;
            OturumServisi.Cikis();
            await DisplayAlert("Oturum Süresi Doldu", "Güvenlik nedeniyle oturumunuz sonlandırıldı.", "Tamam");
            await Shell.Current.GoToAsync("//LoginPage");
            return true;
        }

        private async Task ListeYukle()
        {
            try
            {
                var liste = await _db.TumKullanicilariGetirAsync();
                bool isPatron = OturumServisi.AktifKullanici?.Rol == "Patron";

                // KRİTİK GÜVENLİK: Eğer giren kişi patron değilse şifreleri maskele!
                if (!isPatron)
                {
                    foreach (var k in liste)
                    {
                        k.Sifre = "******";
                    }
                }

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

            OturumServisi.AktiviteYenile();

            var yeniKullanici = new Kullanici
            {
                KullaniciAdi = YeniKullaniciAdiEntry.Text.Trim(),
                // DÜZELTME: Şifreyi hashleyerek oluşturuyoruz
                Sifre = GuvenlikServisi.Hashle(YeniSifreEntry.Text.Trim()),
                Rol = RolPicker.SelectedItem.ToString()!
            };

            bool eklendi = await _db.KullaniciEkleAsync(yeniKullanici);

            if (eklendi)
            {
                YeniKullaniciAdiEntry.Text = string.Empty;
                YeniSifreEntry.Text = string.Empty;
                RolPicker.SelectedIndex = -1;
                await ListeYukle();
            }
            else
            {
                await DisplayAlert("Hata", "Bu kullanıcı adı zaten kullanımda!", "Tamam");
            }
        }
        private async void KullaniciSilClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Kullanici kullanici)
            {
                OturumServisi.AktiviteYenile();

                if (kullanici.Rol == "Patron" && OturumServisi.AktifKullanici?.KullaniciAdi != "admin")
                {
                    await DisplayAlert("Yetki Hatası", "Patron hesaplarını sadece Admin silebilir.", "Tamam");
                    return;
                }

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
            OturumServisi.AktiviteYenile();
            await Shell.Current.GoToAsync("//AnaSayfa");
        }
    }
}