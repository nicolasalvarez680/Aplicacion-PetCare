using PetCare.Domain;

namespace PetCare.Domain.Entities;

public class CitaVeterinaria : ServicioVeterinario
{
    public int DuracionMinutos { get; private set; } = 30;

    public string? VeterinarioAsignado { get; private set; }

    private CitaVeterinaria()
    {
    }

    public CitaVeterinaria(
        string codigo,
        string mascota,
        DateTime fecha,
        string tipoMascota,
        string? razaMascota)
        : base(codigo, mascota, fecha, tipoMascota, razaMascota)
    {
    }

    public bool EstablecerDuracion(int minutos)
    {
        if (minutos is < 15 or > 180)
        {
            return false;
        }

        DuracionMinutos = minutos;

        return true;
    }

    public bool Confirmar(string veterinario)
    {
        if (string.IsNullOrWhiteSpace(veterinario) ||
            VeterinarioAsignado is not null)
        {
            return false;
        }

        veterinario = veterinario.Trim();

        if (veterinario.Length > 100)
        {
            return false;
        }

        VeterinarioAsignado = veterinario;

        return true;
    }

    public void ActualizarDatos(
        string codigo,
        string mascota,
        DateTime fecha,
        int duracionMinutos,
        string tipoMascota,
        string? razaMascota)
    {
        if (duracionMinutos is < 15 or > 180)
        {
            throw new ArgumentException(
                "La duración debe estar entre 15 y 180 minutos.");
        }

        // Reutilizar el comportamiento de la clase base.
        ActualizarDatosGenerales(
            codigo,
            mascota,
            fecha, tipoMascota, razaMascota);

        EstablecerDuracion(duracionMinutos);
    }

    public int CalcularMinutosParaLaCita()
    {
        return Math.Max(
            0,
            (int)Math.Ceiling(
                (Fecha - DateTime.Now).TotalMinutes));
    }

    public override decimal ObtenerPrecioUnitario()
    {
        return 60000m +
            Math.Max(0, DuracionMinutos - 30) * 1500m;
    }

    public override string ObtenerDescripcion()
    {
        return $"Cita veterinaria para {NombreMascota}";
    }

    public override void ValidarFacturable(int cantidad)
    {
        base.ValidarFacturable(cantidad);

        if (VeterinarioAsignado is null)
        {
            throw new ReglaNegocioException(
                "La cita debe estar confirmada.");
        }
    }
}
