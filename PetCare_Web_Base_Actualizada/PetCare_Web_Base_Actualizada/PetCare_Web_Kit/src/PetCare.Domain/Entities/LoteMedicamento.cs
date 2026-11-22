namespace PetCare.Domain.Entities;
public class LoteMedicamento : ProductoFarmacia
{
    public DateOnly FechaVencimiento { get; private set; }
    private LoteMedicamento() { }
    public LoteMedicamento(string codigo, string nombre, DateOnly vencimiento, decimal precio) : base(codigo, nombre, precio)
    { FechaVencimiento = vencimiento; }
    public bool EstaVencido() => FechaVencimiento < DateOnly.FromDateTime(DateTime.Today);
    public override string ObtenerDescripcion() => $"Medicamento: {Nombre} - Lote: {Codigo}";
    public override void ValidarFacturable(int cantidad)
    {
        base.ValidarFacturable(cantidad);
        if (EstaVencido()) throw new InvalidOperationException("No se puede facturar un medicamento vencido.");
    }
}
