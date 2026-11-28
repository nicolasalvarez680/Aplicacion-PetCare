using PetCare.Domain.Enums;
namespace PetCare.Domain.Entities;
public class Usuario
{
    public int Id { get; private set; }
    public string NombreUsuario { get; private set; } = null!;
    public string NombreCompleto { get; private set; } = null!;
    public RolUsuario Rol { get; private set; }
    public string ContrasenaHash { get; private set; } = null!;
    public bool EsAdministrador => Rol == RolUsuario.Administrador;
    private Usuario() { }
    public Usuario(string nombreUsuario, string nombreCompleto, RolUsuario rol, string contrasenaHash)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario)) throw new ArgumentException("Usuario obligatorio.");
        if (string.IsNullOrWhiteSpace(nombreCompleto)) throw new ArgumentException("Nombre obligatorio.");
        if (string.IsNullOrWhiteSpace(contrasenaHash)) throw new ArgumentException("Se requiere hash, no contraseña plana.");
        ActualizarDatos(nombreUsuario, nombreCompleto, rol); ContrasenaHash = contrasenaHash;
    }
    public void CambiarContrasenaHash(string nuevoHash)
    {
        if (string.IsNullOrWhiteSpace(nuevoHash)) throw new ArgumentException("Hash obligatorio.");
        ContrasenaHash = nuevoHash;
    }
    public void ActualizarDatos(
    string nombreUsuario,
    string nombreCompleto,
    RolUsuario rol)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario))
            throw new ArgumentException("Usuario obligatorio.");

        if (string.IsNullOrWhiteSpace(nombreCompleto))
            throw new ArgumentException("Nombre obligatorio.");

        if (nombreUsuario.Trim().Length > 50)
            throw new ArgumentException(
                "El usuario no puede superar los 50 caracteres.");

        if (nombreCompleto.Trim().Length > 100)
            throw new ArgumentException(
                "El nombre no puede superar los 100 caracteres.");

        if (!Enum.IsDefined(rol))
            throw new ArgumentException("Rol no válido.");

        NombreUsuario = nombreUsuario.Trim();
        NombreCompleto = nombreCompleto.Trim();
        Rol = rol;
    }
}
