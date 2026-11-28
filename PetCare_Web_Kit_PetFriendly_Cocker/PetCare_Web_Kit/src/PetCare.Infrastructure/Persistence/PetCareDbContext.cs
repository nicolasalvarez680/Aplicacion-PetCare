using Microsoft.EntityFrameworkCore;
using PetCare.Domain.Entities;
using PetCare.Domain.Enums;
namespace PetCare.Infrastructure.Persistence;
public class PetCareDbContext(DbContextOptions<PetCareDbContext> options) : DbContext(options)
{
    public DbSet<ServicioVeterinario> Servicios => Set<ServicioVeterinario>();
    public DbSet<ProductoFarmacia> Productos => Set<ProductoFarmacia>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<LineaFactura> LineasFactura => Set<LineaFactura>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<ServicioVeterinario>().ToTable("Servicios");
        b.Entity<ServicioVeterinario>().HasDiscriminator<string>("Tipo")
            .HasValue<CitaVeterinaria>("Cita").HasValue<Hospitalizacion>("Hospitalizacion");
        b.Entity<ServicioVeterinario>().HasKey(x => x.Id);
        b.Entity<ServicioVeterinario>().Property(x => x.Codigo).HasMaxLength(20).IsRequired();
        b.Entity<ServicioVeterinario>().HasIndex(x => x.Codigo).IsUnique();
        b.Entity<ServicioVeterinario>().Property(x => x.NombreMascota).HasMaxLength(80).IsRequired();
        b.Entity<ServicioVeterinario>().Property(x => x.TipoMascota).HasMaxLength(10);
        b.Entity<ServicioVeterinario>().Property(x => x.RazaMascota).HasMaxLength(80);

        b.Entity<ProductoFarmacia>().ToTable("Productos");
        b.Entity<ProductoFarmacia>().HasDiscriminator<string>("Tipo")
            .HasValue<LoteMedicamento>("LoteMedicamento");
        b.Entity<ProductoFarmacia>().HasKey(x => x.Id);
        b.Entity<ProductoFarmacia>().Property(x => x.Codigo).HasMaxLength(20).IsRequired();
        b.Entity<ProductoFarmacia>().HasIndex(x => x.Codigo).IsUnique();
        b.Entity<ProductoFarmacia>().Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        b.Entity<ProductoFarmacia>().Property(x => x.PrecioUnidad).HasColumnType("decimal(18,2)");

        b.Entity<LoteMedicamento>().Property(x => x.FechaVencimiento).HasColumnType("date");
        b.Entity<Hospitalizacion>().Property(x => x.TarifaDiaria).HasPrecision(18, 2);
        b.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios"); e.HasKey(x => x.Id);
            e.Property(x => x.NombreUsuario).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.NombreUsuario).IsUnique();
            e.Property(x => x.NombreCompleto).HasMaxLength(100).IsRequired();
            e.Property(x => x.ContrasenaHash).HasMaxLength(200).IsRequired();
            e.Property(x => x.Rol).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.Ignore(x => x.EsAdministrador);
        });

        b.Entity<Factura>(e =>
        {
            e.ToTable("Facturas"); e.HasKey(x => x.Id);
            e.Property(x => x.Numero).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.Numero).IsUnique();
            e.Property(x => x.NombreCliente).HasMaxLength(100).IsRequired();
            e.Property(x => x.EstaAnulada).HasDefaultValue(false);
            e.Property(x => x.InventarioDescontado).HasDefaultValue(false);
            e.Ignore(x => x.Total);
            e.HasOne(x => x.UsuarioEmisor).WithMany().HasForeignKey(x => x.UsuarioEmisorId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Lineas).WithOne().HasForeignKey("FacturaId").OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.Lineas).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<LineaFactura>(e =>
        {
            e.ToTable("LineasFactura", t => t.HasCheckConstraint("CK_LineasFactura_UnOrigen",
                "([ServicioId] IS NOT NULL AND [ProductoId] IS NULL) OR ([ServicioId] IS NULL AND [ProductoId] IS NOT NULL)"));
            e.HasKey(x => x.Id);
            e.Property(x => x.PrecioUnitario).HasPrecision(18, 2);
            e.Ignore(x => x.Item); e.Ignore(x => x.Subtotal); e.Ignore(x => x.Descripcion);
            e.HasOne(x => x.Servicio).WithMany().HasForeignKey(x => x.ServicioId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
