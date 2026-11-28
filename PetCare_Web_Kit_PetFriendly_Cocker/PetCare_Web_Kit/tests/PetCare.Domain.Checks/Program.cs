using PetCare.Domain;
using PetCare.Domain.Entities;
using PetCare.Domain.Enums;

int pruebas = 0;
void Verificar(bool condicion, string nombre)
{
    if (!condicion) throw new Exception("FALLÓ: " + nombre);
    Console.WriteLine("OK: " + nombre); pruebas++;
}
void Rechazar<T>(Action accion, string nombre) where T : Exception
{
    try { accion(); }
    catch (T) { Verificar(true, nombre); return; }
    throw new Exception("FALLÓ (se esperaba rechazo): " + nombre);
}
var ingreso = DateTime.Today.AddDays(-2).AddHours(8);
var h = new Hospitalizacion("HOS-TEST", "Max", ingreso, 80000m, "Perro", "Cocker Spaniel");
Verificar(h.CalcularDias() == 3 && h.ObtenerPrecioUnitario() == 240000m, "Estancia inclusiva y costo");
Rechazar<ArgumentException>(() => new Hospitalizacion("FUT", "Max", DateTime.Now.AddDays(1), 80m, "Perro", "Cocker Spaniel"), "Ingreso futuro");
Rechazar<ArgumentException>(() => new Hospitalizacion("NEG", "Max", ingreso, -1m, "Perro", "Cocker Spaniel"), "Tarifa negativa");
Rechazar<ArgumentException>(() => new Hospitalizacion("DEC", "Max", ingreso, 1.001m, "Perro", "Cocker Spaniel"), "Tarifa con tres decimales");
Rechazar<ReglaNegocioException>(() => h.ValidarFacturable(1), "Hospitalización activa no facturable");
Verificar(!h.DarAlta(ingreso.AddMinutes(-1)), "Alta anterior rechazada");
Verificar(!h.DarAlta(DateTime.Now.AddDays(1)), "Alta futura rechazada");
Verificar(h.DarAlta(ingreso.AddDays(1)) && h.CalcularDias() == 2 && h.ObtenerPrecioUnitario() == 160000m, "Alta válida y costo fijo");
Verificar(!h.DarAlta(DateTime.Now), "Segunda alta rechazada");
Rechazar<ReglaNegocioException>(() => h.ActualizarDatos("HOS-TEST", "Max", ingreso, 100m, "Perro", "Cocker Spaniel"), "Estancia cerrada no editable");
var cita = new CitaVeterinaria("CIT-TEST", "Luna", DateTime.Today, "Gato", null);
Rechazar<ReglaNegocioException>(() => cita.ValidarFacturable(1), "Cita pendiente no facturable");
Verificar(cita.EstablecerDuracion(60) && cita.Confirmar("Veterinario") && cita.ObtenerPrecioUnitario() == 105000m, "Confirmación y tarifa cita");
Verificar(h.TipoMascota == "Perro" && h.RazaMascota == "Cocker Spaniel", "Datos de perro guardados");
Verificar(cita.TipoMascota == "Gato" && cita.RazaMascota is null, "Gato sin raza");
Rechazar<ArgumentException>(() => new CitaVeterinaria("PET1", "Max", DateTime.Today, "Perro", " "), "Perro exige raza");
Rechazar<ArgumentException>(() => new Hospitalizacion("PET2", "Max", ingreso, 10m, "Otro", null), "Tipo inválido rechazado");
var mascota = new CitaVeterinaria("PET3", "Max", DateTime.Today, "Perro", "  Mestizo  ");
Verificar(mascota.RazaMascota == "Mestizo", "Normalización de raza");
Rechazar<ArgumentException>(() => mascota.ActualizarDatos("PET3", "Max", DateTime.Today, 30, "Perro", new string('x',81)), "Raza demasiado larga rechazada");
Verificar(mascota.RazaMascota == "Mestizo", "Edición inválida conserva datos");
mascota.ActualizarDatos("PET3", "Max", DateTime.Today, 30, "Gato", "Mestizo");
Verificar(mascota.TipoMascota == "Gato" && mascota.RazaMascota is null, "Cambiar a gato elimina raza");
var lote = new LoteMedicamento("MED-TEST", "Medicamento prueba", DateOnly.FromDateTime(DateTime.Today.AddDays(30)), 15000m, 10);
var vencido = new LoteMedicamento("MED-V", "Vencido", DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), 1m, 2);
Rechazar<ReglaNegocioException>(() => vencido.ValidarFacturable(1), "Vencimiento");
Rechazar<ReglaNegocioException>(() => lote.ValidarFacturable(11), "Stock insuficiente");
var admin = new Usuario("admin-test", "Admin test", RolUsuario.Administrador, "hash-prueba-no-usar");
var recep = new Usuario("recep-test", "Recep test", RolUsuario.Recepcionista, "hash-prueba-no-usar");
var f = new Factura("FAC-TEST", DateTime.Now, "Cliente prueba", recep);
f.AgregarLinea(cita); f.AgregarLinea(h); f.AgregarLinea(lote, 2); lote.Descontar(2); f.RegistrarDescuentoInventario();
Verificar(f.Lineas.Count == 3 && f.Total == 295000m && lote.Existencias == 8 && f.InventarioDescontado, "Factura mixta y descuento de unidades");
Rechazar<UnauthorizedAccessException>(() => f.Anular(recep), "Recepcionista no anula");
f.Anular(admin); lote.Reponer(2);
Verificar(f.EstaAnulada && lote.Existencias == 10 && f.Total == 295000m, "Anulación conserva importe y reposición");
Rechazar<ReglaNegocioException>(() => f.Anular(admin), "Segunda anulación rechazada");
Rechazar<ReglaNegocioException>(() => f.AgregarLinea(cita), "Factura anulada no editable");
Console.WriteLine($"{pruebas} pruebas de dominio superadas.");
