# E-Commerce Microservices

Bu projeyi .NET backend tarafında kendimi geliştirmek ve gerçek bir e-ticaret sisteminin microservice mimarisiyle nasıl geliştirilebileceğini uygulamalı olarak öğrenmek amacıyla geliştirdim.

Projede servislerin birbirinden bağımsız çalıştığı, her servisin kendi database'ine sahip olduğu bir Microservice mimarisi kullandım.

Özellikle Microservices, CQRS, MediatR, Entity Framework Core, LINQ, Dapper, RabbitMQ, Outbox Pattern, Redis, Idempotency, Docker, SQL Server ve API Gateway konularında uygulamalı çalışma yaptım.


# 🏗️ Proje Mimarisi

~~~text
                                      CLIENT
                                        │
                                        │ HTTP
                                        ▼
                              ┌───────────────────┐
                              │   API GATEWAY     │
                              │      YARP         │
                              │                   │
                              │ Routing / Gateway │
                              └─────────┬─────────┘
                                        │
              ┌─────────────────────────┼─────────────────────────┐
              │                         │                         │
              ▼                         ▼                         ▼
      ┌─────────────────┐      ┌─────────────────┐      ┌─────────────────┐
      │  AuthService    │      │ CustomerService │      │ ProductService  │
      │                 │      │                 │      │                 │
      │ Authentication  │      │ Customer        │      │ Product         │
      │ JWT             │      │ Management      │      │ Stock           │
      └────────┬────────┘      └────────┬────────┘      └────────┬────────┘
               │                        │                        │
               ▼                        ▼                        ▼
          ┌─────────┐             ┌───────────┐            ┌───────────┐
          │ AuthDb  │             │CustomerDb │            │ ProductDb │
          └─────────┘             └───────────┘            └─────┬─────┘
                                                                 │
                                                                 ▼
                                                            ┌─────────┐
                                                            │  Redis  │
                                                            │  Cache  │
                                                            └─────────┘


                              ┌──────────────────┐
                              │   OrderService   │
                              │                  │
                              │ Order Management │
                              └────────┬─────────┘
                                       │
                                       ▼
                                  ┌──────────┐
                                  │ OrderDb  │
                                  └──────────┘
~~~


# 🗄️ Database-per-Service

Her microservice kendi database'inin sorumluluğuna sahip olacak şekilde tasarlandı.

~~~text
AuthService
    │
    ▼
 AuthDb

CustomerService
    │
    ▼
CustomerDb

ProductService
    │
    ▼
ProductDb

OrderService
    │
    ▼
 OrderDb
~~~

Servisler birbirlerinin database'lerine doğrudan erişmiyor.

Örneğin ProductService, CustomerDb'ye doğrudan SQL sorgusu göndermiyor.

Servisler arasındaki iletişim API veya event-driven communication üzerinden gerçekleştiriliyor.


# 🐇 RabbitMQ Event Mimarisi

Servisler arasındaki asynchronous communication için RabbitMQ kullandım.

~~~text
                         ┌──────────────────┐
                         │    AuthService   │
                         └────────┬─────────┘
                                  │
                                  │ UserRegistered
                                  ▼
                         ┌──────────────────┐
                         │   Auth Outbox    │
                         │                  │
                         │ OutboxMessages   │
                         └────────┬─────────┘
                                  │
                                  │ OutboxPublisher
                                  ▼
                         ┌────────────────────────┐
                         │       RabbitMQ         │
                         │                        │
                         │   ecommerce.events     │
                         │      EXCHANGE          │
                         │                        │
                         │       Type: topic      │
                         └───────────┬────────────┘
                                     │
                         routing key: user.registered
                                     │
                                     ▼
                       ┌─────────────────────────┐
                       │ customer-service-queue  │
                       │                         │
                       │        QUEUE            │
                       └────────────┬────────────┘
                                    │
                                    ▼
                       ┌─────────────────────────┐
                       │   CustomerService       │
                       │       Consumer          │
                       └────────────┬────────────┘
                                    │
                                    ▼
                               CustomerDb
~~~


OrderService → ProductService event akışı:

~~~text
                         ┌──────────────────┐
                         │   OrderService   │
                         └────────┬─────────┘
                                  │
                                  │ Create Order
                                  ▼
                         ┌──────────────────┐
                         │     OrderDb      │
                         │                  │
                         │     Orders       │
                         │        +         │
                         │  OutboxMessages  │
                         └────────┬─────────┘
                                  │
                                  │ OutboxPublisher
                                  ▼
                         ┌────────────────────────┐
                         │       RabbitMQ         │
                         │                        │
                         │   ecommerce.events     │
                         │      EXCHANGE          │
                         │                        │
                         │       Type: topic      │
                         └───────────┬────────────┘
                                     │
                           routing key:
                            order.created
                                     │
                                     ▼
                       ┌─────────────────────────┐
                       │  product-service-queue  │
                       │                         │
                       │         QUEUE           │
                       └────────────┬────────────┘
                                    │
                                    ▼
                       ┌─────────────────────────┐
                       │    ProductService       │
                       │       Consumer          │
                       └────────────┬────────────┘
                                    │
                       ┌────────────┴────────────┐
                       │                         │
                       ▼                         ▼
                  ProductDb                    Redis
                  Stock Update                 Cache
