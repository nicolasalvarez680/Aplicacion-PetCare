namespace PetCare.Domain.Interfaces;
public interface IFacturable
{
    string ObtenerDescripcion();
    decimal ObtenerPrecioUnitario();
    void ValidarFacturable(int cantidad);
}
