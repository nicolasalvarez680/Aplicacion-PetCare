using PetCare.Domain;
using PetCare.Domain.Interfaces;
namespace PetCare.Domain.Entities;
public abstract class ProductoFarmacia : IFacturable
{
    public int Id { get; private set; }
    public string Codigo { get; private set; } = null!;
    public string Nombre { get; private set; } = null!;
    public decimal PrecioUnidad { get; private set; }
    protected ProductoFarmacia() { }
    protected ProductoFarmacia(string codigo, string nombre, decimal precio)
    {
        ActualizarDatosProducto(codigo, nombre, precio);
    }
    protected void ActualizarDatosProducto(string codigo, string nombre, decimal precio)
    {
        if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("Código y nombre obligatorios.");
        codigo = codigo.Trim(); nombre = nombre.Trim();
        if (codigo.Length > 20 || nombre.Length > 100) throw new ArgumentException("Código o nombre demasiado largo.");
        if (precio > 9999999999999999.99m || decimal.Round(precio, 2) != precio) throw new ArgumentException("Precio inválido; máximo dos decimales.");
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Código obligatorio.");
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("Nombre obligatorio.");
        if (precio <= 0) throw new ArgumentOutOfRangeException(nameof(precio));
        Codigo = codigo; Nombre = nombre; PrecioUnidad = precio;
    }
    public abstract string ObtenerDescripcion();
    public decimal ObtenerPrecioUnitario() => PrecioUnidad;
    public virtual void ValidarFacturable(int cantidad)
    {
        if (cantidad <= 0) throw new ArgumentOutOfRangeException(nameof(cantidad));
        if (PrecioUnidad <= 0) throw new ReglaNegocioException("Precio inválido.");
    }
}