~~~


# 🔀 RabbitMQ Exchange ve Routing Key Yapısı

Exchange:

~~~text
ecommerce.events
~~~

Exchange tipi:

~~~text
topic
~~~

Routing key'ler:

~~~text
user.registered
order.created
~~~

Queue'lar:

~~~text
customer-service-queue
product-service-queue
~~~

Binding yapısı:

~~~text
ecommerce.events
       │
       ├── user.registered
       │        │
       │        ▼
       │   customer-service-queue
       │        │
       │        ▼
       │   CustomerService Consumer
       │
       └── order.created
                │
                ▼
           product-service-queue
                │
                ▼
           ProductService Consumer
~~~

RabbitMQ'da manual ACK kullandım.

Mesaj başarılı şekilde işlendiğinde:

~~~text
BasicAck
~~~

kullanıyorum.

İşlem sırasında hata oluştuğunda mesajı tekrar kuyruğa almak için:

~~~text
BasicNack
requeue: true
~~~

kullanıyorum.


# 👤 AuthService

Authentication ve kullanıcı kayıt işlemlerini yönetiyor.

Sorumlulukları:

- Kullanıcı kayıt
- JWT authentication
- Password hashing
- User yönetimi
- UserRegistered event oluşturma
- Outbox Pattern

Kullanıcı kayıt olduğunda User ve OutboxMessage aynı transaction içerisinde database'e kaydediliyor.

~~~text
Register User
     │
     ▼
AuthService
     │
     ├───────────────┐
     ▼               ▼
   User         OutboxMessage
     │               │
     └───────┬───────┘
             ▼
          Commit
             │
             ▼
      OutboxPublisher
             │
             ▼
          RabbitMQ
~~~


# 👥 CustomerService

Customer bilgilerini yönetiyor.

AuthService tarafından gönderilen UserRegistered event'ini RabbitMQ üzerinden tüketiyor.

~~~text
AuthService
     │
     │ user.registered
     ▼
RabbitMQ
     │
     ▼
customer-service-queue
     │
     ▼
CustomerService Consumer
     │
     ▼
CustomerDb
~~~

Yeni kullanıcı kayıt olduğunda CustomerService otomatik olarak Customer kaydı oluşturuyor.


# 📦 ProductService

Ürün ve stok yönetiminden sorumlu.

Sorumlulukları:

- Product oluşturma
- Product listeleme
- Product detay
- Product silme
- Stock yönetimi
- Redis cache
- RabbitMQ Consumer
- Idempotency

OrderService'den gelen OrderCreated event'i ProductService tarafından tüketiliyor.

~~~text
OrderService
     │
     │ OrderCreated
     ▼
RabbitMQ
     │
     ▼
product-service-queue
     │
     ▼
ProductService Consumer
     │
     ▼
Stock Decrease
~~~


# 🛒 OrderService

Sipariş işlemlerinden sorumlu.

Order oluşturulduğunda Order ve OutboxMessage aynı database transaction içerisinde kaydediliyor.

~~~text
Create Order
     │
     ▼
OrderService
     │
     ├───────────────┐
     ▼               ▼
   Order        OutboxMessage
     │               │
     └───────┬───────┘
             ▼
          Commit
             │
             ▼
      OutboxPublisher
             │
             ▼
          RabbitMQ
             │
             │ order.created
             ▼
       ProductService
~~~


# 📮 Outbox Pattern

Event publish işlemlerinde Outbox Pattern kullandım.

Amaç, database işlemi başarılı olduğu halde event'in RabbitMQ'ya gönderilememesi gibi durumlarda event'in kaybolmasını önlemeye çalışmak.

Örneğin OrderService'te:

~~~text
Database Transaction
        │
        ├── Order
        │
        └── OutboxMessage
~~~

İki kayıt da başarılı olursa transaction commit ediliyor.

Daha sonra BackgroundService içerisindeki OutboxPublisher işlenmemiş OutboxMessage kayıtlarını RabbitMQ'ya gönderiyor.

~~~text
OutboxMessages
      │
      │ ProcessedAt == null
      ▼
OutboxPublisher
      │
      ▼
