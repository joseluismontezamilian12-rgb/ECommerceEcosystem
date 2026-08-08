# 🚀 ECommerceEcosystem — Distributed .NET Microservices

Decoupled e-commerce backend built as **.NET microservices** with synchronous inter-service communication and **polyglot persistence** (relational + NoSQL).

---

## 🏗️ Architecture

- **`Catalog.API`** — product catalog service. SQL Server storage mapped with **Entity Framework Core**.
- **`Basket.API`** — high-speed shopping-cart service backed by **Redis** (key-value store, millisecond reads/writes).

```
[Client / Swagger] ──(HTTP POST)──> [Basket.API]
                                        │
                          (HTTP GET /api/catalog/{id})
                                        ▼
                                  [Catalog.API] ──> [SQL Server]
                                        │
                        (returns immutable record with the real price)
                                        ▼
[Basket.API] ──(maps via DTO, stores on port 6379)──> [Redis]
```

---

## 🛠️ Engineering Decisions

1. **Redis for volatile operations** — cart add/remove traffic never hits the relational database; baskets are serialized as JSON under a `userName → ShoppingCart` key-value flow.
2. **Server-side price verification** — `Basket.API` never trusts client-submitted prices: on every cart operation it calls `Catalog.API` through `HttpClient` and reads the real price from SQL Server, neutralizing client-side price tampering.
3. **Immutable records + DTOs** — the product domain is modeled with positional `record` types; an internal `CatalogProductDto` translates contract mismatches between services (`Name` vs `ProductName`) without coupling them.
4. **Automated data seeding** — EF Core migrations populate the catalog with test data on first run.

---

## 🧰 Stack

C# / .NET · ASP.NET Core Minimal APIs · Entity Framework Core · SQL Server · Redis (port `6379`) · Swagger / OpenAPI

---

## 🌐 Live Demo

Both services run on Azure App Service, with Azure SQL for the catalog and a
managed Redis instance (São Paulo) for the baskets. Swagger UI is served at the
root of each service, so you can exercise the whole flow from a browser:

| Service | URL |
|---|---|
| **Catalog.API** | https://ecommerce-catalog-lnxj7c.azurewebsites.net |
| **Basket.API** | https://ecommerce-basket-lnxj7c.azurewebsites.net |

Try the price-tampering defence yourself — post a basket claiming the laptop
costs `1.00`:

```bash
curl -X POST https://ecommerce-basket-lnxj7c.azurewebsites.net/api/basket \
  -H "Content-Type: application/json" \
  -d '{"userName":"demo","items":[{"productId":"prod-001","price":1.00,"quantity":2}]}'
```

The response comes back at `1200.00` per unit. `Basket.API` refetched the real
price from `Catalog.API` and overwrote what the client sent.

> The database auto-pauses when idle to stay within the free tier, so the very
> first request after a while may take up to a minute (or return `503`). The
> second one is fast.

---

## 🚀 Getting Started

### Prerequisites

- .NET SDK
- SQL Server instance
- Redis running locally on the default port `6379`

### Steps

```bash
git clone https://github.com/joseluismontezamilian12-rgb/ECommerceEcosystem.git
cd ECommerceEcosystem

# apply migrations + seed the catalog
dotnet ef database update --project Catalog.API --startup-project Catalog.API

# run both services (separate terminals)
dotnet run --project Catalog.API
dotnet run --project Basket.API
```

Open each service's Swagger UI to interact with the APIs.

### Configuration

Nothing environment-specific is hardcoded — every deployment target is a
configuration key, so the same build runs locally and in the cloud:

| Key | Purpose | Local default |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | Catalog database | LocalDB |
| `CacheSettings:ConnectionString` | Redis for baskets | `localhost:6379` |
| `Services:CatalogUrl` | Where `Basket.API` reaches the catalog | `https://localhost:44366` |
| `Database:AutoMigrate` | Apply migrations on startup | `false` |

`Database:AutoMigrate` stays off by default on purpose: a deployment should
never alter a database unless it was explicitly asked to.

> **Visual Studio alternative:** run `Update-Database -Project Catalog.API -StartupProject Catalog.API` in the Package Manager Console and set both APIs as startup projects.

---

## 📄 License

MIT — see [LICENSE](LICENSE).
