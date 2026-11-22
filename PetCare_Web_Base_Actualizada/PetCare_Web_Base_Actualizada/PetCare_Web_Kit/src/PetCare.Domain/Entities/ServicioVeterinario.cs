using PetCare.Domain.Interfaces;
namespace PetCare.Domain.Entities;

public abstract class ServicioVeterinario : IFacturable
{
    public int Id { get; private set; }
    public string Codigo { get; private set; } = null!;
    public string NombreMascota { get; private set; } = null!;
    public DateTime Fecha { get; private set; }

    protected ServicioVeterinario() { }
    protected ServicioVeterinario(string codigo, string nombreMascota, DateTime fecha)
    {
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("El código es obligatorio.");
        if (string.IsNullOrWhiteSpace(nombreMascota)) throw new ArgumentException("La mascota es obligatoria.");
        Codigo = codigo; NombreMascota = nombreMascota; Fecha = fecha;
    }
    public abstract decimal ObtenerPrecioUnitario();
    public abstract string ObtenerDescripcion();
    public virtual void ValidarFacturable(int cantidad)
    {
        if (cantidad != 1) throw new InvalidOperationException("Un servicio veterinario se factura con cantidad 1.");
    }
}
