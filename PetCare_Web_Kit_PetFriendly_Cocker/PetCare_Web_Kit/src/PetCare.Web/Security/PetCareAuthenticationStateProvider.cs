using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace PetCare.Web.Security;

public class PetCareAuthenticationStateProvider
    : RevalidatingServerAuthenticationStateProvider
{
    private readonly IServiceScopeFactory _scopeFactory;

    public PetCareAuthenticationStateProvider(
        ILoggerFactory loggerFactory,
        IServiceScopeFactory scopeFactory)
        : base(loggerFactory)
    {
        _scopeFactory = scopeFactory;
    }

    // Intervalo de comprobación de la sesión.
    protected override TimeSpan RevalidationInterval
        => TimeSpan.FromSeconds(30);

    protected override async Task<bool>
        ValidateAuthenticationStateAsync(
            AuthenticationState authenticationState,
            CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using var scope =
            _scopeFactory.CreateAsyncScope();

        var validador = scope.ServiceProvider
            .GetRequiredService<SessionValidator>();

        bool sesionValida =
            await validador.EsSesionValidaAsync(
                authenticationState.User);

        cancellationToken.ThrowIfCancellationRequested();

        return sesionValida;
    }
}