RabbitMQ
      │
      ▼
ProcessedAt = DateTime.UtcNow
~~~


# 🔐 Idempotency

RabbitMQ'da aynı mesajın birden fazla kez işlenebilmesi durumunu ProductService tarafında ele aldım.

İşlenen OrderId değerlerini ProcessedOrders tablosunda tutuyorum.

Ayrıca OrderId alanına unique index ekledim.

~~~text
                OrderCreated
                     │
                     ▼
              OrderId kontrolü
                     │
              Daha önce işlendi?
                 /        \
               EVET       HAYIR
                │           │
                ▼           ▼
               ACK      Stok düş
                            │
                            ▼
                     ProcessedOrder
                         kaydet
                            │
                            ▼
                         COMMIT
                            │
                            ▼
                           ACK
~~~

İlk mesaj:

~~~text
OrderId = 9999
     │
     ▼
Daha önce işlenmemiş
     │
     ▼
Stok düş
     │
     ▼
ProcessedOrders'a 9999 kaydet
     │
     ▼
ACK
~~~

Aynı mesaj tekrar geldiğinde:

~~~text
OrderId = 9999
     │
     ▼
ProcessedOrders'da mevcut
     │
     ▼
Stok düşme
     │
     ▼
ACK
~~~

Stok güncelleme ve ProcessedOrder kaydını transaction içerisinde gerçekleştirdim.


# 🔴 Redis Cache

ProductService'te ürün listesi için Redis kullandım.

Cache-Aside yaklaşımını uyguladım.

~~~text
GET Products
      │
      ▼
 Redis'te var mı?
      │
   ┌──┴──┐
   │     │
  EVET  HAYIR
   │     │
   ▼     ▼
 Redis  SQL Server
         │
         ▼
       Redis
~~~

Cache key:

~~~text
products
~~~

Ürün listesi Redis'te varsa database'e tekrar gitmeden cache'den okunuyor.


# 🔄 Redis Cache Invalidation

Stok değiştiğinde Redis'teki eski ürün bilgisinin kullanılmasını engellemek için cache'i invalidate ediyorum.

~~~text
OrderCreated
     │
     ▼
ProductService
     │
     ▼
Stock Update
     │
     ▼
Redis Cache Delete
     │
     ▼
"products"
~~~

Sonraki GET isteğinde:

~~~text
Redis'te products yok
        │
        ▼
SQL Server'dan güncel veri
        │
        ▼
Redis'e tekrar yaz
~~~


# 🚀 CQRS + MediatR

Servislerde Command ve Query işlemlerini ayırmak için CQRS yaklaşımını kullandım.

MediatR ile Command ve Query işlemlerini Handler'lara yönlendirdim.

Genel akış:

~~~text
Controller
    │
    ▼
Command / Query
    │
    ▼
MediatR
    │
    ▼
Handler
    │
    ▼
Repository
    │
    ▼
EF Core
    │
    ▼
Database
~~~

Örneğin:

~~~text
CreateOrderCommand
        │
        ▼
CreateOrderHandler
        │
        ▼
OrderRepository
        │
        ▼
OrderDb
~~~


# 🗃️ Entity Framework Core

Database işlemlerinde Entity Framework Core kullandım.

EF Core'u ORM olarak kullandım.

Kullandığım temel özellikler:

- DbContext
- DbSet
- LINQ
- Migration
- Repository Pattern
- Transaction
- Change Tracking

Örneğin:

~~~csharp
var products = await _context.Products
    .Where(x => x.Stock > 0)
    .ToListAsync();
~~~


# 🔎 LINQ

Projede sık kullanılan LINQ ifadelerini uygulamalı olarak çalıştım.

Öğrendiğim temel LINQ işlemleri:

- Where
- Select
- FirstOrDefault
- SingleOrDefault
- Any
- Count
- OrderBy
- OrderByDescending
- Include
- Join
- IQueryable
- IEnumerable
- ToList
- ToListAsync

Örneğin:

~~~csharp
var products = await _context.Products
    .Where(x => x.Stock > 0)
    .Select(x => new
    {
        x.Id,
        x.Name,
        x.Price,
        x.Stock
    })
    .ToListAsync();
~~~


# 🧠 ORM ve Micro ORM

Veritabanı erişim teknolojilerini şu şekilde ele aldım:

~~~text
Veritabanı Erişim Teknolojileri
│
├── ORM
│   └── Entity Framework Core
│
└── Micro ORM
    └── Dapper
~~~

## Entity Framework Core

ORM olarak kullandım.

C# entity'leri ile database tabloları arasında mapping sağlar.

~~~text
C# Entity
    │
    ▼
EF Core
    │
    ▼
SQL
    │
    ▼
SQL Server
~~~

