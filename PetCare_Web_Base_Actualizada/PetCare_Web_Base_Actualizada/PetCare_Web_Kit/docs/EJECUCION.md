# Cómo ejecutar PetCare Web (Windows + Visual Studio)

## 1. Requisitos
- Windows 10/11.
- Visual Studio 2026 con la carga **ASP.NET y desarrollo web** y soporte para .NET 10, o .NET 10 SDK instalado.
- SQL Server LocalDB (se instala normalmente con Visual Studio) o SQL Server Express.
- Conexión a internet la primera vez para restaurar paquetes NuGet.

## 2. Extraer y abrir
1. Descargue y extraiga el ZIP en una carpeta sencilla, por ejemplo `C:\Proyectos\PetCare_Web`.
2. Entre en la carpeta extraída y abra `PetCare.Web.sln` con Visual Studio.
3. Espere a que termine la restauración de paquetes NuGet.
4. En el Explorador de soluciones, haga clic derecho en `PetCare.Web` y elija **Establecer como proyecto de inicio**.
5. En el menú **Compilar**, seleccione **Compilar solución**.

## 3. Configurar la base de datos
La configuración de desarrollo está en `src/PetCare.Web/appsettings.Development.json`. Por defecto apunta a LocalDB:

`Server=(localdb)\MSSQLLocalDB;Database=PetCareDb;Trusted_Connection=True;TrustServerCertificate=True;`

Si usa SQL Server Express, cambie el servidor por `Server=.\SQLEXPRESS;`. No publique credenciales reales ni contraseñas en Git.

## 4. Crear la base (cuando el modelo esté revisado)
Abra **Herramientas > Administrador de paquetes NuGet > Consola del Administrador de paquetes**. Seleccione `PetCare.Infrastructure` como proyecto predeterminado y ejecute:

```powershell
Add-Migration InitialCreate -Project PetCare.Infrastructure -StartupProject PetCare.Web -Context PetCareDbContext
Update-Database -Project PetCare.Infrastructure -StartupProject PetCare.Web -Context PetCareDbContext
```

Alternativa desde una terminal ubicada en la raíz del ZIP:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.0
dotnet ef migrations add InitialCreate --project src/PetCare.Infrastructure --startup-project src/PetCare.Web --context PetCareDbContext --output-dir Migrations
dotnet ef database update --project src/PetCare.Infrastructure --startup-project src/PetCare.Web --context PetCareDbContext
```

**Importante:** antes de crear la migración definitiva, coteje las entidades y relaciones con el UML y el diagrama SQL exigidos por el profesor. La configuración incluida es inicial.

## 5. Ejecutar la aplicación
1. Seleccione el perfil `https` o `http` de `PetCare.Web` en la barra superior de Visual Studio.
2. Presione **F5** (depuración) o **Ctrl+F5** (sin depuración).
3. Se abrirá el navegador en la página inicial de PetCare Web.

También puede ejecutar desde una terminal en la raíz:

```powershell
dotnet run --project src/PetCare.Web
```

Copie en el navegador la dirección local que muestre la terminal (por ejemplo, `https://localhost:7xxx`).

## 6. Si aparece un error
- **No se encuentra .NET 10 / NETSDK...**: instale .NET 10 SDK y actualice Visual Studio con el soporte correspondiente.
- **No se puede conectar a SQL Server**: compruebe que LocalDB/SQL Server esté instalado y que el servidor de la cadena de conexión coincida con su instancia. La página inicial no consulta todavía la base; la conexión se usará al implementar consultas y migraciones.
- **No se restaura NuGet**: revise internet y vuelva a hacer clic derecho en la solución > Restaurar paquetes NuGet.
- **No aparece la página**: compruebe que `PetCare.Web` sea el proyecto de inicio y que está ejecutando el perfil correcto.

## Alcance de esta entrega
La página inicial de Blazor ya está estructurada para el modelo moderno de componentes interactivos. Los módulos son una presentación del alcance, no formularios CRUD terminados. Aún faltan persistencia integrada en pantallas, autenticación/autorización real, pruebas y validación final contra los documentos del curso.
