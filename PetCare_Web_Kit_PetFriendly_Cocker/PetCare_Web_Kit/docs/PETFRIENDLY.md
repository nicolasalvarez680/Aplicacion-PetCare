# PetCare Web — PetFriendly 2.0

## Cambios de navegación

| Ruta | Acceso | Descripción |
|---|---|---|
| `/` | Público | Portada informativa y enlaces a módulos |
| `/login` | Público | Autenticación por cookies |
| `/registro` | Público | Registro libre de Administrador o Recepcionista |
| `/dashboard` | PersonalAutorizado | Panel privado, datos de SQL Server |
| `/citas`, `/hospitalizaciones`, `/farmacia`, `/facturas` | PersonalAutorizado | Módulos existentes |
| `/usuarios` | SoloAdministrador | Administración de usuarios |

La portada no consulta datos privados. Después del login se redirige a `/dashboard`. Los formularios de autenticación conservan los endpoints explícitos y antifalsificación. No se volvió a usar EditForm para guardar datos. El nuevo checkbox de recordar sesión activa la persistencia de la cookie existente, con la política original de expiración y renovación. No guarda la contraseña en el navegador por código propio. Los botones de visibilidad de contraseña son locales. Se conservan los servicios, entidades y migraciones de la entrega Registro Libre.

## Recursos visuales

El logotipo y las imágenes WebP se obtuvieron de las propuestas visuales aprobadas. El texto, navegación, campos y botones son componentes/controles HTML reales; no se incrustaron capturas completas de las páginas. El logo también se utiliza como favicon. No se necesita descargar imágenes ni fuentes al ejecutar el proyecto.

## Verificación efectuada

- Comparación con el ZIP Registro Libre: capas Domain e Infrastructure sin cambios; rutas POST y campos de los formularios existentes conservados.
- Node.js: cinco comprobaciones de vistas previas, además de comprobación sintáctica del JavaScript.
- Chromium: reproducción HTML estática de los tres diseños públicos a 1440, 768 y 390 píxeles; se comprobó el ancho, las imágenes y los controles de visibilidad de contraseña y selección de rol. No fue una ejecución del servidor .NET ni una prueba de envío a SQL Server.
- Revisar `VERIFICACION_REALIZADA.txt` y `PRUEBAS_Y_ENTREGA.md` para las verificaciones previas y la lista de pruebas de integración.

## Pruebas pendientes en Windows

1. Recompilar y abrir la aplicación con el perfil HTTPS.
2. Abrir `/` sin iniciar sesión y navegar a servicios, registro y login.
3. Registrar ambos roles y comprobar que el login lleva a `/dashboard`.
4. Probar visibilidad de la contraseña, sesión normal y sesión recordada; cerrar sesión y verificar que la cookie se elimina.
5. Probar que las rutas privadas requieren autenticación y que Recepcionista no accede a Usuarios.
6. Registrar una cita, hospitalización, lote y factura; probar alta, reposición, anulación y descarga PDF. Consultar los datos en SSMS.
7. Revisar diseño y navegación en móvil y escritorio. Si ya aplicaste la migración de inventario, no hay migración nueva.

**Limitación:** no hay SDK .NET 10 ni SQL Server LocalDB en el entorno de preparación. No se ha certificado la compilación ni la ejecución integral. El registro administrativo libre es una decisión solicitada para la demostración académica; no publicar con datos reales.

## Actualización Cocker Spaniel y mascotas

Esta versión sustituye la imagen del Home por `branding/cocker-portada.webp`, obtenida del retrato aportado (sin regenerar al perro). `branding/wave.svg` y las nuevas reglas de CSS integran el fondo. Login y Registro mantienen los mismos recursos originales. La actualización de los formularios requiere aplicar la migración DatosMascota; consulta README.md.
