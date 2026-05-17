using TP_ClubDeportivo.Models;

namespace TP_ClubDeportivo
{
    internal static class Sesion
    {
        public static Usuario? UsuarioActual { get; private set; }

        public static bool EstaLogueado => UsuarioActual is not null;

        public static void Iniciar(Usuario usuario) => UsuarioActual = usuario;

        public static void Cerrar() => UsuarioActual = null;

        public static bool TieneRol(params string[] roles)
        {
            if (UsuarioActual is null)
            {
                return false;
            }

            return roles.Any(r =>
                UsuarioActual.Rol.Equals(r, StringComparison.OrdinalIgnoreCase));
        }
    }
}
