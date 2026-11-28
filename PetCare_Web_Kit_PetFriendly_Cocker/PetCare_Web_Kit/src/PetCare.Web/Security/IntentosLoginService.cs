using Microsoft.Extensions.Caching.Memory;

namespace PetCare.Web.Security;

public class IntentosLoginService
{
    private const int MaximoIntentos = 5;

    private static readonly TimeSpan TiempoBloqueo =
        TimeSpan.FromMinutes(5);

    private static readonly TimeSpan VigenciaIntentos =
        TimeSpan.FromMinutes(15);

    private readonly IMemoryCache _cache;

    private readonly object _sincronizacion = new();

    public IntentosLoginService(IMemoryCache cache)
    {
        _cache = cache;
    }

    private static string ObtenerClave(int usuarioId)
    {
        return $"PetCare.Login.Intentos:{usuarioId}";
    }

    public bool EstaBloqueado(int usuarioId)
    {
        lock (_sincronizacion)
        {
            string clave = ObtenerClave(usuarioId);

            if (!_cache.TryGetValue<EstadoIntentos>(
                clave, out var estado) || estado == null)
            {
                return false;
            }

            if (estado.BloqueadoHasta == null)
            {
                return false;
            }

            if (estado.BloqueadoHasta.Value >
                DateTimeOffset.UtcNow)
            {
                return true;
            }

            _cache.Remove(clave);

            return false;
        }
    }

    public void RegistrarFallo(int usuarioId)
    {
        lock (_sincronizacion)
        {
            string clave = ObtenerClave(usuarioId);

            var ahora = DateTimeOffset.UtcNow;

            if (!_cache.TryGetValue<EstadoIntentos>(
                clave, out var estado) ||
                estado == null)
            {
                estado = new EstadoIntentos();
            }

            if (estado.BloqueadoHasta.HasValue)
            {
                if (estado.BloqueadoHasta.Value > ahora)
                {
                    return;
                }

                estado = new EstadoIntentos();
            }

            estado.Cantidad++;

            if (estado.Cantidad >= MaximoIntentos)
            {
                estado.BloqueadoHasta =
                    ahora.Add(TiempoBloqueo);

                _cache.Set(
                    clave,
                    estado,
                    TiempoBloqueo);
            }
            else
            {
                _cache.Set(
                    clave,
                    estado,
                    VigenciaIntentos);
            }
        }
    }

    public bool RegistrarExito(int usuarioId)
    {
        lock (_sincronizacion)
        {
            if (EstaBloqueado(usuarioId))
            {
                return false;
            }

            _cache.Remove(ObtenerClave(usuarioId));

            return true;
        }
    }

    private class EstadoIntentos
    {
        public int Cantidad { get; set; }

        public DateTimeOffset? BloqueadoHasta { get; set; }
    }
}
