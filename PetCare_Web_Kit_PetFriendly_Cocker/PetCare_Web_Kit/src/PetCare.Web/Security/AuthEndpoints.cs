using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using PetCare.Domain.Entities;
using PetCare.Domain.Enums;
using PetCare.Infrastructure.Services;

namespace PetCare.Web.Security;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(
        this WebApplication app)
    {
        // =====================================
        // INICIO DE SESIÓN
        // =====================================

        app.MapPost("/auth/login", async (
            HttpContext context,
            UsuarioService servicio,
            IPasswordHasher<Usuario> passwordHasher,
            IAntiforgery antiforgery,
            IntentosLoginService intentos) =>
        {
            // Validar protección contra CSRF.
            if (!await antiforgery.IsRequestValidAsync(context))
            {
                return Results.BadRequest(
                    "Solicitud no válida.");
            }

            var formulario =
                await context.Request.ReadFormAsync();

            string nombreUsuario =
                formulario["nombreUsuario"].ToString().Trim();

            string contrasena =
                formulario["contrasena"].ToString();

            // Validar campos recibidos.
            if (string.IsNullOrWhiteSpace(nombreUsuario) ||
                string.IsNullOrEmpty(contrasena) ||
                nombreUsuario.Length > 50 ||
                contrasena.Length > 128)
            {
                return Results.Redirect("/login?error=1");
            }

            // Buscar el usuario en SQL Server.
            var usuario =
                await servicio.ObtenerPorNombreUsuarioAsync(
                    nombreUsuario);

            if (usuario == null)
            {
                return Results.Redirect("/login?error=1");
            }

            // Comprobar si la cuenta está bloqueada.
            if (intentos.EstaBloqueado(usuario.Id))
            {
                return Results.Redirect("/login?error=1");
            }

            // Verificar la contraseña almacenada.
            var resultado =
                passwordHasher.VerifyHashedPassword(
                    usuario,
                    usuario.ContrasenaHash,
                    contrasena);

            if (resultado ==
                PasswordVerificationResult.Failed)
            {
                // Registrar intento fallido.
                intentos.RegistrarFallo(usuario.Id);

                return Results.Redirect("/login?error=1");
            }

            // Comprobar que el rol sea válido.
            if (usuario.Rol != RolUsuario.Administrador &&
                usuario.Rol != RolUsuario.Recepcionista)
            {
                return Results.Redirect("/login?error=1");
            }

            // Reiniciar el contador si no hay bloqueo activo.
            if (!intentos.RegistrarExito(usuario.Id))
            {
                return Results.Redirect("/login?error=1");
            }

            // Crear la identidad del usuario.
            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.Id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    usuario.NombreUsuario),

                new Claim(
                    "NombreCompleto",
                    usuario.NombreCompleto),

                new Claim(
                    ClaimTypes.Role,
                    usuario.Rol.ToString())
            };

            var identidad = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults
                    .AuthenticationScheme);

            var principal =
                new ClaimsPrincipal(identidad);

            // Generar la cookie de autenticación.
            await context.SignInAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = formulario["recordar"].ToString() == "si",
                    AllowRefresh = true
                });

            return Results.Redirect("/dashboard");
        })
        .AllowAnonymous()
        .RequireRateLimiting("LimiteLogin");

        // =====================================
        // CIERRE DE SESIÓN
        // =====================================

        app.MapPost("/auth/logout", async (
            HttpContext context,
            IAntiforgery antiforgery) =>
        {
            if (!await antiforgery.IsRequestValidAsync(context))
            {
                return Results.BadRequest(
                    "Solicitud no válida.");
            }

            await context.SignOutAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme);

            return Results.Redirect("/login");
        })
        .RequireAuthorization();
    }
}