## Dapper

Micro ORM yaklaşımını öğrenmek ve SQL'i daha kontrollü şekilde kullanabilmek için çalıştığım teknolojilerden biridir.

~~~text
SQL Query
    │
    ▼
Dapper
    │
    ▼
C# Object
~~~


# 🐳 Docker

RabbitMQ ve Redis gibi altyapı servislerini Docker Compose ile çalıştırıyorum.

~~~text
docker-compose.yml
       │
       ├── RabbitMQ
       │     ├── 5672
       │     └── 15672
       │
       └── Redis
             └── 6379
~~~

RabbitMQ Management UI üzerinden:

- Exchange
- Queue
- Binding
- Routing Key
- Ready
- Unacked
- Total
- Message

durumlarını kontrol ederek testler yaptım.


# 🌐 API Gateway

Servislerin dışarıya doğrudan açılması yerine YARP tabanlı API Gateway kullandım.

~~~text
Client
   │
   ▼
API Gateway
   │
   ├──────► AuthService
   │
   ├──────► CustomerService
   │
   ├──────► ProductService
   │
   └──────► OrderService
~~~

Gateway'in amacı client'ın servislerin adreslerini tek tek bilmesi yerine merkezi bir giriş noktası sağlamaktır.


# 🔐 Authentication

AuthService içerisinde JWT tabanlı authentication kullandım.

Genel akış:

~~~text
Login
  │
  ▼
AuthService
  │
  ▼
Validate User
  │
  ▼
JWT Token
  │
  ▼
Client
~~~

Korunan endpoint'lerde JWT üzerinden authentication sağlanıyor.


# ✅ Validation

Request modellerinin doğrulanması için FluentValidation kullandım.

~~~text
Request
   │
   ▼
Validation
   │
   ├── Geçersiz → Validation Error
   │
   └── Geçerli
         │
         ▼
      Handler
~~~


# ⚠️ Global Exception Handling

Uygulama içerisindeki beklenmeyen exception'ları merkezi şekilde yönetmek için Global Exception Handling yaklaşımı kullandım.

~~~text
Controller
    │
    ▼
Handler
    │
    ▼
Exception
    │
    ▼
Global Exception Handler
    │
    ▼
Standardized Error Response
~~~


# 🧪 RabbitMQ Idempotency Testi

Idempotency mekanizmasını RabbitMQ üzerinden test ettim.

Aynı OrderCreated event'ini iki kez gönderdim.

İlk mesaj:

~~~text
OrderId = 9999

Stok güncellendi.
OrderId: 9999
~~~

İkinci aynı mesaj:

~~~text
OrderId = 9999

OrderId 9999 daha önce işlendi.
~~~

İkinci mesajda stok tekrar düşmedi.

Bu test ile duplicate message senaryosunu uygulamalı olarak kontrol ettim.


# 📊 Projede Kullandığım Teknolojiler ve Yapılar

| Teknoloji / Pattern | Kullanım Amacı |
|---|---|
| .NET 9 | Backend platformu |
| ASP.NET Core Web API | REST API geliştirme |
| C# | Programlama dili |
| Microservices | Servisleri bağımsız geliştirme |
| YARP | API Gateway |
| SQL Server | Kalıcı veri saklama |
| Entity Framework Core | ORM / Database işlemleri |
| LINQ | Veri sorgulama |
| Dapper | Micro ORM yaklaşımını öğrenme |
| MediatR | Command / Query dispatch |
| CQRS | Command ve Query ayrımı |
| Repository Pattern | Data access abstraction |
| RabbitMQ | Asynchronous communication |
| Topic Exchange | Event routing |
| Outbox Pattern | Reliable event publishing |
| BackgroundService | Background işlemler |
| Redis | Cache |
| Cache-Aside | Cache stratejisi |
| Cache Invalidation | Güncel cache yönetimi |
| Idempotency | Duplicate message kontrolü |
| Docker | Altyapı servislerini container olarak çalıştırma |
| JWT | Authentication |
| FluentValidation | Request validation |
| Swagger | API test / dokümantasyon |


# 🎯 Projede Öğrendiklerim

Bu proje üzerinde çalışırken sadece CRUD geliştirmek yerine gerçek backend sistemlerinde karşılaşılabilecek problemleri uygulamalı olarak çalıştım.

- Microservice architecture
- Database-per-service
- CQRS
- MediatR
- Repository Pattern
- Dependency Injection
- EF Core
- LINQ
- SQL
- RabbitMQ
- Event-driven architecture
- Outbox Pattern
- BackgroundService
- Redis
- Cache-Aside
- Cache Invalidation
- Idempotency
- Docker
- API Gateway
- JWT Authentication
- Global Exception Handling
- FluentValidation

