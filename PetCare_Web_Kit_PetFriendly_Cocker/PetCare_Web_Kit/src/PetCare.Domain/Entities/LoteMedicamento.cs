using PetCare.Domain;
namespace PetCare.Domain.Entities;
public class LoteMedicamento : ProductoFarmacia
{
    public DateOnly FechaVencimiento { get; private set; }
    public int Existencias { get; private set; }
    private LoteMedicamento() { }
    public LoteMedicamento(string codigo, string nombre, DateOnly vencimiento, decimal precio, int existencias = 0) : base(codigo, nombre, precio)
    {
        ValidarVencimiento(vencimiento);
        if (existencias < 0 || existencias > 1000000) throw new ArgumentException("Existencias entre 0 y 1.000.000.");
        FechaVencimiento = vencimiento; Existencias = existencias;
    }
    private static void ValidarVencimiento(DateOnly vencimiento)
    { if (vencimiento == default) throw new ArgumentException("Fecha de vencimiento obligatoria."); }
    public void ActualizarDatos(string codigo, string nombre, DateOnly vencimiento, decimal precio)
    { ValidarVencimiento(vencimiento); ActualizarDatosProducto(codigo, nombre, precio); FechaVencimiento = vencimiento; }
    public bool EstaVencido() => FechaVencimiento < DateOnly.FromDateTime(DateTime.Today);
    public void Reponer(int cantidad)
    { if (cantidad <= 0 || (long)Existencias + cantidad > int.MaxValue) throw new ArgumentException("Cantidad de reposición inválida."); Existencias += cantidad; }
    public void Descontar(int cantidad)
    { ValidarFacturable(cantidad); Existencias -= cantidad; }
    public override string ObtenerDescripcion() => $"Medicamento: {Nombre} - Lote: {Codigo}";
    public override void ValidarFacturable(int cantidad)
    {
        base.ValidarFacturable(cantidad);
        if (EstaVencido()) throw new ReglaNegocioException("No se puede facturar un medicamento vencido.");
        if (cantidad > Existencias) throw new ReglaNegocioException("Existencias insuficientes.");
    }
}
