# Arquitectura PetCare

## Domain
Contiene reglas de negocio y entidades. No referencia EF Core ni SQL Server.

## Infrastructure
Implementa persistencia: `PetCareDbContext`, mapeos y migraciones.

## Web
Es el punto de entrada ASP.NET Core/Blazor. Configura la conexión e inyección de dependencias.

## Flujo previsto
Componente Blazor -> servicio de aplicación -> `IDbContextFactory<PetCareDbContext>` -> SQL Server.

No inyectar un `DbContext` de larga vida en componentes interactivos. Crear un contexto por operación mediante la fábrica.

## Modelo relacional inicial
El contexto utiliza jerarquía TPH para servicios y productos; Factura tiene UsuarioEmisor y colección de líneas. Cada línea tiene ServicioId o ProductoId, nunca ambos.
