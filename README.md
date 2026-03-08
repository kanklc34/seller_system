# 🥩 Kasap Pro

> **Barcode-Based Butcher Shop Sales Tracking System**  
> .NET MAUI · Android · SQLite · v1.0

![Platform](https://img.shields.io/badge/Platform-Android-green?style=flat-square&logo=android)
![Framework](https://img.shields.io/badge/Framework-.NET%20MAUI-512BD4?style=flat-square&logo=dotnet)
![Database](https://img.shields.io/badge/Database-SQLite-003B57?style=flat-square&logo=sqlite)
![Version](https://img.shields.io/badge/Version-1.0.0-blue?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-yellow?style=flat-square)

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [User Roles](#-user-roles)
- [Setup Wizard](#-setup-wizard)
- [Weight-Based (Gramaj) System](#-weight-based-gramaj-system)
- [Reports & Excel Export](#-reports--excel-export)
- [Technical Details](#-technical-details)
- [Security](#-security)
- [Project Structure](#-project-structure)
- [Getting Started](#-getting-started)

---

## 🔍 Overview

**Kasap Pro** is an Android application built specifically for butcher shops to manage barcode-based sales tracking. It combines barcode scanning, weight-based pricing, profit tracking, and Excel reporting into a single offline-capable app.

- 📱 All data is stored **locally on the device** — no internet required
- ⚡ Optimized for single-register, single-device usage
- 🌙 Light / Dark theme support
- 🏪 First-launch setup wizard for store configuration

---

## ✨ Features

| Feature | Description |
|---|---|
| 🔐 User Management | Admin, Manager, and Cashier roles with role-based access control |
| 📷 Barcode Scanning | Real-time camera barcode scanning via ZXing.Net.MAUI |
| ⚖️ Weight (Gramaj) System | 8 scale barcode formats supported; automatic kg/price calculation |
| 🛒 Sales Cart | Multi-item cart with custom pricing per item |
| 💰 Purchase / Sale Price | Separate buy and sell price tracking; automatic profit calculation |
| 📊 Reports | Daily and monthly revenue and profit reports |
| 📥 Excel Export | Export daily, monthly, or all sales to .xlsx |
| 📈 Price History | Track historical price changes per product |
| 🌙 Dark Mode | Theme preference saved and restored on each launch |
| 🏪 Setup Wizard | First-launch wizard for store info, scale config, and admin password |

---

## 👥 User Roles

| Role | Permissions |
|---|---|
| **Admin** | Full access — user management, settings, reports, product CRUD |
| **Manager** | Reports, product add/edit, sales — no user management |
| **Cashier** | Sales and barcode scanning only — no prices, no reports |

> Default credentials: `admin / 1234`, `yonetici / 1234`, `kasiyer / 1234`  
> ⚠️ The admin password is changed during the first-launch setup wizard.

---

## 🧙 Setup Wizard

On first launch, the app automatically opens the **Setup Wizard** before the login screen:

```
Step 1 → Store name & phone number
Step 2 → Scale barcode format detection (skippable if no scale)
Step 3 → Set admin password (replaces default "1234")
Step 4 → Summary & launch
```

Settings can be updated later from the **⚙️ Settings** page.

**Phone number format (Turkey):**
```
Mobile : 0544 444 44 44  (11 digits)
Landline: 0312 444 44 44  (11 digits)
```

---

## ⚖️ Weight-Based (Gramaj) System

Scale-printed barcodes start with `2` and are 13 digits long.  
The app automatically detects the format and calculates the weight in grams.

**Supported formats:**

| Format | Prefix | Product Code | Weight Digits | Check |
|--------|--------|--------------|---------------|-------|
| F1 | 1 | 6 digits | positions 7–11 | 5 digits |
| F2 | 1 | 5 digits | positions 6–10 | 5 digits |
| F3 | 1 | 4 digits | positions 5–9  | 6 digits |
| F4 | 1 | 5 digits | positions 6–10 | 4 digits |
| F5 | 1 | 4 digits | positions 5–9  | 5 digits |
| F6 | 1 | 6 digits | positions 7–11 | 4 digits |
| F7 | 2 | 5 digits | positions 7–11 | 5 digits |
| F8 | 2 | 4 digits | positions 6–10 | 5 digits |

- Valid weight range: **1g – 30,000g (30 kg)**
- Values outside this range are treated as invalid

---

## 📊 Reports & Excel Export

### On-Screen Reports

| Metric | Description |
|---|---|
| Daily Revenue | Total sales amount for today |
| Daily Sales Count | Number of items sold today |
| Daily Profit | Today's revenue minus purchase cost |
| Monthly Revenue | Total sales for the current month |
| Monthly Profit | Total profit for the current month |

### Excel Export Options

- 📅 Today's Sales
- 📆 This Month
- 📋 All Sales

> Excel files are saved to the device storage. The file path is shown after export.  
> ⚠️ Sales older than **30 days** are automatically purged. Regular Excel exports are recommended as backups.

---

## 🛠 Technical Details

### Tech Stack

| Component | Technology |
|---|---|
| Framework | .NET MAUI (Android) |
| Database | SQLite via `sqlite-net-pcl 1.9.172` |
| Barcode Scanner | `ZXing.Net.MAUI.Controls 0.4.0` |
| Excel Export | `ClosedXML 0.105.0` |
| Password Hashing | SHA-256 |

### Requirements

- Android **8.0 (API 26)** or higher
- Camera permission (barcode scanning)
- Storage permission (Excel export)

### Database Location

```
/data/data/com.sallersystem.app/files/saller.db
```

**Tables:** `Urun`, `Satis`, `Kullanici`, `FiyatGecmisi`, `Ayarlar`

---

## 🔒 Security

- All passwords are hashed with **SHA-256** — plain text is never stored
- Cashier role cannot view purchase prices or access reports
- Admin role cannot be assigned by a Manager
- Users cannot be deleted by non-Admin accounts
- The setup wizard runs **only once**

---

## 📁 Project Structure

```
Saller_System/
├── Models/
│   ├── Urun.cs                 # Product model
│   ├── Satis.cs                # Sale model (includes profit)
│   ├── Kullanici.cs            # User model
│   ├── SepetItem.cs            # Cart item model
│   ├── FiyatGecmisi.cs         # Price history model
│   └── Ayarlar.cs              # Settings model
├── Views/
│   ├── SplashSayfa.xaml        # Splash / loading screen
│   ├── KurulumSihirbazi.xaml   # First-launch setup wizard
│   ├── LoginPage.xaml          # Login screen
│   ├── AnaSayfa.xaml           # Main menu
│   ├── BarkodSayfa.xaml        # Barcode scanner
│   ├── UrunListesi.xaml        # Product list
│   ├── UrunEkle.xaml           # Add product
│   ├── UrunDuzenle.xaml        # Edit product
│   ├── SepetSayfa.xaml         # Sales cart
│   ├── Raporlar.xaml           # Reports
│   ├── KullaniciYonetimi.xaml  # User management
│   ├── AyarlarSayfa.xaml       # Settings
│   └── FiyatGecmisiSayfa.xaml  # Price history
├── Services/
│   ├── DatabaseService.cs      # SQLite operations
│   ├── OturumServisi.cs        # Session management
│   ├── GuvenlikServisi.cs      # SHA-256 hashing
│   ├── ExcelServisi.cs         # Excel export
│   ├── SepetServisi.cs         # Cart management
│   ├── TartiServisi.cs         # Scale barcode parsing
│   ├── UrunDuzenleServisi.cs   # Product edit state
│   └── AyarlarServisi.cs       # Settings persistence
├── Platforms/Android/
│   ├── MainActivity.cs
│   └── Resources/values/styles.xml
├── App.xaml(.cs)
├── AppShell.xaml(.cs)
└── MauiProgram.cs
```

---

## 🚀 Getting Started

### Prerequisites

- [Visual Studio 2022](https://visualstudio.microsoft.com/) with .NET MAUI workload
- Android SDK (API 26+)
- Android device or emulator

### Build & Run

```bash
# Clone the repository
git clone https://github.com/yourusername/kasap-pro.git

# Open Saller_System.sln in Visual Studio
# Select your Android device or emulator
# Press F5 or click Run
```

### First Launch

1. The **Setup Wizard** opens automatically
2. Enter your store name and phone number
3. Optionally scan a scale barcode for automatic format detection
4. Set your admin password
5. Log in with `admin` and your new password

---

## 📄 License

This project is licensed under the MIT License.

---

<p align="center">
  🥩 <strong>Kasap Pro</strong> — Built with .NET MAUI
</p>
