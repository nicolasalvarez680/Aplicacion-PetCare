using PetCare.Domain;

using PetCare.Domain.Interfaces;

namespace PetCare.Domain.Entities;

public abstract class ServicioVeterinario : IFacturable
{
    public int Id { get; private set; }

    public string Codigo { get; private set; } = null!;

    public string NombreMascota { get; private set; } = null!;

    public DateTime Fecha { get; private set; }

    // Nulos únicamente para registros anteriores a la migración.
    public string? TipoMascota { get; private set; }
    public string? RazaMascota { get; private set; }

    protected ServicioVeterinario()
    {
    }

    protected ServicioVeterinario(
        string codigo,
        string nombreMascota,
        DateTime fecha,
        string tipoMascota,
        string? razaMascota)
    {
        ActualizarDatosGenerales(
            codigo,
            nombreMascota,
            fecha, tipoMascota, razaMascota);
    }

    // Permitir que las clases derivadas modifiquen
    // los datos generales de manera controlada.
    protected void ActualizarDatosGenerales(
        string codigo,
        string nombreMascota,
        DateTime fecha,
        string tipoMascota,
        string? razaMascota)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException(
                "El código es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(nombreMascota))
        {
            throw new ArgumentException(
                "El nombre de la mascota es obligatorio.");
        }

        codigo = codigo.Trim();
        nombreMascota = nombreMascota.Trim();

        if (codigo.Length > 20)
        {
            throw new ArgumentException(
                "El código no puede superar los 20 caracteres.");
        }

        if (nombreMascota.Length > 80)
        {
            throw new ArgumentException(
                "El nombre de la mascota no puede superar los 80 caracteres.");
        }

        if (fecha == default)
        {
            throw new ArgumentException(
                "Debes indicar una fecha válida.");
        }

        if (tipoMascota != "Perro" && tipoMascota != "Gato")
            throw new ArgumentException("Selecciona Perro o Gato como tipo de mascota.");

        razaMascota = razaMascota?.Trim();
        if (tipoMascota == "Perro" && (string.IsNullOrWhiteSpace(razaMascota) || razaMascota.Length > 80))
            throw new ArgumentException("Indica la raza del perro (máximo 80 caracteres). Si no se conoce, escribe Mestizo o No identificada.");

        TipoMascota = tipoMascota;
        RazaMascota = tipoMascota == "Perro" ? razaMascota : null;
        Codigo = codigo;
        NombreMascota = nombreMascota;
        Fecha = fecha;
    }

    public abstract decimal ObtenerPrecioUnitario();

    public abstract string ObtenerDescripcion();

    public virtual void ValidarFacturable(int cantidad)
    {
        if (cantidad != 1)
        {
            throw new ReglaNegocioException(
                "Un servicio veterinario se factura con cantidad 1.");
        }
    }
}
