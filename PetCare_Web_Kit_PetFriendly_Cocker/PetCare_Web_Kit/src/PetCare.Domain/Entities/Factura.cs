using PetCare.Domain;
namespace PetCare.Domain.Entities;
public class Factura
{
    public int Id { get; private set; }
    public string Numero { get; private set; } = null!;
    public DateTime FechaEmision { get; private set; }
    public string NombreCliente { get; private set; } = null!;
    public bool EstaAnulada { get; private set; }
    public bool InventarioDescontado { get; private set; }
    public int UsuarioEmisorId { get; private set; }
    public Usuario UsuarioEmisor { get; private set; } = null!;
    private readonly List<LineaFactura> _lineas = new();
    public IReadOnlyCollection<LineaFactura> Lineas => _lineas.AsReadOnly();
    public decimal Total => _lineas.Sum(x => x.Subtotal);
    private Factura() { }
    public Factura(string numero, DateTime fecha, string cliente, Usuario emisor)
    {
        if (string.IsNullOrWhiteSpace(numero) || string.IsNullOrWhiteSpace(cliente)) throw new ArgumentException("Número y cliente obligatorios.");
        numero = numero.Trim(); cliente = cliente.Trim();
        if (numero.Length > 20 || cliente.Length > 100) throw new ArgumentException("Número o cliente demasiado largo.");
        if (string.IsNullOrWhiteSpace(numero)) throw new ArgumentException("Número obligatorio.");
        if (string.IsNullOrWhiteSpace(cliente)) throw new ArgumentException("Cliente obligatorio.");
        Numero = numero; FechaEmision = fecha; NombreCliente = cliente;
        UsuarioEmisor = emisor ?? throw new ArgumentNullException(nameof(emisor));
    }
    public void AgregarLinea(ServicioVeterinario servicio)
    {
        AsegurarActiva(); _lineas.Add(new LineaFactura(servicio));
    }
    public void AgregarLinea(ProductoFarmacia producto, int cantidad)
    {
        AsegurarActiva(); _lineas.Add(new LineaFactura(producto, cantidad));
    }
    public void RegistrarDescuentoInventario()
    { AsegurarActiva(); InventarioDescontado = true; }
    public void Anular(Usuario usuario)
    {
        if (usuario is null || !usuario.EsAdministrador) throw new UnauthorizedAccessException("Solo administrador puede anular facturas.");
        if (EstaAnulada) throw new ReglaNegocioException("La factura ya está anulada.");
        EstaAnulada = true;
    }
    private void AsegurarActiva()
    {
        if (EstaAnulada) throw new ReglaNegocioException("No se puede modificar una factura anulada.");
    }
}
