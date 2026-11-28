using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace PetCare.Web.Security;

public class PetCareCookieEvents
    : CookieAuthenticationEvents
{
    private readonly SessionValidator _validador;

    public PetCareCookieEvents(
        SessionValidator validador)
    {
        _validador = validador;
    }

    public override async Task ValidatePrincipal(
        CookieValidatePrincipalContext context)
    {
        bool sesionValida =
            context.Principal != null &&
            await _validador.EsSesionValidaAsync(
                context.Principal);

        if (!sesionValida)
        {
            // Rechazar la identidad anterior.
            context.RejectPrincipal();

            // Eliminar la cookie de autenticación.
            await context.HttpContext.SignOutAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme);
        }
    }
}
