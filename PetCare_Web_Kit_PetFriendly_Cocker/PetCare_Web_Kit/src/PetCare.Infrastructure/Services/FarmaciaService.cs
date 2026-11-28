using PetCare.Domain;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Persistence;

namespace PetCare.Infrastructure.Services;

public class FarmaciaService(IDbContextFactory<PetCareDbContext> factory)
{
    public Task<List<LoteMedicamento>> ObtenerTodosAsync(ClaimsPrincipal identidad) =>
        OperacionClinica.EjecutarAsync(factory, identidad, db => db.Productos.OfType<LoteMedicamento>().AsNoTracking().OrderBy(l => l.Nombre).ThenBy(l => l.FechaVencimiento).ToListAsync());
    public Task<LoteMedicamento?> ObtenerPorIdAsync(int id, ClaimsPrincipal identidad) =>
        OperacionClinica.EjecutarAsync(factory, identidad, db => db.Productos.OfType<LoteMedicamento>().AsNoTracking().FirstOrDefaultAsync(l => l.Id == id));
    public Task<bool> EstaFacturadoAsync(int id, ClaimsPrincipal identidad) =>
        OperacionClinica.EjecutarAsync(factory, identidad, db => db.LineasFactura.AnyAsync(l => l.ProductoId == id));
    public Task CrearAsync(string codigo, string nombre, DateOnly vencimiento, decimal precio, int existencias, ClaimsPrincipal identidad) =>
        OperacionClinica.EjecutarAsync(factory, identidad, async db => {
            var lote = new LoteMedicamento(codigo, nombre, vencimiento, precio, existencias);
            if (await db.Productos.AnyAsync(p => p.Codigo == lote.Codigo)) throw new ReglaNegocioException("El código de lote ya existe.");
            db.Productos.Add(lote); return true;
        });
    public Task ActualizarAsync(int id, string codigo, string nombre, DateOnly vencimiento, decimal precio, ClaimsPrincipal identidad) =>
        OperacionClinica.EjecutarAsync(factory, identidad, async db => {
            var lote = await Buscar(db, id); await SinFactura(db, id);
            lote.ActualizarDatos(codigo, nombre, vencimiento, precio);
            if (await db.Productos.AnyAsync(p => p.Codigo == lote.Codigo && p.Id != id)) throw new ReglaNegocioException("El código de lote ya existe.");
            return true;
        });
    public Task ReponerAsync(int id, int cantidad, ClaimsPrincipal identidad) =>
        OperacionClinica.EjecutarAsync(factory, identidad, async db => { var lote = await Buscar(db, id); if (lote.EstaVencido()) throw new ReglaNegocioException("No se puede reponer un lote vencido."); lote.Reponer(cantidad); return true; });
    public Task EliminarAsync(int id, ClaimsPrincipal identidad) =>
        OperacionClinica.EjecutarAsync(factory, identidad, async db => { var lote = await Buscar(db, id); await SinFactura(db, id); db.Productos.Remove(lote); return true; });
    private static async Task<LoteMedicamento> Buscar(PetCareDbContext db, int id) =>
        await db.Productos.OfType<LoteMedicamento>().FirstOrDefaultAsync(l => l.Id == id) ?? throw new ReglaNegocioException("El lote no existe.");
    private static async Task SinFactura(PetCareDbContext db, int id)
    { if (await db.LineasFactura.AnyAsync(l => l.ProductoId == id)) throw new ReglaNegocioException("El lote tiene facturas asociadas; sus datos históricos están protegidos."); }
}
