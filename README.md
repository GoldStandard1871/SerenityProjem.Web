# SerenityProjem.Web - Real-Time User Activity Tracking System

Bu proje, **Serenity.NET** framework'ü kullanılarak geliştirilmiş ve **gerçek zamanlı kullanıcı aktivite takip sistemi** ile genişletilmiş kapsamlı bir web uygulamasıdır.

## 🌟 Ana Özellikler

### 📊 Real-Time User Activity Tracking System
- **Anlık kullanıcı görüntüleme**: Dashboard'da online kullanıcıları gerçek zamanlı olarak takip edin
- **Konum takibi**: Kullanıcıların hangi lokasyondan giriş yaptığını IP geolocation ile görüntüleyin
- **Aktivite durumu**: Kullanıcıların anlık kullanım durumunu ve sistem içindeki hareketlerini izleyin
- **Geçmiş kayıtları**: Giriş/çıkış geçmişi ve aktivite loglarını veritabanında saklayın

### 🔐 Güvenlik ve Yetkilendirme
- **Rol tabanlı erişim**: Admin kullanıcılar tüm kullanıcıları, normal kullanıcılar sadece kendi aktivitelerini görebilir
- **SignalR ile güvenli bağlantı**: Authorize edilmiş kullanıcılar için gerçek zamanlı güncellemeler
- **Oturum yönetimi**: Gereksiz log kayıtlarını önleyen akıllı session tracking

## 🚀 Yeni Eklenen Özellikler

### 1. Real-Time Dashboard Widgets
- **Online Users Counter**: Anlık online kullanıcı sayısı
- **User Location Tracking**: IP tabanlı konum bilgileri (şehir, ülke, ISP)
- **Activity Status**: Active/Idle/Away durumları
- **Recent Activity Log**: Son aktivitelerin zaman damgalı listesi

### 2. SignalR Hub Integration
- **UserActivityHub**: Gerçek zamanlı bağlantı yönetimi
- **Automatic Reconnection**: Bağlantı koptuğunda otomatik yeniden bağlanma
- **Group Management**: Kullanıcı grupları için organize edilmiş mesajlaşma

### 3. Database Schema Enhancements
```sql
-- UserActivityHistory tablosu
CREATE TABLE UserActivityHistory (
    Id int IDENTITY(1,1) PRIMARY KEY,
    UserId nvarchar(100) NOT NULL,
    Username nvarchar(100) NOT NULL,
    ActivityType nvarchar(50) NOT NULL,
    IpAddress nvarchar(45),
    UserAgent nvarchar(500),
    Location nvarchar(200),
    Isp nvarchar(200),
    Timezone nvarchar(100),
    ActivityTime datetime2 NOT NULL,
    Details nvarchar(max)
);
```

## 📁 Proje Yapısı

### Yeni Eklenen Dosyalar

#### Backend Components
```
Modules/Administration/UserActivity/
├── UserActivityService.cs          # Ana servis logic
├── UserActivityHub.cs              # SignalR hub
├── UserActivityEndpoint.cs         # REST API endpoints
├── UserActivityClient.ts           # TypeScript client
└── UserActivityGlobal.ts          # Global activity tracker
```

#### Database Migrations
```
Migrations/DefaultDB/
├── DefaultDB_20250724_1200_UserActivityHistory.cs
├── DefaultDB_20250722_1500_AddAdmin2User.cs
└── DefaultDB_20250722_1501_AddAdmin2UserSimple.cs
```

#### Frontend Integration
```
Modules/Common/Dashboard/
└── DashboardIndex.cshtml           # Enhanced dashboard with activity widgets
```

## 🛠️ Kurulum ve Çalıştırma

### Gereksinimler
- .NET 6.0 veya üzeri
- SQL Server (LocalDB desteklenir)
- Node.js (frontend build için)

### Kurulum Adımları

1. **Projeyi klonlayın:**
```bash
git clone https://github.com/GoldStandard1871/SerenityProjem.Web.git
cd SerenityProjem.Web
```

2. **NuGet paketlerini yükleyin:**
```bash
dotnet restore
```

3. **Node.js bağımlılıklarını yükleyin:**
```bash
npm install
```

4. **Veritabanını oluşturun:**
```bash
dotnet run -- --migrate
```

5. **Uygulamayı çalıştırın:**
```bash
dotnet run
```

