namespace PetCare.Domain.Entities;
public class CitaVeterinaria : ServicioVeterinario
{
    public int DuracionMinutos { get; private set; } = 30;
    public string? VeterinarioAsignado { get; private set; }
    private CitaVeterinaria() { }
    public CitaVeterinaria(string codigo, string mascota, DateTime fecha) : base(codigo, mascota, fecha) { }
    public bool EstablecerDuracion(int minutos)
    {
        if (minutos is < 15 or > 180) return false;
        DuracionMinutos = minutos; return true;
    }
    public bool Confirmar(string veterinario)
    {
        if (string.IsNullOrWhiteSpace(veterinario) || VeterinarioAsignado is not null) return false;
        VeterinarioAsignado = veterinario; return true;
    }
    public int CalcularMinutosParaLaCita() => Math.Max(0, (int)Math.Ceiling((Fecha - DateTime.Now).TotalMinutes));
    public override decimal ObtenerPrecioUnitario() => 60000m + Math.Max(0, DuracionMinutos - 30) * 1500m;
    public override string ObtenerDescripcion() => $"Cita veterinaria para {NombreMascota}";
    public override void ValidarFacturable(int cantidad)
    {
        base.ValidarFacturable(cantidad);
        if (VeterinarioAsignado is null) throw new InvalidOperationException("La cita debe estar confirmada.");
    }
}
