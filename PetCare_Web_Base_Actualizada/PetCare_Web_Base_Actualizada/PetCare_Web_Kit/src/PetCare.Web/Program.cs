using Microsoft.EntityFrameworkCore;
using PetCare.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
var connection = builder.Configuration.GetConnectionString("PetCareConnection")
    ?? throw new InvalidOperationException("Falta ConnectionStrings:PetCareConnection en la configuración.");

builder.Services.AddDbContextFactory<PetCareDbContext>(options => options.UseSqlServer(connection));
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<PetCare.Web.Components.App>().AddInteractiveServerRenderMode();
app.Run();
