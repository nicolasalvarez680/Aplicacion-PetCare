# Cambios realizados en esta revisión

1. Se conservó la solución y los tres proyectos del ZIP recibido.
2. Se reemplazó el patrón antiguo de hosting (`AddServerSideBlazor`, `MapBlazorHub`, `/_Host`) por el patrón de Blazor Web App: `AddRazorComponents().AddInteractiveServerComponents()` y `MapRazorComponents<App>().AddInteractiveServerRenderMode()`.
3. Se añadieron `App.razor`, `Routes.razor`, `_Imports.razor`, `MainLayout.razor`, una página inicial y estilos adaptables a móvil.
4. Se actualizó la guía de ejecución y se aclaró qué partes siguen pendientes.

## Verificación
El entorno de preparación de este archivo no dispone del comando `dotnet`, así que no fue posible ejecutar `dotnet restore` ni `dotnet build` aquí. El proyecto debe compilarse en un equipo con .NET 10 SDK/Visual Studio actualizado. Si Visual Studio muestra errores, compartir captura de la lista de errores y se corrigen sobre esta misma base.

## Pendiente de documentos del curso
El ZIP recibido no incluye el UML ni el diagrama físico de la base de datos. Por eso se preservaron las entidades originales y se señala el mapeo como preliminar; no se afirma que ya cumpla todos los requisitos del profesor.
