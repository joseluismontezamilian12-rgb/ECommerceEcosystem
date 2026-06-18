# ECommerceEcosystem 🚀

Ecosistema distribuido y desacoplado de Comercio Electrónico diseñado bajo una arquitectura de **Microservicios** con **.NET**, aplicando comunicación síncrona inter-servicio y persistencia políglota (relacional y NoSQL).

## 🏗️ Arquitectura del Sistema

El ecosistema está fragmentado en componentes autónomos que interactúan eficientemente para garantizar la consistencia y la velocidad del negocio:

* **`Catalog.API`:** Microservicio encargado de la gestión del catálogo de productos. Utiliza **SQL Server** como almacenamiento relacional y mapeo quirúrgico de datos mediante **Entity Framework Core**.
* **`Basket.API`:** Microservicio enfocado en la gestión del carrito de compras de alta velocidad. Implementa un repositorio NoSQL clave-valor utilizando **Redis** para asegurar lecturas y escrituras en milisegundos.

[Cliente/Swagger] ───(HTTP POST)───> [Basket.API]
│
(HTTP GET /api/catalog/{id})
│
▼
[Catalog.API] ───> [SQL Server]
│
(Devuelve Record inmutable con Precio Real)
│
▼
[Basket.API] ───(Mapea vía DTO & Guarda en Puerto 6379)───> [Redis NoSQL]

## 🛠️ Decisiones de Ingeniería y Patrones Aplicados

### 1. Persistencia NoSQL con Redis
Para evitar saturar la base de datos relacional con operaciones volátiles (agregar/quitar productos del carrito), se delegó el almacenamiento de `Basket.API` a una instancia distribuida de **Redis Server**. Los datos se serializan y estructuran dinámicamente como documentos JSON eficientes bajo el flujo clave-valor (`userName` -> `ShoppingCart`).

### 2. Comunicación Síncrona Segura (HttpClient)
El microservicio de carrito no confía en los precios ni en la información que envía el cliente desde el frontend. Cada vez que se procesa o actualiza un carrito:
* `Basket.API` invoca internamente a `Catalog.API` mediante un cliente HTTP optimizado (`HttpClient`).
* Se extrae el precio real directamente desde SQL Server, neutralizando cualquier intento de alteración maliciosa de costos.

### 3. Inmutabilidad de Datos y Patrón DTO (Data Transfer Object)
* **Records Posicionales:** En `Catalog.API`, el dominio de productos se modeló utilizando `public record Product`, garantizando contratos de datos inmutables y seguros.
* **Desacoplamiento con DTOs:** Para resolver el *mismatch* de propiedades entre servicios (donde el catálogo expone `Name` y el carrito requiere `ProductName`), se implementó un `CatalogProductDto` interno en la capa de consumo para traducir quirúrgicamente los contratos de las APIs sin romper la compilación.

### 4. Inicialización Automática de Base de Datos (Data Seeding)
Se configuró el pipeline de Entity Framework Core para inyectar de manera automatizada datos semilla reales en SQL Server a través de migraciones dirigidas, poblando el catálogo con hardware de prueba listo para producción desde el primer arranque.

## 🧰 Tecnologías Utilizadas

* **Lenguaje:** C# (.NET)
* **Frameworks:** ASP.NET Core Web API (Minimal APIs)
* **ORMs & BD Relacional:** Entity Framework Core & SQL Server
* **BD NoSQL:** Redis Distributed Cache (Puerto `6379`)
* **Documentación:** Swagger / OpenAPI
* **Control de Versiones:** Git & GitHub

## 🚀 Cómo Ejecutar el Proyecto

### Requisitos Previos
* SDK de .NET instalado.
* Instancia de SQL Server activa.
* Servidor Redis corriendo localmente en el puerto predeterminado `6379`.

### Pasos
1. **Clonar el repositorio:**
   ```bash
   git clone [https://github.com/joseluismontezamilian12-rgb/ECommerceEcosystem.git](https://github.com/joseluismontezamilian12-rgb/ECommerceEcosystem.git)
Aplicar las migraciones e inyectar los datos semilla:
Abre la Consola del Administrador de Paquetes en Visual Studio y ejecuta:

Plaintext
Update-Database -Project Catalog.API -StartupProject Catalog.API
Configurar múltiples proyectos de inicio:
En las propiedades de la solución en Visual Studio, establece tanto a Catalog.API como a Basket.API con la acción Iniciar (Start).

Ejecutar:
Presiona F5 o el botón Play. Se abrirán los dashboards de Swagger de ambos microservicios listos para interactuar.
