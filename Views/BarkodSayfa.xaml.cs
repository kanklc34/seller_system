using Saller_System.Models;
using Saller_System.Services;
using ZXing.Net.Maui;
using Microsoft.Maui.ApplicationModel;
using Plugin.Maui.Audio;

namespace Saller_System.Views
{
    public partial class BarkodSayfa : ContentPage
    {
        private readonly DatabaseService _db;
        private readonly SepetServisi _sepet;
        private readonly AyarlarServisi _ayarlar;
        private readonly IAudioManager _audioManager;
        private IDispatcherTimer? _flasZamanlayici;
        bool _bipCalAktifMi;
        bool _manuelBipAktifMi;
        bool _sonIslemManuelMi = false;
        private Urun? _bulunanUrun;

        public BarkodSayfa(DatabaseService db, SepetServisi sepet, AyarlarServisi ayarlar, IAudioManager audioManager)
        {
            InitializeComponent();
            _db = db;
            _sepet = sepet;
            _ayarlar = ayarlar;
            _audioManager = audioManager;

            BarkodOkuyucu.Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormat.Ean13 | BarcodeFormat.Ean8 | BarcodeFormat.Code128 | BarcodeFormat.QrCode,
                AutoRotate = true,
                Multiple = false
            };
        }

        private async Task BipCal()
        {
            if (!_bipCalAktifMi) return;

            if (_sonIslemManuelMi && !_manuelBipAktifMi) return;

            try
            {
                var player = _audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("bip.wav"));
                player.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ses Çalma Hatası: {ex.Message}");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (await ZamanAsimKontrolAsync()) return;
            OturumServisi.AktiviteYenile();
            var magazaAdi = await _ayarlar.GetAsync("MagazaAdi", "");
            MagazaAdiLabel.Text = string.IsNullOrWhiteSpace(magazaAdi) ? "ÖZ BİGA ET" : magazaAdi.ToUpper();
            BarkodOkuyucu.IsDetecting = true;
            MesajBorder.IsVisible = false;
            _bipCalAktifMi = await _ayarlar.GetAsync("BipCal", "1") == "1";
            _manuelBipAktifMi = await _ayarlar.GetAsync("ManuelBip", "1") == "1";

            if (_flasZamanlayici == null) {
                _flasZamanlayici = Dispatcher.CreateTimer();
                _flasZamanlayici.Interval = TimeSpan.FromSeconds(30);
                _flasZamanlayici.Tick += (s, e) => FlasKapat();
            }
        }

        protected override void OnDisappearing()
        {
            FlasKapat();
            base.OnDisappearing();
            BarkodOkuyucu.IsDetecting = false;
        }

        private async Task<bool> ZamanAsimKontrolAsync()
        {
            if (!OturumServisi.OturumSuresiDolduMu()) return false;
            FlasKapat();
            OturumServisi.Cikis();
            await DisplayAlert("Oturum Süresi Doldu", "Güvenlik nedeniyle oturumunuz sonlandırıldı.", "Tamam");
            await Shell.Current.GoToAsync("//LoginPage");
            return true;
        }

