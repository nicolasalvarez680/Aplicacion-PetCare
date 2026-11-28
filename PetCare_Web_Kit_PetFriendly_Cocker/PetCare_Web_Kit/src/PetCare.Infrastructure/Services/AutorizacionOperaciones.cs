
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PetCare.Domain.Enums;
using PetCare.Infrastructure.Persistence;

namespace PetCare.Infrastructure.Services;

internal static class AutorizacionOperaciones
{
    internal static async Task VerificarAsync(
        PetCareDbContext db,
        ClaimsPrincipal identidad,
        bool soloAdministrador = false)
    {
        if (db.Database.CurrentTransaction == null)
        {
            throw new InvalidOperationException(
                "La autorización requiere una transacción activa.");
        }

        if (identidad.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException(
                "Debes iniciar sesión.");
        }

        string? identificador = identidad
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(identificador, out int usuarioId))
        {
            throw new UnauthorizedAccessException(
                "No fue posible identificar al usuario.");
        }

        var usuario = await db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == usuarioId);

        if (usuario == null)
        {
            throw new UnauthorizedAccessException(
                "Tu cuenta ya no existe.");
        }

        if (usuario.Rol != RolUsuario.Administrador &&
            usuario.Rol != RolUsuario.Recepcionista)
        {
            throw new UnauthorizedAccessException(
                "No tienes permisos para realizar esta operación.");
        }

        string rolActual = usuario.Rol.ToString();

        string? rolSesion = identidad
            .FindFirst(ClaimTypes.Role)?.Value;

        string? nombreCompleto = identidad
            .FindFirst("NombreCompleto")?.Value;

        if (rolSesion != rolActual ||
            !identidad.IsInRole(rolActual) ||
            identidad.Identity.Name != usuario.NombreUsuario ||
            nombreCompleto != usuario.NombreCompleto)
        {
            throw new UnauthorizedAccessException(
                "Tu sesión ya no es válida. Inicia sesión nuevamente.");
        }

        if (soloAdministrador &&
            usuario.Rol != RolUsuario.Administrador)
        {
            throw new UnauthorizedAccessException(
                "No tienes permisos de administrador.");
        }
    }
}
