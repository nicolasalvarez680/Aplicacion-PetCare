namespace PetCare.Domain;

/// <summary>Regla de negocio que impide completar una operación de PetCare.</summary>
public sealed class ReglaNegocioException(string mensaje) : InvalidOperationException(mensaje) { }