        private async void BarkodOkundu(object sender, BarcodeDetectionEventArgs e)
        {
            var result = e.Results.FirstOrDefault();
            if (result == null) return;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                OturumServisi.AktiviteYenile();
                BarkodEntry.Text = result.Value;
                BarkodOkuyucu.IsDetecting = false;

                _sonIslemManuelMi = false;

                if (BarkodOkuyucu.IsTorchOn) {
                    _flasZamanlayici?.Stop();
                    _flasZamanlayici?.Start();
                }

                await UrunGetirAsync(result.Value);
            });
        }

        private void FlasTapped(object sender, EventArgs e) 
        {
            if (BarkodOkuyucu.IsTorchOn) {
                FlasKapat();
            } else {
                BarkodOkuyucu.IsTorchOn = true;
                FlasBorder.BackgroundColor = Color.FromArgb("#D4AF37");
                _flasZamanlayici?.Start();
            }
        }

        private void FlasKapat() 
        {
            BarkodOkuyucu.IsTorchOn = false;
            FlasBorder.BackgroundColor = Color.FromArgb("#B3000000"); 
            _flasZamanlayici?.Stop();
        }

        private async Task UrunGetirAsync(string okunanBarkod)
        {
            if (string.IsNullOrEmpty(okunanBarkod))
            {
                BarkodOkuyucu.IsDetecting = true;
                return;
            }

            try
            {
                OturumServisi.AktiviteYenile();
                await _db.InitAsync();

                string aranacakBarkod = okunanBarkod;
                decimal okunanMiktar = 1;
                bool teraziUrunuMu = false;
                string prefix = await _ayarlar.GetAsync("TeraziPrefix", "27");

                // 1. TERAZİ AYRIŞTIRMA MANTIĞI
                if (okunanBarkod.Length == 13 && okunanBarkod.StartsWith(prefix))
                {
                    teraziUrunuMu = true;
                    aranacakBarkod = okunanBarkod.Substring(2, 5);
                    string miktarStr = okunanBarkod.Substring(7, 5);
                    if (decimal.TryParse(miktarStr, out var miktar))
                        okunanMiktar = miktar / 1000m;
                }

                // 2. VERİTABANINDAN TÜM EŞLEŞENLERİ GETİR
                // Not: DatabaseService içinde BarkodIleTumunuGetirAsync metodunu yazmıştık
                var urunler = await _db.BarkodIleTumunuGetirAsync(aranacakBarkod);

                // Sıfır toleransı (Liste boşsa alternatifleri dene)
                if (urunler == null || urunler.Count == 0)
                    urunler = await _db.BarkodIleTumunuGetirAsync(aranacakBarkod.TrimStart('0'));
                if (urunler == null || urunler.Count == 0)
                    urunler = await _db.BarkodIleTumunuGetirAsync("0" + aranacakBarkod);

                // 3. ÇAKIŞMA KONTROLÜ
                if (urunler != null && urunler.Count > 1)
                {
                    // Birden fazla ürün bulundu: Butonları oluştur
                    CakismaButonlariFlex.Children.Clear();
                    CakismaPaneli.IsVisible = true;
                    UrunBilgiFrame.IsVisible = false;
                    MesajBorder.IsVisible = false;

                    foreach (var urun in urunler)
                    {
                        var btn = new Button
                        {
                            Text = urun.Ad,
                            Margin = new Thickness(5),
                            BackgroundColor = Color.FromArgb("#1E293B"), // Koyu SaaS temasına uygun
                            TextColor = Colors.White,
                            CornerRadius = 12,
                            FontSize = 13,
                            HeightRequest = 42,
                            Padding = new Thickness(15, 0)
                        };

                        // Butona basıldığında seçilen ürünü yansıt
                        btn.Clicked += (s, e) => UrunuSecVeYansit(urun, okunanMiktar, teraziUrunuMu);

                        CakismaButonlariFlex.Children.Add(btn);
                    }
                }
                else if (urunler != null && urunler.Count == 1)
                {
                    // Tek ürün bulundu: Direkt yansıt
                    CakismaPaneli.IsVisible = false;
                    UrunuSecVeYansit(urunler[0], okunanMiktar, teraziUrunuMu);
                }
                else
                {
                    // Ürün hiç yoksa
                    bool ekle = await DisplayAlert("Ürün Bulunamadı", $"'{okunanBarkod}' bulunamadı. Eklensin mi?", "Evet", "Hayır");
                    if (ekle)
                    {
                        FlasKapat();
                        UrunDuzenleServisi.HizliEkleBarkod = okunanBarkod;
                        await Shell.Current.GoToAsync("//UrunListesi");
                    }
                    else
                    {
                        BarkodOkuyucu.IsDetecting = true;
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", "İşlem hatası: " + ex.Message, "Tamam");
                BarkodOkuyucu.IsDetecting = true;
            }
        }
        private async void UrunuSecVeYansit(Urun urun, decimal miktar, bool teraziMi)
        {
            _bulunanUrun = urun;
            CakismaPaneli.IsVisible = false; // Seçim yapıldığı için paneli kapat

            if (urun.GramajliMi || teraziMi)
            {
                // Gramajlı ürün: Bilgileri göster, onay bekle
                UrunAdLabel.Text = urun.Ad;
                UrunFiyatLabel.Text = $"Birim: ₺{urun.KgFiyati:N2} / Kg";
                UrunKategoriLabel.Text = $"Kategori: {urun.Kategori}";
                AdetEntry.Text = miktar.ToString("0.###");

                UrunBilgiFrame.IsVisible = true;
                MesajBorder.IsVisible = false;
                BarkodOkuyucu.IsDetecting = false; // Kullanıcı onaylayana kadar okumayı durdur
            }
            else
            {
                // Normal ürün: Direkt sepete at
                _sepet.Ekle(urun, 1, urun.Fiyat);
                await BipCal();
                HapticFeedback.Default.Perform(HapticFeedbackType.Click);

                MesajLabel.Text = $"✅ {urun.Ad} eklendi!";
                MesajBorder.IsVisible = true;
                UrunBilgiFrame.IsVisible = false;
                BarkodEntry.Text = "";

                // Kısa bir bekleme sonrası kamerayı tekrar aç
                await Task.Delay(1000);
                BarkodOkuyucu.IsDetecting = true;
            }
        }
        private async void SatisaEkleTapped(object sender, EventArgs e)
        {
            if (_bulunanUrun == null) return;
            OturumServisi.AktiviteYenile();

            // ÇÖKMEYİ ENGELLEYEN KISIM: decimal.Parse yerine TryParse
            if (!decimal.TryParse(AdetEntry.Text, out decimal girilenMiktar))
            {
                await DisplayAlert("Hata", "Lütfen geçerli bir miktar (sayı) girin.", "Tamam");
                return;
            }

            decimal eklenecekFiyat = _bulunanUrun.GramajliMi ? _bulunanUrun.KgFiyati : _bulunanUrun.Fiyat;
            _sepet.Ekle(_bulunanUrun, girilenMiktar, eklenecekFiyat);

            // SES ÇIKAR
            await BipCal(); // Eğer bir önceki tavsiyemdeki gibi düzenlediysen await gerekmez
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);

            // Mesaj ve Temizlik
            MesajLabel.Text = $"✅ {_bulunanUrun.Ad} sepete eklendi!";
            MesajBorder.IsVisible = true;
            UrunBilgiFrame.IsVisible = false;
            BarkodEntry.Text = "";
            BarkodOkuyucu.IsDetecting = true;
        }

        private async void UrunGetirTapped(object sender, EventArgs e) 
        { 
            _sonIslemManuelMi = true;
            await UrunGetirAsync(BarkodEntry.Text); 
        }
        private async void BarkodEntry_Completed(object sender, EventArgs e) 
        { 
            _sonIslemManuelMi = true;
            await UrunGetirAsync(BarkodEntry.Text); 
        }
        private async void GeriClicked(object sender, EventArgs e) 
        { 
            FlasKapat();
            await Shell.Current.GoToAsync("//AnaSayfa"); 
        }
        private async void SepeteGitClicked(object sender, EventArgs e) 
        { 
            FlasKapat();
            await Shell.Current.GoToAsync("//SepetSayfa"); 
        }
    }
}