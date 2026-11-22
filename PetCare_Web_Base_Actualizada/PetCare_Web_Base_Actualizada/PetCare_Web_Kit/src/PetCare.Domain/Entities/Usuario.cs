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
        NombreUsuario = nombreUsuario; NombreCompleto = nombreCompleto; Rol = rol; ContrasenaHash = contrasenaHash;
    }
    public void CambiarContrasenaHash(string nuevoHash)
    {
        if (string.IsNullOrWhiteSpace(nuevoHash)) throw new ArgumentException("Hash obligatorio.");
        ContrasenaHash = nuevoHash;
    }
}
