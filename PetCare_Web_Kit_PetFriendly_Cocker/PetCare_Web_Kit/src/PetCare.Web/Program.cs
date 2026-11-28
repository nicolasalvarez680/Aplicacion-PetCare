using Microsoft.EntityFrameworkCore;
using PetCare.Infrastructure.Persistence;
using PetCare.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using PetCare.Domain.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using PetCare.Domain.Enums;
using PetCare.Web.Security;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONEXIÓN A LA BASE DE DATOS
// ==========================================

var connection = builder.Configuration
    .GetConnectionString("PetCareConnection")
    ?? throw new InvalidOperationException(
        "Falta ConnectionStrings:PetCareConnection en la configuración.");

builder.Services.AddDbContextFactory<PetCareDbContext>(
    options => options.UseSqlServer(connection));

// ==========================================
// 2. SERVICIOS DE LA APLICACIÓN
// ==========================================

builder.Services.AddScoped<UsuarioService>();

builder.Services.AddScoped<DashboardService>();

builder.Services.AddScoped<CitaVeterinariaService>();
builder.Services.AddScoped<HospitalizacionService>();
builder.Services.AddScoped<FarmaciaService>();
builder.Services.AddScoped<FacturaService>();

builder.Services.AddScoped<IPasswordHasher<Usuario>,
    PasswordHasher<Usuario>>();

// Servicios de seguridad.
builder.Services.AddScoped<SessionValidator>();

builder.Services.AddScoped<PetCareCookieEvents>();

builder.Services.AddScoped<PermisosService>();

// Control de intentos de inicio de sesión.
builder.Services.AddMemoryCache();

builder.Services.AddSingleton<IntentosLoginService>();

// ==========================================
// 3. LIMITACIÓN DE SOLICITUDES
// ==========================================

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.AddPolicy("LimiteLogin", context =>
    {
        string ip = context.Connection
            .RemoteIpAddress?.ToString()
            ?? "desconocida";

        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: ip,
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType =
            "text/plain; charset=utf-8";

        if (context.Lease.TryGetMetadata(
            MetadataName.RetryAfter, out var espera))
        {
            context.HttpContext.Response.Headers["Retry-After"] =
                Math.Ceiling(espera.TotalSeconds).ToString(
                    System.Globalization.CultureInfo.InvariantCulture);
        }

        await context.HttpContext.Response.WriteAsync(
            "Demasiadas solicitudes. " +
            "Espera un momento antes de volver a intentarlo.",
            cancellationToken);
    };
});

// ==========================================
// 4. AUTENTICACIÓN MEDIANTE COOKIES
// ==========================================

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "PetCare.Auth";

        options.Cookie.HttpOnly = true;

        options.Cookie.SecurePolicy =
            CookieSecurePolicy.Always;

        options.Cookie.SameSite =
            SameSiteMode.Lax;

        options.LoginPath = "/login";

        options.AccessDeniedPath = "/acceso-denegado";

        options.ExpireTimeSpan = TimeSpan.FromHours(8);

        options.SlidingExpiration = true;

        options.EventsType =
            typeof(PetCareCookieEvents);
    });

// ==========================================
// 5. AUTORIZACIÓN POR ROLES
// ==========================================

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SoloAdministrador", policy =>
        policy.RequireRole(
            nameof(RolUsuario.Administrador)));

    options.AddPolicy("PersonalAutorizado", policy =>
        policy.RequireRole(
            nameof(RolUsuario.Administrador),
            nameof(RolUsuario.Recepcionista)));
});

builder.Services.AddCascadingAuthenticationState();

// ==========================================
// 6. CONFIGURACIÓN DE BLAZOR
// ==========================================

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<AuthenticationStateProvider,
    PetCareAuthenticationStateProvider>();

var app = builder.Build();

// ==========================================
// 7. CONFIGURACIÓN DEL SERVIDOR
// ==========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Activar la limitación de solicitudes.
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapAuthEndpoints();
app.MapPetCareEndpoints();

app.MapRazorComponents<PetCare.Web.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
