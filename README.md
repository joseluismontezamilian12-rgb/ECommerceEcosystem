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

> **Visual Studio alternative:** run `Update-Database -Project Catalog.API -StartupProject Catalog.API` in the Package Manager Console and set both APIs as startup projects.

---

## 📄 License

MIT — see [LICENSE](LICENSE).
