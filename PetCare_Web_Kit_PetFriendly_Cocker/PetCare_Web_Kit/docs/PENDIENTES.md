NOTA: documento histórico de la versión inicial. Para esta entrega consulta README.md y docs/PRUEBAS_Y_ENTREGA.md.

# Pendientes de PetCare Web

Esta lista evita confundir la base inicial con un sistema terminado.

## Antes de formularios
- [ ] Comparar todas las propiedades y relaciones con el UML definitivo.
- [ ] Comparar nombres, tipos, longitudes, restricciones y nulabilidad con el diagrama SQL del profesor.
- [ ] Confirmar la entidad/relación de Cita, Hospitalización, Lote, Factura, Línea y Usuario.
- [ ] Compilar en la máquina de los tres integrantes.
- [ ] Crear migración inicial y comprobarla en SQL Server.
- [ ] Crear un servicio de aplicación de prueba para insertar y consultar una entidad.

## Funcionalidades Web
- [ ] Formularios CRUD de citas.
- [ ] Formularios CRUD de hospitalizaciones y alta.
- [ ] Administración de lotes y control de vencimiento.
- [ ] Facturación con líneas de servicio/producto.
- [ ] Consulta de facturas.
- [ ] Login seguro y autorización por roles.
- [ ] Mensajes de validación en español.

## Robustez / Unidad 3
- [ ] Guardar commit/copia `antes-robustez`.
- [ ] Elegir un formulario persistido en SQL Server.
- [ ] Ejecutar y documentar los ocho ataques del documento del profesor.
- [ ] Crear excepción propia donde tenga sentido.
- [ ] Capturar excepciones específicas y registrar con ILogger.
- [ ] Repetir los mismos ataques y documentar `despues-robustez`.
- [ ] Completar anexos, investigación y reflexiones individuales según la guía.

## Calidad
- [ ] Pruebas unitarias de reglas del dominio.
- [ ] Pruebas de integración con SQL Server.
- [ ] Documentación XML de métodos públicos relevantes.
- [ ] Revisar concurrencia, transacciones y restricciones de integridad.
