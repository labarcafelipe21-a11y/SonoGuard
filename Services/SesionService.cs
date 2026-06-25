using SonoGuard.Models;

namespace SonoGuard.Services
{
    public static class SesionService
    {
        public static Usuario? UsuarioActual { get; private set; }
        public static bool HaySesion => UsuarioActual != null;

        public static void IniciarSesion(Usuario u) => UsuarioActual = u;
        public static void CerrarSesion()           => UsuarioActual = null;
    }
}
