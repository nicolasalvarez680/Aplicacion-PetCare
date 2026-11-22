# PetCare Web - base de trabajo

Solución organizada en tres proyectos para el trabajo académico PetCare Web:

- `PetCare.Domain`: entidades, interfaz `IFacturable` y enumeración de roles.
- `PetCare.Infrastructure`: contexto de Entity Framework Core y proveedor SQL Server.
- `PetCare.Web`: aplicación ASP.NET Core con Blazor Web App y componentes interactivos del lado del servidor.

## Requisitos
- Visual Studio 2026 actualizado con la carga **ASP.NET y desarrollo web** y soporte .NET 10, o .NET 10 SDK.
- SQL Server LocalDB o SQL Server Express para crear y usar la base de datos.

## Inicio rápido
1. Extraiga el ZIP.
2. Abra `PetCare.Web.sln`.
3. Establezca `PetCare.Web` como proyecto de inicio.
4. Revise `src/PetCare.Web/appsettings.Development.json` para confirmar el servidor SQL local.
5. Compile la solución y ejecute con F5.
6. Para crear la base, primero coteje el modelo con el UML y el esquema SQL del profesor; después siga `docs/EJECUCION.md` para crear/aplicar la migración.

## Estado y límites
Esta versión corrige el arranque de la capa web usando el modelo actual de Blazor Web App y añade una página de inicio responsive que presenta los módulos del dominio. Mantiene las entidades y el mapeo inicial que venían en el ZIP recibido.

No es todavía un sistema clínico terminado: los formularios CRUD, consultas conectadas a la base, autenticación, autorización por roles, pruebas automatizadas y todas las reglas de negocio pendientes no están implementados. El esquema EF Core es preliminar y debe verificarse contra los documentos del profesor. El ZIP recibido no contenía el UML ni el diagrama físico SQL, por lo que no se modificaron las entidades para adivinar esos requisitos.

## Estructura
- `src/`: proyectos de la solución.
- `docs/`: arquitectura, ejecución, pendientes y uso de GitHub.
- `scripts/`: apoyo para migraciones.

Consulte `docs/EJECUCION.md` para los pasos detallados en Windows/Visual Studio.
