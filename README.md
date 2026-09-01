# 🏷️ GeminiLabelApi - Smart Product Label OCR & Search Service

[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![Google Gemini](https://img.shields.io/badge/AI-Google%20Gemini%20API-blue.svg)](https://ai.google.dev/)
[![SQLite](https://img.shields.io/badge/Database-SQLite-lightgrey.svg)](https://www.sqlite.org/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**GeminiLabelApi**, fiziksel tekstil ve giyim ürün etiketlerindeki metinleri (yıkama talimatları, kumaş karışımları, ürün kodları, seri numaraları vb.) **Google Gemini Multimodal AI API** entegrasyonuyla yüksek doğrulukla analiz eden, dijitalleştiren ve geçmişe dönük aranabilir kılan bir RESTful API servisidir.

> *Bu proje, kurumsal e-ticaret ve katalog süreçlerindeki etiket okuma / arşivleme gereksinimlerine yönelik geliştirilmiştir.*

---

## 🚀 Özellikler

- 🔍 **Yapay Zekâ Destekli OCR:** Gemini Multimodal modeli ile karmaşık/bozuk açılı etiket görsellerinden hatasız metin ve nitelik çıkarma.
- 🗄️ **Yapılandırılmış Veritabanı:** Çıkarılan etiket verilerinin SQLite üzerinde indeksli olarak saklanması.
- 🔎 **Gelişmiş Arama & Filtreleme:** Etiket içeriğinde geçen metinlere, kodlara veya kumaş içeriklerine göre hızlı arama endpoint'leri.
- ⚡ **RESTful API Mimarisi:** Modern .NET altyapısı ve OpenAPI/Swagger dokümantasyonu.

---

## 🛠️ Teknolojiler

- **Backend:** C# / .NET 10.0
- **AI Entegrasyonu:** Google Gemini API
- **Veritabanı & ORM:** SQLite / Entity Framework Core
- **Dokümantasyon:** Swagger / OpenAPI

---

## 📂 Proje Yapısı

```text
GeminiLabelApi/
├── Controllers/         # API Endpoint tanımlamaları
├── Services/            # Gemini AI entegrasyonu & OCR mantığı
├── Models/              # DTO ve Entity modelleri
├── Data/                # EF Core DbContext & konfigürasyonlar
├── appsettings.Example.json
└── Program.cs