namespace PetCare.Domain.Entities;
public class Hospitalizacion : ServicioVeterinario
{
    public decimal TarifaDiaria { get; private set; }
    public DateTime? FechaAlta { get; private set; }
    public bool EstaHospitalizado => FechaAlta is null;
    private Hospitalizacion() { }
    public Hospitalizacion(string codigo, string mascota, DateTime ingreso, decimal tarifaDiaria) : base(codigo, mascota, ingreso)
    {
        if (tarifaDiaria <= 0) throw new ArgumentOutOfRangeException(nameof(tarifaDiaria));
        TarifaDiaria = tarifaDiaria;
    }
    public bool DarAlta(DateTime fechaAlta)
    {
        if (!EstaHospitalizado || fechaAlta.Date < Fecha.Date) return false;
        FechaAlta = fechaAlta; return true;
    }
    public int CalcularDias()
    {
        var fin = (FechaAlta ?? DateTime.Today).Date;
        return Math.Max(1, (fin - Fecha.Date).Days + 1);
    }
    public override decimal ObtenerPrecioUnitario() => CalcularDias() * TarifaDiaria;
    public override string ObtenerDescripcion() => $"Hospitalización de {NombreMascota}";
}
