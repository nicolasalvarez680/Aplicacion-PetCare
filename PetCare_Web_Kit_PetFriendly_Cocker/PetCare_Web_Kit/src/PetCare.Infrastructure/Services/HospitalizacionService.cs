using PetCare.Domain;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Persistence;

namespace PetCare.Infrastructure.Services;

public class HospitalizacionService(IDbContextFactory<PetCareDbContext> factory)
{
    public Task<List<Hospitalizacion>> ObtenerTodasAsync(ClaimsPrincipal identidad) =>
        OperacionClinica.EjecutarAsync(factory, identidad, db => db.Servicios.OfType<Hospitalizacion>().AsNoTracking().OrderBy(h => h.FechaAlta != null).ThenByDescending(h => h.Fecha).ToListAsync());
    public Task<Hospitalizacion?> ObtenerPorIdAsync(int id, ClaimsPrincipal identidad) =>
        OperacionClinica.EjecutarAsync(factory, identidad, db => db.Servicios.OfType<Hospitalizacion>().AsNoTracking().FirstOrDefaultAsync(h => h.Id == id));
    public Task<bool> EstaFacturadaAsync(int id, ClaimsPrincipal identidad) =>
        OperacionClinica.EjecutarAsync(factory, identidad, db => db.LineasFactura.AnyAsync(l => l.ServicioId == id));
    public Task CrearAsync(string codigo, string mascota, DateTime ingreso, decimal tarifa, string tipoMascota, string? razaMascota, ClaimsPrincipal identidad) =>
        OperacionClinica.EjecutarAsync(factory, identidad, async db => {
            var h = new Hospitalizacion(codigo, mascota, ingreso, tarifa, tipoMascota, razaMascota);
            if (await db.Servicios.AnyAsync(s => s.Codigo == h.Codigo)) throw new ReglaNegocioException("El código ya está registrado en un servicio.");
            db.Servicios.Add(h); return true;
        });
    public Task ActualizarAsync(int id, string codigo, string mascota, DateTime ingreso, decimal tarifa, string tipoMascota, string? razaMascota, ClaimsPrincipal identidad) =>
        OperacionClinica.EjecutarAsync(factory, identidad, async db => {
            var h = await Buscar(db, id); await SinFactura(db, id);
            h.ActualizarDatos(codigo, mascota, ingreso, tarifa, tipoMascota, razaMascota);
            if (await db.Servicios.AnyAsync(s => s.Codigo == h.Codigo && s.Id != id)) throw new ReglaNegocioException("El código ya está registrado en un servicio.");
            return true;
        });
    public Task DarAltaAsync(int id, DateTime alta, ClaimsPrincipal identidad) =>
        OperacionClinica.EjecutarAsync(factory, identidad, async db => {
            var h = await Buscar(db, id); await SinFactura(db, id);
            if (!h.DarAlta(alta)) throw new ReglaNegocioException("Alta inválida: comprueba la fecha de ingreso, la fecha actual y el estado.");
            return true;
        });
    public Task EliminarAsync(int id, ClaimsPrincipal identidad) =>
        OperacionClinica.EjecutarAsync(factory, identidad, async db => {
            var h = await Buscar(db, id); await SinFactura(db, id); db.Servicios.Remove(h); return true;
        });
    private static async Task<Hospitalizacion> Buscar(PetCareDbContext db, int id) =>
        await db.Servicios.OfType<Hospitalizacion>().FirstOrDefaultAsync(h => h.Id == id) ?? throw new ReglaNegocioException("La hospitalización no existe.");
    private static async Task SinFactura(PetCareDbContext db, int id)
    { if (await db.LineasFactura.AnyAsync(l => l.ServicioId == id)) throw new ReglaNegocioException("La hospitalización tiene facturas asociadas."); }
}
