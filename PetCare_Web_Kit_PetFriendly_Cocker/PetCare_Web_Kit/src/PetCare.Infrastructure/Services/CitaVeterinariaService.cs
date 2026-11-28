using PetCare.Domain;

using System.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Persistence;

namespace PetCare.Infrastructure.Services;

public class CitaVeterinariaService
{
    private readonly IDbContextFactory<PetCareDbContext>
        _dbFactory;

    public CitaVeterinariaService(
        IDbContextFactory<PetCareDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // =====================================
    // LISTAR Y BUSCAR CITAS
    // =====================================

    public async Task<List<CitaVeterinaria>>
        ObtenerTodasAsync(
            ClaimsPrincipal identidad,
            string? busqueda = null)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        await using var transaccion =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        await AutorizacionOperaciones
            .VerificarAsync(db, identidad);

        var consulta = db.Servicios
            .OfType<CitaVeterinaria>()
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            busqueda = busqueda.Trim();

            consulta = consulta.Where(c =>
                c.Codigo.Contains(busqueda) ||
                c.NombreMascota.Contains(busqueda));
        }

        var citas = await consulta
            .OrderBy(c => c.Fecha)
            .ThenBy(c => c.Codigo)
            .ToListAsync();

        await transaccion.CommitAsync();

        return citas;
    }

    // =====================================
    // CONSULTAR UNA CITA
    // =====================================

    public async Task<CitaVeterinaria?>
        ObtenerPorIdAsync(
            int id,
            ClaimsPrincipal identidad)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        await using var transaccion =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        await AutorizacionOperaciones
            .VerificarAsync(db, identidad);

        var cita = await db.Servicios
            .OfType<CitaVeterinaria>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        await transaccion.CommitAsync();

        return cita;
    }

    // =====================================
    // REGISTRAR UNA CITA
    // =====================================

    public async Task CrearAsync(
        string codigo,
        string mascota,
        DateTime fecha,
        int duracionMinutos,
        string tipoMascota,
        string? razaMascota,
        ClaimsPrincipal identidad)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        await using var transaccion =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        await AutorizacionOperaciones
            .VerificarAsync(db, identidad);

        var cita = new CitaVeterinaria(
            codigo,
            mascota,
            fecha, tipoMascota, razaMascota);

        if (!cita.EstablecerDuracion(duracionMinutos))
        {
            throw new ArgumentException(
                "La duración debe estar entre 15 y 180 minutos.");
        }

        // El código debe ser único entre todos
        // los servicios veterinarios.
        bool existe = await db.Servicios
            .AnyAsync(s => s.Codigo == cita.Codigo);

        if (existe)
        {
            throw new ReglaNegocioException(
                "El código del servicio ya está registrado.");
        }

        db.Servicios.Add(cita);

        await db.SaveChangesAsync();
        await transaccion.CommitAsync();
    }

    // =====================================
    // ACTUALIZAR UNA CITA
    // =====================================

    public async Task ActualizarAsync(
        int id,
        string codigo,
        string mascota,
        DateTime fecha,
        int duracionMinutos,
        string tipoMascota,
        string? razaMascota,
        ClaimsPrincipal identidad)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        await using var transaccion =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        await AutorizacionOperaciones
            .VerificarAsync(db, identidad);

        var cita = await BuscarCitaAsync(db, id);

        await VerificarNoFacturadaAsync(db, id);

        cita.ActualizarDatos(
            codigo,
            mascota,
            fecha,
            duracionMinutos, tipoMascota, razaMascota);

        bool existe = await db.Servicios
            .AnyAsync(s =>
                s.Codigo == cita.Codigo &&
                s.Id != id);

        if (existe)
        {
            throw new ReglaNegocioException(
                "El código del servicio ya está registrado.");
        }

        await db.SaveChangesAsync();
        await transaccion.CommitAsync();
    }

    // =====================================
    // CONFIRMAR UNA CITA
    // =====================================

    public async Task ConfirmarAsync(
        int id,
        string veterinario,
        ClaimsPrincipal identidad)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        await using var transaccion =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        await AutorizacionOperaciones
            .VerificarAsync(db, identidad);

        var cita = await BuscarCitaAsync(db, id);

        await VerificarNoFacturadaAsync(db, id);

        if (cita.VeterinarioAsignado is not null)
        {
            throw new ReglaNegocioException(
                "La cita ya se encuentra confirmada.");
        }

        if (!cita.Confirmar(veterinario))
        {
            throw new ArgumentException(
                "Debes indicar un veterinario válido " +
                "de máximo 100 caracteres.");
        }

        await db.SaveChangesAsync();
        await transaccion.CommitAsync();
    }

    // =====================================
    // ELIMINAR UNA CITA
    // =====================================

    public async Task EliminarAsync(
        int id,
        ClaimsPrincipal identidad)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        await using var transaccion =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        await AutorizacionOperaciones
            .VerificarAsync(db, identidad);

        var cita = await BuscarCitaAsync(db, id);

        await VerificarNoFacturadaAsync(db, id);

        db.Servicios.Remove(cita);

        await db.SaveChangesAsync();
        await transaccion.CommitAsync();
    }

    // =====================================
    // CONSULTAR ESTADO DE FACTURACIÓN
    // =====================================

    public async Task<bool> EstaFacturadaAsync(
        int id,
        ClaimsPrincipal identidad)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        await using var transaccion =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        await AutorizacionOperaciones
            .VerificarAsync(db, identidad);

        await BuscarCitaAsync(db, id);

        bool facturada = await db.LineasFactura
            .AnyAsync(l => l.ServicioId == id);

        await transaccion.CommitAsync();

        return facturada;
    }

    // =====================================
    // MÉTODOS PRIVADOS REUTILIZABLES
    // =====================================

    private static async Task<CitaVeterinaria>
        BuscarCitaAsync(
            PetCareDbContext db,
            int id)
    {
        var cita = await db.Servicios
            .OfType<CitaVeterinaria>()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cita == null)
        {
            throw new ReglaNegocioException(
                "La cita veterinaria no existe.");
        }

        return cita;
    }

    private static async Task VerificarNoFacturadaAsync(
        PetCareDbContext db,
        int id)
    {
        bool tieneFactura = await db.LineasFactura
            .AnyAsync(l => l.ServicioId == id);

        if (tieneFactura)
        {
            throw new ReglaNegocioException(
                "No se puede modificar ni eliminar " +
                "una cita que tiene facturas asociadas.");
        }
    }
}
