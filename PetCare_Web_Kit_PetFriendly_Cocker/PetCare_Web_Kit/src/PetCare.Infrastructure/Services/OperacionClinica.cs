using System.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PetCare.Infrastructure.Persistence;

namespace PetCare.Infrastructure.Services;

// Todas las operaciones verifican la identidad vigente dentro de la transacción.
internal static class OperacionClinica
{
    internal static async Task<T> EjecutarAsync<T>(IDbContextFactory<PetCareDbContext> factory, ClaimsPrincipal identidad, Func<PetCareDbContext, Task<T>> operacion, bool soloAdmin = false)
    {
        await using var db = await factory.CreateDbContextAsync();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        await AutorizacionOperaciones.VerificarAsync(db, identidad, soloAdmin);
        T resultado = await operacion(db);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return resultado;
    }
}
