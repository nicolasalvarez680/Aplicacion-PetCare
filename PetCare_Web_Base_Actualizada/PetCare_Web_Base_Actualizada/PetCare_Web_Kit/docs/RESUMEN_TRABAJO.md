# Resumen del trabajo PetCare Web

## Objetivo
Construir una aplicación web para una clínica veterinaria con C#, Blazor, .NET 10 y SQL Server, separando el dominio, la persistencia y la interfaz.

## Arquitectura acordada
- `PetCare.Domain`: reglas de negocio, entidades, interfaces y enumeraciones. No debe depender de Entity Framework ni de SQL Server.
- `PetCare.Infrastructure`: persistencia, `PetCareDbContext`, configuración de EF Core y migraciones.
- `PetCare.Web`: aplicación Blazor, configuración de servicios y futuros formularios.

## Modelo de dominio trabajado
- `IFacturable`: contrato de descripción, precio unitario y validación.
- `ServicioVeterinario`: clase base de servicios.
- `CitaVeterinaria` y `Hospitalizacion`: especializaciones de servicio.
- `ProductoFarmacia`: clase abstracta facturable.
- `LoteMedicamento`: producto farmacéutico con fecha de vencimiento.
- `Usuario` y `RolUsuario`: usuario del sistema y rol.
- `Factura` y `LineaFactura`: facturación, detalle, composición y uso polimórfico de elementos facturables.

## Persistencia preparada en este kit
- `PetCareDbContext` con conjuntos para servicios, productos, usuarios, facturas y líneas.
- Configuración inicial de herencia TPH para servicios y productos.
- Índices únicos para códigos, nombre de usuario y número de factura.
- Conversión del rol a texto.
- Relación entre factura y usuario emisor.
- Relación de composición entre factura y líneas.
- Restricción para que cada línea tenga un único origen: servicio o producto.
- Configuración para SQL Server mediante cadena de conexión en `appsettings`.

## GitHub
El `.gitignore` excluye `.vs`, `bin`, `obj`, archivos de usuario y archivos temporales. La carpeta `.vs` contiene datos locales de Visual Studio y no debe versionarse. Si ya se había agregado al índice, desde la raíz del repositorio se puede ejecutar:

```bash
git rm -r --cached --ignore-unmatch "Aplicacion PetCare/.vs"
```

Ajusta la ruta si la carpeta de la solución tiene otro nombre. Este comando retira los archivos del seguimiento de Git sin borrarlos del disco.

## Estado y límites
Este ZIP es un kit de trabajo de referencia generado a partir del trabajo conversado. **No es una copia exacta del repositorio local del equipo** porque no se ha proporcionado aquí el proyecto completo que tienen en Visual Studio.

No afirmar como terminado:
- La compilación en los equipos del grupo.
- La comparación completa de todas las propiedades y relaciones con el UML y el diagrama físico del profesor.
- La migración inicial aplicada en SQL Server.
- Formularios CRUD completos.
- Autenticación real y autorización por roles.
- Pruebas automatizadas y pruebas de integración.
- La actividad de robustez de Unidad 3.

Antes de usarlo como base definitiva, comparar el modelo y los nombres de tablas/columnas con los documentos oficiales del curso, configurar la cadena de conexión local y compilar en Visual Studio.
