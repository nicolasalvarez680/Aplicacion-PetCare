using Microsoft.EntityFrameworkCore;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Persistence;

namespace PetCare.Infrastructure.Services;

public class DashboardService
{
    private readonly IDbContextFactory<PetCareDbContext>
        _dbFactory;

    public DashboardService(
        IDbContextFactory<PetCareDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<DashboardResumen> ObtenerResumenAsync()
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        DateTime hoy = DateTime.Today;
        DateTime manana = hoy.AddDays(1);

        // Citas programadas para el día actual.
        var citasHoy = db.Servicios
            .OfType<CitaVeterinaria>()
            .AsNoTracking()
            .Where(c =>
                c.Fecha >= hoy &&
                c.Fecha < manana);

        int totalCitasHoy =
            await citasHoy.CountAsync();

        // Citas con veterinario asignado.
        int citasConfirmadas =
            await citasHoy.CountAsync(c =>
                c.VeterinarioAsignado != null);

        // Hospitalizaciones sin fecha de alta.
        int hospitalizacionesActivas =
            await db.Servicios
                .OfType<Hospitalizacion>()
                .AsNoTracking()
                .CountAsync(h =>
                    h.FechaAlta == null);

        // Cantidad de registros de farmacia.
        int productosRegistrados =
            await db.Productos
                .AsNoTracking()
                .CountAsync();

        // Facturas emitidas hoy y no anuladas.
        int facturasHoy =
            await db.Facturas
                .AsNoTracking()
                .CountAsync(f =>
                    f.FechaEmision >= hoy &&
                    f.FechaEmision < manana &&
                    !f.EstaAnulada);

        return new DashboardResumen(
            totalCitasHoy,
            citasConfirmadas,
            hospitalizacionesActivas,
            productosRegistrados,
            facturasHoy,
            DateTime.Now);
    }
}

// Objeto de transferencia de datos del dashboard.
public sealed record DashboardResumen(
    int CitasHoy,
    int CitasConfirmadas,
    int HospitalizacionesActivas,
    int ProductosRegistrados,
    int FacturasHoy,
    DateTime ActualizadoEn);
