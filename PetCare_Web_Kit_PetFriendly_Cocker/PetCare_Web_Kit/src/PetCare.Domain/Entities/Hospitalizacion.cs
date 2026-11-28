using PetCare.Domain;
namespace PetCare.Domain.Entities;
public class Hospitalizacion : ServicioVeterinario
{
    public decimal TarifaDiaria { get; private set; }
    public DateTime? FechaAlta { get; private set; }
    public bool EstaHospitalizado => FechaAlta is null;
    private Hospitalizacion() { }
    public Hospitalizacion(string codigo, string mascota, DateTime ingreso, decimal tarifaDiaria, string tipoMascota, string? razaMascota) : base(codigo, mascota, ingreso, tipoMascota, razaMascota)
    { Validar(ingreso, tarifaDiaria); TarifaDiaria = tarifaDiaria; }
    private static void Validar(DateTime ingreso, decimal tarifa)
    {
        if (ingreso == default || ingreso > DateTime.Now) throw new ArgumentException("Fecha de ingreso inválida o futura.");
        if (tarifa <= 0 || tarifa > 9999999999999999.99m || decimal.Round(tarifa, 2) != tarifa) throw new ArgumentException("Tarifa diaria inválida; máximo dos decimales.");
    }
    public void ActualizarDatos(string codigo, string mascota, DateTime ingreso, decimal tarifa, string tipoMascota, string? razaMascota)
    {
        if (!EstaHospitalizado) throw new ReglaNegocioException("La estancia ya está finalizada.");
        Validar(ingreso, tarifa); ActualizarDatosGenerales(codigo, mascota, ingreso, tipoMascota, razaMascota); TarifaDiaria = tarifa;
    }
    public bool DarAlta(DateTime fechaAlta)
    {
        if (!EstaHospitalizado || fechaAlta < Fecha || fechaAlta > DateTime.Now) return false;
        FechaAlta = fechaAlta; return true;
    }
    public int CalcularDias() => Math.Max(1, ((FechaAlta ?? DateTime.Today).Date - Fecha.Date).Days + 1);
    public override decimal ObtenerPrecioUnitario() => CalcularDias() * TarifaDiaria;
    public override string ObtenerDescripcion() => $"Hospitalización de {NombreMascota}";
    public override void ValidarFacturable(int cantidad)
    {
        base.ValidarFacturable(cantidad);
        if (EstaHospitalizado) throw new ReglaNegocioException("Registra el alta antes de facturar.");
        if (ObtenerPrecioUnitario() > 9999999999999999.99m) throw new ReglaNegocioException("Costo superior al límite de facturación.");
    }
}
