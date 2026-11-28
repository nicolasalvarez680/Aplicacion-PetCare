
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using PetCare.Domain.Enums;

namespace PetCare.Web.Security;

public class PermisosService
{
    private readonly AuthenticationStateProvider
        _authenticationState;

    private readonly SessionValidator _validador;

    public PermisosService(
        AuthenticationStateProvider authenticationState,
        SessionValidator validador)
    {
        _authenticationState = authenticationState;
        _validador = validador;
    }

    // =====================================
    // PERMISOS DE ADMINISTRADOR
    // =====================================

    public async Task VerificarAdministradorAsync()
    {
        await ObtenerAdministradorAsync();
    }

    public async Task<ClaimsPrincipal>
        ObtenerAdministradorAsync()
    {
        var usuario =
            await ObtenerPersonalAutorizadoAsync();

        if (!usuario.IsInRole(
            nameof(RolUsuario.Administrador)))
        {
            throw new UnauthorizedAccessException(
                "No tienes permisos de administrador.");
        }

        return usuario;
    }

    // =====================================
    // PERMISOS DEL PERSONAL DE LA CLÍNICA
    // =====================================

    public async Task VerificarPersonalAutorizadoAsync()
    {
        await ObtenerPersonalAutorizadoAsync();
    }

    public async Task<ClaimsPrincipal>
        ObtenerPersonalAutorizadoAsync()
    {
        var estado = await _authenticationState
            .GetAuthenticationStateAsync();

        var usuario = estado.User;

        if (usuario.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException(
                "Debes iniciar sesión.");
        }

        bool autorizado =
            usuario.IsInRole(
                nameof(RolUsuario.Administrador)) ||
            usuario.IsInRole(
                nameof(RolUsuario.Recepcionista));

        if (!autorizado)
        {
            throw new UnauthorizedAccessException(
                "No tienes permisos para realizar esta operación.");
        }

        bool sesionValida =
            await _validador.EsSesionValidaAsync(usuario);

        if (!sesionValida)
        {
            throw new UnauthorizedAccessException(
                "Tu sesión ya no es válida. Inicia sesión nuevamente.");
        }

        return usuario;
    }
}
