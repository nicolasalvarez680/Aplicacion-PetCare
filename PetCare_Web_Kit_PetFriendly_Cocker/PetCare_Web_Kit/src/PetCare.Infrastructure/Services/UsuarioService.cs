using PetCare.Domain;
using System.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Persistence;
using PetCare.Domain.Enums;

namespace PetCare.Infrastructure.Services;

public class UsuarioService
{
    private readonly IDbContextFactory<PetCareDbContext> _dbFactory;

    public UsuarioService(
        IDbContextFactory<PetCareDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // =====================================
    // VERIFICACIÓN TRANSACCIONAL DE PERMISOS
    // =====================================

    private static async Task VerificarAdministradorAsync(
        PetCareDbContext db,
        ClaimsPrincipal administrador)
    {
        if (administrador.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException(
                "Debes iniciar sesión.");
        }

        if (!administrador.IsInRole(
            nameof(RolUsuario.Administrador)))
        {
            throw new UnauthorizedAccessException(
                "No tienes permisos de administrador.");
        }

        string? identificador = administrador
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(identificador, out int administradorId))
        {
            throw new UnauthorizedAccessException(
                "No fue posible identificar al administrador.");
        }

        // Consultar al administrador dentro de
        // la misma transacción de la operación.
        var usuarioActual = await db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.Id == administradorId);

        if (usuarioActual == null)
        {
            throw new UnauthorizedAccessException(
                "Tu cuenta ya no existe.");
        }

        // Comprobar el rol actual en SQL Server.
        if (usuarioActual.Rol != RolUsuario.Administrador)
        {
            throw new UnauthorizedAccessException(
                "Ya no tienes permisos de administrador.");
        }

        // Comprobar que la identidad siga vigente.
        string? nombreCompleto = administrador
            .FindFirst("NombreCompleto")?.Value;

        if (usuarioActual.NombreUsuario !=
                administrador.Identity.Name ||
            usuarioActual.NombreCompleto != nombreCompleto)
        {
            throw new UnauthorizedAccessException(
                "Tu sesión ya no es válida. Inicia sesión nuevamente.");
        }
    }

    // =====================================
    // CONSULTAR TODOS LOS USUARIOS
    // =====================================

    public async Task<List<Usuario>> ObtenerTodosAsync()
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        return await db.Usuarios
            .AsNoTracking()
            .OrderBy(u => u.NombreUsuario)
            .ToListAsync();
    }

    // =====================================
    // VERIFICAR EXISTENCIA
    // =====================================

    public async Task<bool> ExisteUsuarioAsync(
        string nombreUsuario)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        return await db.Usuarios
            .AnyAsync(u =>
                u.NombreUsuario == nombreUsuario);
    }

    // =====================================
    // CREAR USUARIO
    // =====================================

    public async Task CrearAsync(
        Usuario usuario,
        ClaimsPrincipal administrador)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        await using var transaccion =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        // Verificar los permisos dentro
        // de la transacción.
        await VerificarAdministradorAsync(
            db, administrador);

        bool existe = await db.Usuarios.AnyAsync(
            u => u.NombreUsuario == usuario.NombreUsuario);

        if (existe)
        {
            throw new ReglaNegocioException(
                "El nombre de usuario ya está registrado.");
        }

        db.Usuarios.Add(usuario);

        await db.SaveChangesAsync();
        await transaccion.CommitAsync();
    }

    // =====================================
    // CONSULTAR USUARIO POR ID
    // =====================================

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        return await db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    // =====================================
    // ACTUALIZAR USUARIO
    // =====================================

    public async Task ActualizarAsync(
        int id,
        string nombreUsuario,
        string nombreCompleto,
        RolUsuario rol,
        ClaimsPrincipal administrador)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        await using var transaccion =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        // Verificar los permisos dentro
        // de la transacción.
        await VerificarAdministradorAsync(
            db, administrador);

        var usuario = await db.Usuarios
            .FirstOrDefaultAsync(u => u.Id == id);

        if (usuario == null)
        {
            throw new ReglaNegocioException(
                "El usuario no existe.");
        }

        // Proteger el rol del último administrador.
        if (usuario.Rol == RolUsuario.Administrador &&
            rol != RolUsuario.Administrador)
        {
            int cantidadAdministradores =
                await db.Usuarios.CountAsync(u =>
                    u.Rol == RolUsuario.Administrador);

            if (cantidadAdministradores <= 1)
            {
                throw new ReglaNegocioException(
                    "No se puede cambiar el rol del último administrador.");
            }
        }

        // Actualizar los datos mediante la entidad.
        usuario.ActualizarDatos(
            nombreUsuario,
            nombreCompleto,
            rol);

        // Evitar nombres de usuario duplicados.
        bool existe = await db.Usuarios.AnyAsync(u =>
            u.NombreUsuario == usuario.NombreUsuario &&
            u.Id != id);

        if (existe)
        {
            throw new ReglaNegocioException(
                "El nombre de usuario ya está registrado.");
        }

        await db.SaveChangesAsync();
        await transaccion.CommitAsync();
    }

    // =====================================
    // ELIMINAR USUARIO
    // =====================================

    public async Task EliminarAsync(
        int id,
        ClaimsPrincipal administrador)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        await using var transaccion =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        // Verificar los permisos dentro
        // de la transacción.
        await VerificarAdministradorAsync(
            db, administrador);

        var usuario = await db.Usuarios
            .FirstOrDefaultAsync(u => u.Id == id);

        if (usuario == null)
        {
            throw new ReglaNegocioException(
                "El usuario no existe.");
        }

        // Proteger al último administrador.
        if (usuario.Rol == RolUsuario.Administrador)
        {
            int cantidadAdministradores =
                await db.Usuarios.CountAsync(u =>
                    u.Rol == RolUsuario.Administrador);

            if (cantidadAdministradores <= 1)
            {
                throw new ReglaNegocioException(
                    "No se puede eliminar al último administrador.");
            }
        }

        // Proteger la integridad de las facturas.
        bool tieneFacturas = await db.Facturas
            .AnyAsync(f => f.UsuarioEmisorId == id);

        if (tieneFacturas)
        {
            throw new ReglaNegocioException(
                "No se puede eliminar un usuario que tiene facturas asociadas.");
        }

        db.Usuarios.Remove(usuario);

        await db.SaveChangesAsync();
        await transaccion.CommitAsync();
    }

    // =====================================
    // CONSULTAR USUARIO POR NOMBRE
    // =====================================

    public async Task RegistrarCuentaAsync(Usuario usuario)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        if (await db.Usuarios.AnyAsync(u => u.NombreUsuario == usuario.NombreUsuario))
            throw new ReglaNegocioException("El nombre de usuario ya está registrado.");
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(
        string nombreUsuario)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario))
            return null;

        nombreUsuario = nombreUsuario.Trim();

        await using var db =
            await _dbFactory.CreateDbContextAsync();

        return await db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.NombreUsuario == nombreUsuario);
    }
}
