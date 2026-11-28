# Verificación de PetCare Web

## Estado de la entrega

Se integraron los módulos funcionales pendientes sobre el ZIP de GitHub suministrado, preservando la arquitectura y el historial de migraciones. Se añadió registro de cuentas con elección de rol, registro libre para ambos roles y validación del servidor (solo para demostración local). La facturación es un comprobante académico interno; no incluye facturación electrónica DIAN, impuestos ni pagos. La zona horaria utilizada por los cálculos es la del servidor. No se incluyó el UML definitivo ni el documento de ocho pruebas del profesor; la comparación formal y las reflexiones individuales siguen pendientes.

**Evidencia disponible:** revisión de los archivos, rutas y migraciones; comprobación sintáctica del JavaScript y pruebas de vistas previas con Node.js. No se ejecutó el compilador de .NET ni SQL Server, por ausencia de SDK/LocalDB y falta de conectividad para descargarlos. No se deben dar por aprobadas las pruebas de dominio, base de datos, concurrencia o navegador hasta ejecutarlas en Windows.

## Comandos de verificación (raíz de la solución)

```powershell
dotnet restore PetCare.Web.sln
dotnet build PetCare.Web.sln
dotnet run --project tests/PetCare.Domain.Checks/PetCare.Domain.Checks.csproj
# Opcional, si tienes Node.js:
node tests/preview-checks.cjs
```

Después aplica `Update-Database` desde Visual Studio (ver README). Los tests de dominio no necesitan SQL Server; la suite de interfaz/integración se ejecuta a continuación.

## Pruebas manuales funcionales y de seguridad

| N° | Prueba | Resultado esperado |
|---:|---|---|
| 1 | Registro recepcionista con contraseña de 12 caracteres o más | Cuenta creada; iniciar sesión con ella. |
| 2 | Registro con nombre repetido o contraseñas diferentes | Rechazo, sin duplicar usuarios. |
| 3 | Registro administrador con nombre único y contraseña válida, sin código | Cuenta creada; iniciar sesión y verificar acceso a Usuarios. |
| 4 | Enviar un rol no permitido al registro (por ejemplo, Supervisor) | Rechazo; no se crea la cuenta. |
| 5 | Recepcionista abre `/usuarios` o intenta POST de gestión de usuarios | Acceso denegado. |
| 6 | Administrador edita un usuario, intenta eliminar el último administrador | Edición válida; eliminación del último rechazada. |
| 7 | Usuario no autenticado abre `/hospitalizaciones`, `/farmacia` o `/facturas` | Redirección a login; sin lectura ni escritura. |
| 8 | Crear cita `CIT-PRUEBA` con 60 min, confirmar veterinario | Costo 105.000 COP. |
| 9 | Crear hospitalización `HOS-PRUEBA`, hace 2 días, tarifa 80.000 | 3 días, 240.000 COP si se consulta el mismo día. |
| 10 | Alta anterior al ingreso, alta futura o ingreso futuro | Rechazo. |
| 11 | Registrar alta válida hoy, luego intentar editar estancia | Costo fijo; edición rechazada. |
| 12 | Crear lote `MED-PRUEBA`, precio 15.000, stock 10, vence en 30 días | Aparece en inventario y catálogo facturable. |
| 13 | Crear lote vencido y buscarlo en farmacia | Se muestra vencido; no aparece como facturable. |
| 14 | Emitir `FAC-PRUEBA` con cita, hospitalización y 2 medicamentos | Total 375.000 COP (si las fechas coinciden con pasos 8–11); stock 8. |
| 15 | Intentar volver a facturar un servicio de factura vigente | Rechazo. |
| 16 | Intentar vender 11 unidades del lote o un lote vencido | Rechazo aun si se manipula el formulario. |
| 17 | Intentar editar o eliminar servicio/lote asociado a una factura | Rechazo; se mantiene historial. |
| 18 | Recepcionista intenta anular factura | Acceso denegado. |
| 19 | Administrador anula `FAC-PRUEBA` | Estado anulada; stock vuelve a 10; total histórico se conserva. |
| 20 | Intentar anular de nuevo | Rechazo; stock no aumenta. |
| 21 | Editar o eliminar una cuenta desde otra sesión y probar operación con cookie anterior | Rechazo/revalidación de la sesión. |
| 22 | Enviar POST sin token CSRF | HTTP 400; sin cambios en SQL Server. |
| 23 | Ver detalle e imprimir factura | Comprobante con líneas, precio y total. |
| 24 | Ejecutar simultáneamente ventas que en conjunto superan stock | No debe persistir saldo negativo; una operación puede rechazarse por concurrencia. |
| 25 | Abrir y enviar formularios incluso con SignalR no disponible | Los envíos HTML van a `/operaciones/...`, no a la página Razor. |

**Todas las pruebas manuales están pendientes de ejecución en tu equipo.** Los códigos de ejemplo deben ser únicos; usa una base de pruebas o códigos distintos si ya existen. No borres datos reales para ejecutar esta lista.

## Consultas de verificación en SSMS

```sql
USE PetCareDb;
SELECT MigrationId, ProductVersion FROM dbo.__EFMigrationsHistory ORDER BY MigrationId;
SELECT Codigo, NombreMascota, Fecha, FechaAlta, TarifaDiaria FROM dbo.Servicios WHERE Tipo='Hospitalizacion';
SELECT Codigo, Nombre, PrecioUnidad, FechaVencimiento, Existencias FROM dbo.Productos WHERE Tipo='LoteMedicamento';
SELECT f.Numero, f.NombreCliente, f.EstaAnulada, f.InventarioDescontado,
       SUM(l.Cantidad * l.PrecioUnitario) AS Total
FROM dbo.Facturas f LEFT JOIN dbo.LineasFactura l ON l.FacturaId=f.Id
GROUP BY f.Numero,f.NombreCliente,f.EstaAnulada,f.InventarioDescontado;
SELECT NombreUsuario, NombreCompleto, Rol, LEN(ContrasenaHash) AS LongitudHash FROM dbo.Usuarios;
```

## Registro para GitHub

Después de probar, copia los cambios de esta entrega a tu repositorio local con GitHub Desktop (conserva `.git` de tu repositorio; el ZIP no contiene el historial). No publiques `bin`, `obj`, `.vs`, ni secretos. Commit sugerido: `Completar módulos de PetCare y registro libre de usuarios`. No se realizó ningún push desde el entorno de preparación.

## Pruebas de la actualización DatosMascota (ejecutar en Windows)

1. Respaldar PetCareDb y aplicar Update-Database. Confirmar que las citas y hospitalizaciones previas continúan visibles con tipo «Sin registrar».
2. Crear cita e ingreso con Perro y Cocker Spaniel. Verificar los datos en el listado y volver a abrir el registro.
3. Intentar crear un perro sin raza: el formulario debe bloquear el envío y el servidor rechazar solicitudes manipuladas.
4. Crear una cita e ingreso con Gato: el campo raza no debe exigirse ni persistirse.
5. Editar una cita o estancia activa no facturada de Perro a Gato: verificar que se limpia la raza en SQL Server.
6. Buscar por «Cocker» o «Gato»; verificar las restricciones originales de alta y facturación.
7. Abrir Home, login y registro en escritorio y móvil; comprobar que el cocker se muestra solo en Home.

```sql
SELECT Codigo, NombreMascota, TipoMascota, RazaMascota, Tipo FROM dbo.Servicios ORDER BY Id DESC;
```
