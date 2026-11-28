# PetCare Web — PetFriendly 2.0

Aplicación académica de gestión veterinaria con .NET 10, Blazor Web App, Entity Framework Core y SQL Server LocalDB. Conserva los tres proyectos Domain, Infrastructure y Web, así como las migraciones y los datos existentes.

## Puesta en marcha en Windows

1. Guarda una copia de tu proyecto anterior y realiza una copia de seguridad de `PetCareDb` en SSMS antes de aplicar migraciones. Extrae el ZIP en una carpeta nueva. No copies `.vs`, `bin` ni `obj` de otra versión.
2. Abre `PetCare.Web.sln` en Visual Studio compatible con .NET 10. Selecciona `PetCare.Web` como proyecto de inicio. Restaura NuGet y recompila.
3. En Visual Studio, **Herramientas > Administrador de paquetes NuGet > Consola del Administrador de paquetes**, ejecuta:

   ```powershell
   Update-Database -Project PetCare.Infrastructure -StartupProject PetCare.Web -Context PetCareDbContext
   ```

   Se aplicará `DatosMascota` y, si todavía está pendiente, `InventarioLotes`. En una base nueva también se aplicará `InitialCreate`. No ejecutes `Add-Migration InitialCreate` nuevamente. La migración añade `Productos.Existencias` y `Facturas.InventarioDescontado`; los lotes existentes comienzan con cero unidades para que el responsable ingrese el inventario real. Las facturas antiguas no devuelven unidades que nunca se descontaron.
4. Presiona F5, usa el perfil HTTPS. El puerto del ZIP es 60219. La conexión de desarrollo utiliza `(localdb)\MSSQLLocalDB`, base `PetCareDb`. Si tu instancia es diferente, ajusta `appsettings.Development.json`.
5. Abre `https://localhost:60219/` para ver la nueva portada pública. Accede con tus usuarios existentes en `/login` o crea una cuenta en `/registro`. El inicio de sesión exitoso lleva a `/dashboard`. No se incluye ningún usuario ni contraseña predeterminados.

## Nuevo diseño PetFriendly

- Portada pública `/` con módulos, navegación y acceso a registro e inicio de sesión.
- Inicio de sesión `/login` con fotografía de mascota, opción de mostrar contraseña y opción real de recordar la sesión (cookie persistente con la configuración de duración existente).
- Registro `/registro` con los campos originales, ambos roles libres y fotografías de perro y gato.
- Panel privado `/dashboard` y módulos internos con nueva paleta turquesa, menta y amarillo, tarjetas y logotipo. La autorización del panel no se ha eliminado: se trasladó a su nueva ruta.
- Recursos visuales locales en `wwwroot/branding`, sin dependencias de imágenes o fuentes externas. CSS adicional `wwwroot/petfriendly.css`.
- Esta versión añade la migración `20261126190000_DatosMascota`, necesaria para guardar el tipo y la raza de las mascotas. Ejecuta `Update-Database` (paso 3), aunque ya hayas aplicado `InventarioLotes`. No generes otra migración con el mismo propósito.

El registro mantiene únicamente los datos que la base actual almacena; no se añadieron campos decorativos de correo, términos o contacto que no tengan una implementación real. Para seguridad, deja desmarcada la opción Recordar mi sesión en equipos compartidos.

## Actualización: Cocker Spaniel y datos de mascotas

