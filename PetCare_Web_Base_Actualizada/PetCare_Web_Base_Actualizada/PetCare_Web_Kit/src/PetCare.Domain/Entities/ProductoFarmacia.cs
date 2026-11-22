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
        if (PrecioUnidad <= 0) throw new InvalidOperationException("Precio inválido.");
    }
}
