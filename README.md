# Ürün-Sipariş Yönetim Sistemi API
Bu proje, .NET 9 kullanılarak geliştirilmiş, katmanlı mimariye sahip bir Ürün-Sipariş Yönetim Sistemi API'sidir. Proje, Clean Arch.-RESTful standartlarına uygun olarak tasarlanmıştır ve JWT tabanlı kimlik doğrulama, rol tabanlı yetkilendirme, Entity Framework Core, CQRS pattern, ve daha birçok modern yazılım geliştirme tekniği içermektedir.
## 🚀 Özellikler
- **Kimlik Doğrulama ve Yetkilendirme**: JWT tabanlı kimlik doğrulama ve rol tabanlı yetkilendirme (Admin, User).
- **Ürün Yönetimi**: Ürünlerin CRUD operasyonları, filtreleme, sıralama ve sayfalama.
- **Sipariş Yönetimi**: Sipariş oluşturma, görüntüleme ve iptal etme.
- **Sağlık Kontrolleri**: API'nin sağlık durumunu kontrol etmek için endpoint'ler.
- **Sürüm Kontrolü**: API sürüm kontrolü (v1).
- **Rate Limiting**: Kullanıcı rolüne göre farklı rate limiting politikaları.
- **Caching**: ETag ile koşullu istekler ve dağıtık önbellekleme (Redis).
- **Localization**: Türkçe ve İngilizce destekli hata mesajları.
- **Loglama**: Serilog kullanılarak konsol ve dosya tabanlı loglama.
- **Hata Yönetimi**: Global hata yakalama ve standart hata yanıtları.
## 🏗️ Mimari
Proje, **Clean Architecture** prensipleri doğrultusunda katmanlı mimari ile geliştirilmiştir:
- **API Katmanı**: Controller'lar, middleware'ler ve API konfigürasyonu.
- **Application Katmanı**: CQRS pattern, servisler, DTO'lar, validasyonlar.
- **Domain Katmanı**: Entity'ler,enum'lar 
- **Infrastructure Katmanı**: Repository pattern, Entity Framework Core, migration'lar.
### 📦 Teknoloji Yığını
- **.NET 9**
- **Entity Framework Core **
- **PostgreSQL** (Ana veritabanı)
- **Redis** (Dağıtık önbellekleme)
- **JWT** (Kimlik doğrulama)
- **FluentValidation** (Doğrulama)
- **Mapping** (Nesne dönüşümü)
- **MediatR** (CQRS pattern)
- **Serilog** (Loglama)
- **xUnit** (Test)
- **Moq** (Mocking)
- **FluentAssertions** (Test assertion'ları)
- **Docker** (Containerization)
## 🔧 Kurulum ve Çalıştırma
### Önkoşullar
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/products/docker-desktop) (Opsiyonel, container için)
- [PostgreSQL](https://www.postgresql.org/download/) (Opsiyonel, local development için)
- [Redis](https://redis.io/download) (Opsiyonel, local development için)
### 1. Repository'yi Klonlayın
```bash
git clone <repository-url>
cd ShopCaseStudy
```
### 2. Veritabanı Kurulumu
#### Seçenek 1: Docker ile (Tavsiye Edilen)
```bash
docker-compose up -d
```
Bu komut, PostgreSQL ve Redis container'larını başlatacaktır.
#### Seçenek 2: Manuel Kurulum
PostgreSQL ve Redis'i manuel olarak kurun ve çalıştırın. Ardından `appsettings.json` dosyasındaki connection string'leri güncelleyin.
### 3. Migration'ları Uygulayın
```bash
cd src/Infrastructure
dotnet ef database update --startup-project ../Api
```
### 4. Uygulamayı Çalıştırın
```bash
cd src/Api
dotnet run
```
Uygulama varsayılan olarak `http://localhost:5164` adresinde çalışacaktır.
### 5. Swagger UI
Tarayıcınızda `http://localhost:5164/swagger` adresine giderek API dokümantasyonunu görüntüleyebilirsiniz.
## 🧪 Testler
Proje, unit test ve integration test'leri içermektedir.
### Unit Testleri Çalıştırma
```bash
dotnet test tests/UnitTests/
```
### Integration Testleri Çalıştırma
Integration testleri çalıştırmak için öncelikle PostgreSQL ve Redis'in çalışıyor olması gerekmektedir.
```bash
dotnet test tests/IntegrationTests/
```
```bash
pg_dump -h localhost -U postgres -d shop_db -f database_dump.sql
```
Bu dosyayı proje dizininde `database` adlı bir klasöre koyabilirsiniz.
## 🌐 API Endpoint'leri
### Kimlik Doğrulama
- `POST /api/v1/auth/register` - Kullanıcı kaydı
- `POST /api/v1/auth/login` - Kullanıcı girişi
### Ürün Yönetimi
- `GET /api/v1/products` - Ürünleri listeleme (sayfalama, filtreleme, sıralama)
- `GET /api/v1/products/{id}` - ID ile ürün getirme
- `POST /api/v1/products` - Yeni ürün oluşturma (Admin)
- `PUT /api/v1/products/{id}` - Ürün güncelleme (Admin)
- `DELETE /api/v1/products/{id}` - Ürün silme (Admin)
### Sipariş Yönetimi
- `GET /api/v1/orders` - Siparişleri listeleme
- `GET /api/v1/orders/{id}` - ID ile sipariş getirme
- `POST /api/v1/orders` - Yeni sipariş oluşturma
- `PUT /api/v1/orders/{id}/cancel` - Sipariş iptali
### Sistem
- `GET /health` - Sistem sağlık durumu
## 🔒 Güvenlik
- **JWT Token**: Tüm endpoint'ler (login ve register hariç) JWT token gerektirir.
- **Rol Tabanlı Erişim**: Admin ve User rolleri mevcuttur. Admin tüm ürün ve siparişleri yönetebilir, User sadece kendi siparişlerini.
- **Rate Limiting**: Kullanıcı rolüne göre farklı rate limiting kuralları uygulanır. Admin sınırsız, User ise 10 dakikada 100 istek ile sınırlıdır.
- **HTTPS**: Production ortamında HTTPS zorunludur.
## 📊 Loglama
Loglama, Serilog kullanılarak yapılmaktadır. Loglar konsola ve `logs/api-.log` dosyasına yazılır. Log dosyaları günlük olarak rotate edilir.
## 🚀 CI/CD
Proje, GitHub Actions ile CI/CD pipeline'ına sahiptir. Her push işleminde:
- Unit testler ve integration testler çalıştırılır.
- Docker image'ı oluşturulur ve Docker Hub'a push edilir.
- Staging ve production ortamlarına otomatik deploy yapılır.
CI yapılandırması `.github/workflows/ci.yml` dosyasında bulunmaktadır.
## 📝 Proje Yapısı
```
ShopCaseStudy/
├── src/
│   ├── Api/ (API Katmanı)
│   ├── Application/ (İş Mantığı Katmanı)
│   ├── Domain/ (Domain Katmanı)
│   └── Infrastructure/ (Veri Erişim Katmanı)
├── tests/
│   ├── UnitTests/ (Unit Testler)
│   └── IntegrationTests/ (Integration Testler)
├── database/ (Veritabanı Diagram ve Dump)
├── docker-compose.yml (Docker Compose)
└── README.md (Bu dosya)
```
## 🧩 Ek Özellikler
- **ETag ve Conditional GET**: Ürün listesi için ETag desteği. Değişmeyen veri için `304 Not Modified` döndürür.
- **Localization**: Türkçe ve İngilizce hata mesajları.
- **Cursor-based Pagination**: Büyük veri setleri için performanslı sayfalama.
- **Redis Caching**: Sık erişilen veriler için Redis önbellekleme.
- **Policy-Based Authorization**: Esnek yetkilendirme politikaları.
## 👨‍💻 Geliştirici
Burak Güven - burakguven3599@gmail.com - https://www.linkedin.com/in/burakkguven
## 📞 İletişim
Herhangi bir sorunuz veya geri bildiriminiz için lütfen burakguven3599@gmail.com üzerinden iletişime geçin.