- Solo la portada pública `/` muestra el cocker cobrizo del retrato suministrado, sin el gato. El fondo usa formas continuas y una transición ondulada. Inicio de sesión y registro conservan sus imágenes originales, con bordes suavizados; el panel privado `/dashboard` permanece intacto.
- Los formularios de citas y hospitalizaciones (creación y edición) requieren elegir Perro o Gato. Si es perro, se exige la raza: se ofrecen sugerencias, pero se admite texto libre, incluyendo Mestizo o No identificada. Para gato no se almacena raza. Las listas y detalles muestran ambos datos y admiten búsquedas por tipo y raza.
- Los nuevos campos están en `ServicioVeterinario`, compartido por ambos módulos. El servidor valida los datos incluso si se omite JavaScript.
- La migración añade `Servicios.TipoMascota` (nvarchar(10)) y `Servicios.RazaMascota` (nvarchar(80)), ambos anulables para preservar los registros históricos. Los registros anteriores aparecen como «Sin registrar». Al editar un registro permitido se solicita la información; los registros facturados y las hospitalizaciones finalizadas mantienen sus restricciones originales.
- No se modifica el registro libre ni las reglas de facturación. Realiza una copia de seguridad de la base antes de ejecutar `Update-Database`.
- Verificación: cinco pruebas de costos y tres de controles de tipo/raza pasaron en Node.js. La portada se comprobó con una reproducción HTML y los recursos reales a 1440, 768 y 390 píxeles; los controles condicionales se comprobaron en Chromium. No se ejecutó el servidor .NET ni SQL Server.

## Registro libre de cuentas

En `/registro`, cualquier visitante puede elegir Administrador o Recepcionista y crear su cuenta sin código de invitación ni configuración adicional. Se mantienen la validación de roles, la confirmación de contraseña, el hash, el control de duplicados, la protección antifalsificación y la limitación de solicitudes. Después del registro, inicia sesión con las credenciales creadas.

**Advertencia:** esta modalidad se destina a una demostración académica local con datos ficticios. Cualquier visitante puede obtener permisos administrativos completos. No expongas la aplicación a Internet ni la uses con datos reales sin restringir el alta de cuentas a personal autorizado. Las cuentas anteriores se conservan. Si ya configuraste un código de invitación, esta versión no lo utiliza. No se requiere una migración adicional por este cambio.

## Funciones

- Usuarios: creación, consulta, edición y eliminación, protección del último administrador y de usuarios asociados a facturas.
- Registro: alta pública, contraseña hash, validación de roles, protección CSRF, limitación de solicitudes; elección libre de ambos roles para uso académico local.
- Citas: búsqueda, filtros, creación, edición, confirmación, costo polimórfico y eliminación de registros sin facturas.
- Hospitalizaciones: ingreso, edición de estancias activas, días inclusivos, tarifa diaria, alta, búsqueda y protección de registros facturados.
- Farmacia: lotes, vencimiento, precio, existencias, reposición y bloqueo de venta vencida o sin stock.
- Facturación: líneas de servicios y medicamentos, precios obtenidos del servidor, descuento transaccional de stock, consulta, impresión, anulación exclusiva de administrador y devolución de existencias una sola vez.
- Panel principal: indicadores y enlaces a todos los módulos. Administradores y recepcionistas acceden a los módulos de clínica, farmacia y facturación. Solo administradores gestionan usuarios y anulan facturas.

## Cambio técnico: formularios

Se mantuvo Blazor y la estructura de capas. Los formularios de los módulos ahora usan HTML renderizado en el servidor, rutas POST explícitas `/operaciones/...` o `/auth/...`, token antifalsificación, autorización de servidor y redirección después del POST. No dependen de `EditForm`, `@onclick` ni de una conexión SignalR para guardar. Las búsquedas se ejecutan al pulsar Buscar; las vistas previas de costos usan JavaScript local y son solo informativas. Los importes definitivos se calculan en C# en el servidor. Los recursos se registran con `MapStaticAssets`.

## Verificación

Consulta `docs/PRUEBAS_Y_ENTREGA.md` para la lista completa de pruebas y las limitaciones. Se realizó revisión estática y pruebas de JavaScript, pero el entorno de preparación no tiene .NET SDK ni SQL Server LocalDB y no puede descargar dependencias. La compilación y las pruebas de integración en Windows quedan pendientes. No se garantiza que la solución compile sin ajustes hasta realizar esas pruebas.

Para comprobar el diseño, consulta `docs/PETFRIENDLY.md`.
