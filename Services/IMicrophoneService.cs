namespace SonoGuard.Services
{
    public interface IMicrophoneService : IDisposable
    {
        bool EstaActivo { get; }
        event Action<double>? NuevaMedicion;
        Task<bool> SolicitarPermisoAsync();
        void IniciarMedicion();
        void DetenerMedicion();
    }
}
