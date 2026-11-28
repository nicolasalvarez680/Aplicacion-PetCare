using PetCare.Domain;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Persistence;

namespace PetCare.Infrastructure.Services;

public sealed record CatalogoFacturable(List<ServicioVeterinario> Servicios, List<LoteMedicamento> Lotes);
public class FacturaService(IDbContextFactory<PetCareDbContext> factory)
{
    private static IQueryable<Factura> Detalles(PetCareDbContext db) => db.Facturas.Include(f => f.UsuarioEmisor).Include(f => f.Lineas).ThenInclude(l => l.Servicio).Include(f => f.Lineas).ThenInclude(l => l.Producto);
    public Task<List<Factura>> ObtenerTodasAsync(ClaimsPrincipal identidad) =>
        OperacionClinica.EjecutarAsync(factory, identidad, db => Detalles(db).AsNoTracking().OrderByDescending(f => f.FechaEmision).ToListAsync());
    public Task<Factura?> ObtenerPorIdAsync(int id, ClaimsPrincipal identidad) =>
        OperacionClinica.EjecutarAsync(factory, identidad, db => Detalles(db).AsNoTracking().FirstOrDefaultAsync(f => f.Id == id));
    public Task<CatalogoFacturable> ObtenerCatalogoAsync(ClaimsPrincipal identidad) =>
        OperacionClinica.EjecutarAsync(factory, identidad, async db => {
            var usados = db.Facturas.Where(f => !f.EstaAnulada).SelectMany(f => f.Lineas).Where(l => l.ServicioId.HasValue).Select(l => l.ServicioId!.Value);
            var servicios = await db.Servicios.AsNoTracking().Where(s => !usados.Contains(s.Id)).OrderBy(s => s.Codigo).ToListAsync();
            servicios = servicios.Where(s => { try { s.ValidarFacturable(1); return true; } catch (InvalidOperationException) { return false; } }).ToList();
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var lotes = await db.Productos.OfType<LoteMedicamento>().AsNoTracking().Where(l => l.Existencias > 0 && l.FechaVencimiento >= hoy).OrderBy(l => l.Nombre).ToListAsync();
            return new CatalogoFacturable(servicios, lotes);
        });
    public async Task<int> CrearAsync(string numero, string cliente, IReadOnlyCollection<int> servicios, IReadOnlyDictionary<int,int> productos, ClaimsPrincipal identidad)
    {
        if (servicios.Count + productos.Count is < 1 or > 100 || servicios.Distinct().Count() != servicios.Count || productos.Any(p => p.Value <= 0)) throw new ArgumentException("Selecciona entre 1 y 100 líneas válidas, sin duplicados.");
        var factura = await OperacionClinica.EjecutarAsync(factory, identidad, async db => {
            int usuarioId = int.Parse(identidad.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var emisor = await db.Usuarios.SingleAsync(u => u.Id == usuarioId);
            var f = new Factura(numero, DateTime.Now, cliente, emisor);
            if (await db.Facturas.AnyAsync(x => x.Numero == f.Numero)) throw new ReglaNegocioException("El número de factura ya existe.");
            foreach (int id in servicios.Order())
            {
                if (await db.Facturas.Where(x => !x.EstaAnulada).SelectMany(x => x.Lineas).AnyAsync(l => l.ServicioId == id)) throw new ReglaNegocioException("Uno de los servicios ya está facturado.");
                var servicio = await db.Servicios.FindAsync(id) ?? throw new ReglaNegocioException("Servicio no disponible.");
                f.AgregarLinea(servicio);
            }
            foreach (var par in productos.OrderBy(x => x.Key))
            {
                var lote = await db.Productos.OfType<LoteMedicamento>().FirstOrDefaultAsync(l => l.Id == par.Key) ?? throw new ReglaNegocioException("Lote no disponible.");
                f.AgregarLinea(lote, par.Value); lote.Descontar(par.Value);
            }
            f.RegistrarDescuentoInventario(); db.Facturas.Add(f); return f;
        });
        return factura.Id;
    }
    public Task AnularAsync(int id, ClaimsPrincipal identidad) =>
        OperacionClinica.EjecutarAsync(factory, identidad, async db => {
            var f = await Detalles(db).FirstOrDefaultAsync(x => x.Id == id) ?? throw new ReglaNegocioException("La factura no existe.");
            var admin = await db.Usuarios.SingleAsync(u => u.Id == int.Parse(identidad.FindFirst(ClaimTypes.NameIdentifier)!.Value));
            f.Anular(admin);
            if (f.InventarioDescontado)
                foreach (var linea in f.Lineas) if (linea.Producto is LoteMedicamento lote) lote.Reponer(linea.Cantidad);
            return true;
        }, soloAdmin: true);
}
