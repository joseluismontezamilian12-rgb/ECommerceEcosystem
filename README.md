# ECommerceEcosystem 🚀

Ecosistema distribuido y desacoplado de Comercio Electrónico diseñado bajo una arquitectura de **Microservicios** con **.NET**, aplicando comunicación síncrona inter-servicio y persistencia políglota (relacional y NoSQL).

## 🏗️ Arquitectura del Sistema

El ecosistema está fragmentado en componentes autónomos que interactúan eficientemente para garantizar la consistencia y la velocidad del negocio:

* **`Catalog.API`:** Microservicio encargado de la gestión del catálogo de productos. Utiliza **SQL Server** como almacenamiento relacional y mapeo quirúrgico de datos mediante **Entity Framework Core**.
* **`Basket.API`:** Microservicio enfocado en la gestión del carrito de compras de alta velocidad. Implementa un repositorio NoSQL clave-valor utilizando **Redis** para asegurar lecturas y escrituras en milisegundos.