## 📊 Kullanım Rehberi

### Admin Kullanıcısı
- **Tüm kullanıcı aktivitelerini görme**: Dashboard'dan tüm online kullanıcıları ve aktivitelerini izleyin
- **Lokasyon takibi**: Kullanıcıların giriş yaptığı IP adreslerini ve konum bilgilerini görün
- **Geçmiş raporları**: Detaylı giriş/çıkış geçmişi ve session süreleri

### Normal Kullanıcı
- **Kişisel aktivite görüntüleme**: Sadece kendi aktivitelerinizi görebilirsiniz
- **Gizlilik koruması**: Diğer kullanıcıların bilgilerine erişim yok

## 🔧 Teknik Detaylar

### SignalR Integration
```typescript
// UserActivityClient.ts - Gerçek zamanlı bağlantı
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/userActivityHub")
    .withAutomaticReconnect([0, 2000, 5000, 10000])
    .build();
```

### IP Geolocation Service
```csharp
// UserActivityService.cs - Konum belirleme
private async Task<LocationInfo> GetDetailedLocationFromIpAsync(string ipAddress)
{
    var response = await httpClient.GetStringAsync(
        $"http://ip-api.com/json/{ipAddress}?fields=country,city,regionName,isp,timezone"
    );
    return JsonSerializer.Deserialize<LocationInfo>(response);
}
```

### Session Management
```csharp
// Akıllı session tracking - gereksiz logları önler
var isRealLogin = !OnlineUsers.ContainsKey(userId) || 
                  DateTime.UtcNow - lastLogin > TimeSpan.FromMinutes(5);
```

## 📈 Dashboard Özellikleri

### Real-Time Widgets
1. **Online Users Counter**: Canlı kullanıcı sayacı
2. **User Activity Table**: Detaylı kullanıcı listesi
   - Kullanıcı adı
   - Konum (şehir, ülke)
   - ISP ve Timezone bilgileri
   - Bağlantı zamanı
   - Son aktivite
   - Durum (Active/Idle/Away)

3. **Recent Activity Log**: 
   - Giriş/çıkış aktiviteleri
   - Zaman damgaları
   - Konum bilgileri
   - Otomatik yenileme

## 🔒 Güvenlik Özellikleri

### Role-Based Access Control
```csharp
var isAdmin = Context.User?.IsInRole("Administrators") == true || 
              Context.User?.IsInRole("admin") == true ||
              currentUser.ToLower() == "admin";

// Admin: Tüm kullanıcıları görebilir
// User: Sadece kendi aktivitelerini görebilir
```

### Privacy Protection
- Hassas bilgiler sadece admin kullanıcılar tarafından görülebilir
- IP adresleri ve konum bilgileri güvenli şekilde saklanır
- Session tracking ile gereksiz veri toplama önlenir

## 🚀 Performance Optimizations

### Connection Management
- **Automatic Reconnection**: Bağlantı koptuğunda otomatik yeniden bağlanma
- **Connection Pooling**: Verimli veritabanı bağlantı yönetimi
- **Cached Responses**: Sık kullanılan verilerin cache'lenmesi

### Database Optimization
- **Indexed Queries**: Performanslı sorgular için index kullanımı
- **Batch Operations**: Toplu işlemler ile performans artışı
- **Session Filtering**: Gereksiz log kayıtlarının önlenmesi

## 🤝 Katkıda Bulunma

Bu proje açık kaynak olarak geliştirilmektedir. Katkılarınızı bekliyoruz!

1. Fork edin
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Değişikliklerinizi commit edin (`git commit -m 'Add amazing feature'`)
4. Branch'inizi push edin (`git push origin feature/amazing-feature`)
5. Pull Request oluşturun

## 📞 İletişim

Proje hakkında sorularınız için:
- GitHub Issues: [SerenityProjem.Web Issues](https://github.com/GoldStandard1871/SerenityProjem.Web/issues)
- Email: [Projenin sahibi ile iletişim]

## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Detaylar için `LICENSE` dosyasına bakınız.

---

**Not**: Bu proje [Claude Code](https://claude.ai/code) AI asistanı ile birlikte geliştirilmiştir.

## 🔍 Ek Kaynaklar

- [Serenity.NET Documentation](https://serenity.is/docs/)
- [SignalR Documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr/introduction)
- [IP-API Documentation](https://ip-api.com/docs)