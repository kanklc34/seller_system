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
            await ListeyiYukle();
        }

        private async Task ListeyiYukle()
        {
            await _db.InitAsync();
            var kullanicilar = await _db.TumKullanicilariGetirAsync();
            KullaniciListesi.ItemsSource = kullanicilar;
        }

        private async void KullaniciEkleClicked(object sender, EventArgs e)
        {
            string ad = YeniKullaniciAdiEntry.Text?.Trim() ?? "";
            string sifre = YeniSifreEntry.Text?.Trim() ?? "";
            string rol = RolPicker.SelectedItem?.ToString() ?? "";

            if (string.IsNullOrEmpty(ad) || string.IsNullOrEmpty(sifre) || string.IsNullOrEmpty(rol))
            {
                await DisplayAlert("Hata", "Tüm alanları doldurun!", "Tamam");
                return;
            }

            if (!OturumServisi.AdminMi && rol == "Admin")
            {
                await DisplayAlert("Yetkisiz", "Admin eklemek için admin yetkisi gerekli!", "Tamam");
                return;
            }

            var yeniKullanici = new Kullanici
            {
                KullaniciAdi = ad,
                Sifre = GuvenlikServisi.Hashle(sifre),
                Rol = rol
            };

            await _db.KullaniciEkleAsync(yeniKullanici);
            YeniKullaniciAdiEntry.Text = "";
            YeniSifreEntry.Text = "";
            RolPicker.SelectedIndex = -1;
            await ListeyiYukle();
            await DisplayAlert("Başarılı", $"{ad} eklendi!", "Tamam");
        }

        private async void KullaniciSilClicked(object sender, EventArgs e)
        {
            if (sender is TapGestureRecognizer tap && tap.CommandParameter is Kullanici kullanici)
            {
                if (kullanici.KullaniciAdi == OturumServisi.AktifKullanici?.KullaniciAdi)
                {
                    await DisplayAlert("Hata", "Kendi hesabınızı silemezsiniz!", "Tamam");
                    return;
                }

                bool onay = await DisplayAlert("Onay", $"{kullanici.KullaniciAdi} silinsin mi?", "Evet", "Hayır");
                if (onay)
                {
                    await _db.KullaniciSilAsync(kullanici);
                    await ListeyiYukle();
                }
            }
        }

        private async void GeriClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//AnaSayfa");
    }
}