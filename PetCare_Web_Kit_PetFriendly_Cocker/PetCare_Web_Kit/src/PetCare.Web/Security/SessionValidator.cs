using System.Security.Claims;
using PetCare.Domain.Enums;
using PetCare.Infrastructure.Services;

namespace PetCare.Web.Security;

public class SessionValidator
{
    private readonly UsuarioService _servicio;

    public SessionValidator(UsuarioService servicio)
    {
        _servicio = servicio;
    }

    public async Task<bool> EsSesionValidaAsync(
        ClaimsPrincipal principal)
    {
        // Comprobar que existe una sesión autenticada.
        if (principal.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        // Obtener el identificador del usuario.
        string? idClaim = principal.FindFirst(
            ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(idClaim, out int id))
        {
            return false;
        }

        // Consultar los datos actuales en SQL Server.
        var usuario = await _servicio.ObtenerPorIdAsync(id);

        // Comprobar que el usuario todavía existe.
        if (usuario == null)
        {
            return false;
        }

        // Comprobar que tiene un rol válido.
        if (usuario.Rol != RolUsuario.Administrador &&
            usuario.Rol != RolUsuario.Recepcionista)
        {
            return false;
        }

        // Comparar el rol actual con el de la sesión.
        string? rolSesion = principal.FindFirst(
            ClaimTypes.Role)?.Value;

        if (rolSesion != usuario.Rol.ToString())
        {
            return false;
        }

        // Comprobar el nombre de usuario.
        if (principal.Identity?.Name != usuario.NombreUsuario)
        {
            return false;
        }

        // Comprobar el nombre completo.
        string? nombreCompleto = principal.FindFirst(
            "NombreCompleto")?.Value;

        if (nombreCompleto != usuario.NombreCompleto)
        {
            return false;
        }

        return true;
    }
}
