using PetCare.Domain;
using System.Globalization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PetCare.Domain.Entities;
using PetCare.Domain.Enums;
using PetCare.Infrastructure.Services;

namespace PetCare.Web.Security;

public static class PetCareEndpoints
{
    public static void MapPetCareEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/registro", async (HttpContext ctx, IAntiforgery anti, UsuarioService usuarios, IPasswordHasher<Usuario> hasher, ILoggerFactory logger) =>
        {
            if (ctx.User.Identity?.IsAuthenticated == true) return Results.Redirect("/dashboard");
            return await Procesar(ctx, anti, logger, "/registro", async f => {
                var nombre = Texto(f, "nombreCompleto", 100);
                var usuario = Texto(f, "nombreUsuario", 50);
                var clave = f["contrasena"].ToString();
                if (clave.Length is < 12 or > 128 || string.IsNullOrWhiteSpace(clave) || clave != f["confirmarContrasena"].ToString()) throw new ArgumentException("Las contraseñas deben coincidir y tener entre 12 y 128 caracteres.");
                var rolTexto = f["rol"].ToString();
                if (rolTexto != nameof(RolUsuario.Administrador) && rolTexto != nameof(RolUsuario.Recepcionista)) throw new ArgumentException("Rol inválido.");
                var rol = Enum.Parse<RolUsuario>(rolTexto);
                var cuenta = new Usuario(usuario, nombre, rol, "Pendiente");
                cuenta.CambiarContrasenaHash(hasher.HashPassword(cuenta, clave));
                await usuarios.RegistrarCuentaAsync(cuenta);
                return "/login?registrado=1";
            });
        }).AllowAnonymous().RequireRateLimiting("LimiteLogin");

        var citas = app.MapGroup("/operaciones/citas").RequireAuthorization("PersonalAutorizado");
        citas.MapPost("/nueva", (HttpContext c, IAntiforgery a, ILoggerFactory log, CitaVeterinariaService s) => Procesar(c,a,log,"/citas/nueva",async f => {
            await s.CrearAsync(Texto(f,"codigo",20),Texto(f,"mascota",80),FechaHora(f,"fecha"),Entero(f,"duracion",15,180),Texto(f,"tipoMascota",10),f["razaMascota"].ToString(),c.User); return "/citas?ok=1";
        }));
        citas.MapPost("/{id:int}/editar", (int id,HttpContext c, IAntiforgery a, ILoggerFactory log, CitaVeterinariaService s) => Procesar(c,a,log,$"/citas/editar/{id}",async f => {
            await s.ActualizarAsync(id,Texto(f,"codigo",20),Texto(f,"mascota",80),FechaHora(f,"fecha"),Entero(f,"duracion",15,180),Texto(f,"tipoMascota",10),f["razaMascota"].ToString(),c.User); return $"/citas/editar/{id}?ok=1";
        }));
        citas.MapPost("/{id:int}/confirmar", (int id,HttpContext c, IAntiforgery a, ILoggerFactory log, CitaVeterinariaService s) => Procesar(c,a,log,$"/citas/editar/{id}",async f => {
            await s.ConfirmarAsync(id,Texto(f,"veterinario",100),c.User); return $"/citas/editar/{id}?ok=1";
        }));
        citas.MapPost("/{id:int}/eliminar", (int id,HttpContext c, IAntiforgery a, ILoggerFactory log, CitaVeterinariaService s) => Procesar(c,a,log,$"/citas/editar/{id}",async f => {
            Confirmar(f); await s.EliminarAsync(id,c.User); return "/citas?ok=1";
        }));
        var usuariosAdmin = app.MapGroup("/operaciones/usuarios").RequireAuthorization("SoloAdministrador");
        usuariosAdmin.MapPost("/nuevo", (HttpContext c, IAntiforgery a, ILoggerFactory log, UsuarioService s, IPasswordHasher<Usuario> hasher) => Procesar(c,a,log,"/usuarios/nuevo",async f => {
            var clave=f["contrasena"].ToString();
            if(clave.Length is < 12 or > 128 || string.IsNullOrWhiteSpace(clave) || clave!=f["confirmarContrasena"].ToString()) throw new ArgumentException("Las contraseñas deben coincidir y tener entre 12 y 128 caracteres.");
            var usuario=new Usuario(Texto(f,"nombreUsuario",50),Texto(f,"nombreCompleto",100),Rol(f),"Pendiente");
            usuario.CambiarContrasenaHash(hasher.HashPassword(usuario,clave));
            await s.CrearAsync(usuario,c.User); return "/usuarios?ok=1";
        }));
        usuariosAdmin.MapPost("/{id:int}/editar", (int id,HttpContext c, IAntiforgery a, ILoggerFactory log, UsuarioService s) => Procesar(c,a,log,$"/usuarios/editar/{id}",async f => {
            await s.ActualizarAsync(id,Texto(f,"nombreUsuario",50),Texto(f,"nombreCompleto",100),Rol(f),c.User); return "/usuarios?ok=1";
        }));
        usuariosAdmin.MapPost("/{id:int}/eliminar", (int id,HttpContext c, IAntiforgery a, ILoggerFactory log, UsuarioService s) => Procesar(c,a,log,$"/usuarios/editar/{id}",async f => {
            Confirmar(f); await s.EliminarAsync(id,c.User); return "/usuarios?ok=1";
        }));

        var h = app.MapGroup("/operaciones/hospitalizaciones").RequireAuthorization("PersonalAutorizado");
        h.MapPost("/nueva", (HttpContext c, IAntiforgery a, ILoggerFactory log, HospitalizacionService s) => Procesar(c,a,log,"/hospitalizaciones/nueva",async f => {
            await s.CrearAsync(Texto(f,"codigo",20),Texto(f,"mascota",80),FechaHora(f,"ingreso"),Dinero(f,"tarifa"),Texto(f,"tipoMascota",10),f["razaMascota"].ToString(),c.User); return "/hospitalizaciones?ok=1";
        }));
        h.MapPost("/{id:int}/editar", (int id,HttpContext c, IAntiforgery a, ILoggerFactory log, HospitalizacionService s) => Procesar(c,a,log,$"/hospitalizaciones/editar/{id}",async f => {
            await s.ActualizarAsync(id,Texto(f,"codigo",20),Texto(f,"mascota",80),FechaHora(f,"ingreso"),Dinero(f,"tarifa"),Texto(f,"tipoMascota",10),f["razaMascota"].ToString(),c.User); return $"/hospitalizaciones/editar/{id}?ok=1";
        }));
        h.MapPost("/{id:int}/alta", (int id,HttpContext c, IAntiforgery a, ILoggerFactory log, HospitalizacionService s) => Procesar(c,a,log,$"/hospitalizaciones/editar/{id}",async f => {
            await s.DarAltaAsync(id,FechaHora(f,"alta"),c.User); return $"/hospitalizaciones/editar/{id}?ok=1";
        }));
        h.MapPost("/{id:int}/eliminar", (int id,HttpContext c, IAntiforgery a, ILoggerFactory log, HospitalizacionService s) => Procesar(c,a,log,$"/hospitalizaciones/editar/{id}",async f => {
            Confirmar(f); await s.EliminarAsync(id,c.User); return "/hospitalizaciones?ok=1";
        }));

        var farmacia = app.MapGroup("/operaciones/farmacia").RequireAuthorization("PersonalAutorizado");
        farmacia.MapPost("/nuevo", (HttpContext c, IAntiforgery a, ILoggerFactory log, FarmaciaService s) => Procesar(c,a,log,"/farmacia/nuevo",async f => {
            await s.CrearAsync(Texto(f,"codigo",20),Texto(f,"nombre",100),Fecha(f,"vencimiento"),Dinero(f,"precio"),Entero(f,"existencias",0,1000000),c.User); return "/farmacia?ok=1";
        }));
        farmacia.MapPost("/{id:int}/editar", (int id,HttpContext c, IAntiforgery a, ILoggerFactory log, FarmaciaService s) => Procesar(c,a,log,$"/farmacia/editar/{id}",async f => {
            await s.ActualizarAsync(id,Texto(f,"codigo",20),Texto(f,"nombre",100),Fecha(f,"vencimiento"),Dinero(f,"precio"),c.User); return $"/farmacia/editar/{id}?ok=1";
        }));
        farmacia.MapPost("/{id:int}/reponer", (int id,HttpContext c, IAntiforgery a, ILoggerFactory log, FarmaciaService s) => Procesar(c,a,log,$"/farmacia/editar/{id}",async f => {
            await s.ReponerAsync(id,Entero(f,"cantidad",1,1000000),c.User); return $"/farmacia/editar/{id}?ok=1";
        }));
        farmacia.MapPost("/{id:int}/eliminar", (int id,HttpContext c, IAntiforgery a, ILoggerFactory log, FarmaciaService s) => Procesar(c,a,log,$"/farmacia/editar/{id}",async f => {
            Confirmar(f); await s.EliminarAsync(id,c.User); return "/farmacia?ok=1";
        }));
        var facturas = app.MapGroup("/operaciones/facturas").RequireAuthorization("PersonalAutorizado");
        facturas.MapPost("/nueva", (HttpContext c, IAntiforgery a, ILoggerFactory log, FacturaService s) => Procesar(c,a,log,"/facturas/nueva",async f => {
            var servicios = f["servicio"].Select(v => IdValido(v)).ToArray();
            var productos = new Dictionary<int,int>();
            foreach (var item in f.Where(x => x.Key.StartsWith("producto_", StringComparison.Ordinal)))
            {
                int id = IdValido(item.Key[9..]);
                if (string.IsNullOrWhiteSpace(item.Value.ToString())) continue;
                int cantidad = Entero(f,item.Key,0,1000000);
                if (cantidad > 0) productos.Add(id,cantidad);
            }
            int facturaId = await s.CrearAsync(Texto(f,"numero",20),Texto(f,"cliente",100),servicios,productos,c.User);
            return $"/facturas/{facturaId}?ok=1";
        }));
        facturas.MapPost("/{id:int}/anular", (int id,HttpContext c, IAntiforgery a, ILoggerFactory log, FacturaService s) => Procesar(c,a,log,$"/facturas/{id}",async f => {
            Confirmar(f); await s.AnularAsync(id,c.User); return $"/facturas/{id}?ok=1";
        })).RequireAuthorization("SoloAdministrador");
    }

    private static async Task<IResult> Procesar(HttpContext ctx, IAntiforgery anti, ILoggerFactory logger, string errorPath, Func<IFormCollection,Task<string>> accion)
    {
        if (!ctx.Request.HasFormContentType || !await anti.IsRequestValidAsync(ctx)) return Results.BadRequest("Solicitud no válida. Recarga la página.");
        try { return Results.Redirect(await accion(await ctx.Request.ReadFormAsync())); }
        catch (UnauthorizedAccessException) { return Results.Redirect("/acceso-denegado"); }
        catch (ArgumentException ex) { return Error(errorPath,ex.Message); }
        catch (ReglaNegocioException ex) { logger.CreateLogger("PetCare.Operaciones").LogWarning(ex,"Operación rechazada"); return Error(errorPath,ex.Message); }
        catch (DbUpdateException ex) { logger.CreateLogger("PetCare.Operaciones").LogError(ex,"Error de persistencia"); return Error(errorPath,"No se pudo guardar. Comprueba los datos, códigos duplicados o registros asociados."); }
        catch (Exception ex) { logger.CreateLogger("PetCare.Operaciones").LogError(ex,"Error de operación"); return Error(errorPath,"No fue posible completar la operación. Intenta de nuevo."); }
    }
    private static IResult Error(string path,string error) => Results.Redirect(path+"?error="+Uri.EscapeDataString(error));
    private static string Texto(IFormCollection f,string campo,int max) { var v=f[campo].ToString().Trim(); if(v.Length==0 || v.Length>max) throw new ArgumentException($"Campo {campo} obligatorio, máximo {max} caracteres."); return v; }
    private static DateTime FechaHora(IFormCollection f,string campo) => DateTime.TryParseExact(f[campo].ToString(),new[]{"yyyy-MM-ddTHH:mm","yyyy-MM-ddTHH:mm:ss"},CultureInfo.InvariantCulture,DateTimeStyles.None,out var v) ? v : throw new ArgumentException("Fecha y hora inválidas.");
    private static DateOnly Fecha(IFormCollection f,string campo) => DateOnly.TryParseExact(f[campo].ToString(),"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out var v) ? v : throw new ArgumentException("Fecha inválida.");
    private static decimal Dinero(IFormCollection f,string campo) => decimal.TryParse(f[campo].ToString(),NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out var v) && v>0 && v<=9999999999999999.99m && decimal.Round(v,2)==v ? v : throw new ArgumentException("Importe inválido; máximo dos decimales.");
    private static int Entero(IFormCollection f,string campo,int min,int max) => int.TryParse(f[campo].ToString(),NumberStyles.None,CultureInfo.InvariantCulture,out var v) && v>=min && v<=max ? v : throw new ArgumentException("Cantidad inválida.");
    private static int IdValido(string? s) => int.TryParse(s,NumberStyles.None,CultureInfo.InvariantCulture,out var v) && v>0 ? v : throw new ArgumentException("Identificador inválido.");
    private static RolUsuario Rol(IFormCollection f) => f["rol"].ToString() switch { "Administrador" => RolUsuario.Administrador, "Recepcionista" => RolUsuario.Recepcionista, _ => throw new ArgumentException("Rol inválido.") };
    private static void Confirmar(IFormCollection f) { if(f["confirmar"]!="si") throw new ArgumentException("Debes confirmar la operación."); }
}
