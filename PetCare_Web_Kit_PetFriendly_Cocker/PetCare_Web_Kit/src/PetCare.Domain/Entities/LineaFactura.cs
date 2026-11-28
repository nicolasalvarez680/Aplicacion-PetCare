using PetCare.Domain.Interfaces;
namespace PetCare.Domain.Entities;
public class LineaFactura
{
    public int Id { get; private set; }
    public int Cantidad { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public int? ServicioId { get; private set; }
    public ServicioVeterinario? Servicio { get; private set; }
    public int? ProductoId { get; private set; }
    public ProductoFarmacia? Producto { get; private set; }
    public IFacturable Item => (IFacturable?)Servicio ?? Producto!;
    public decimal Subtotal => Cantidad * PrecioUnitario;
    public string Descripcion => Item.ObtenerDescripcion();
    private LineaFactura() { }
    internal LineaFactura(ServicioVeterinario servicio)
    {
        servicio.ValidarFacturable(1); Servicio = servicio; Cantidad = 1; PrecioUnitario = servicio.ObtenerPrecioUnitario();
    }
    internal LineaFactura(ProductoFarmacia producto, int cantidad)
    {
        producto.ValidarFacturable(cantidad); Producto = producto; Cantidad = cantidad; PrecioUnitario = producto.ObtenerPrecioUnitario();
    }
}
