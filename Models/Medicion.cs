using SQLite;

namespace SonoGuard.Models
{
    public class Medicion
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public double NivelDb { get; set; }

        public DateTime FechaHora { get; set; }

        public string Ubicacion { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public bool EsDenuncia { get; set; }

        public int UsuarioId { get; set; }

        [Ignore]
        public string NivelDbTexto => $"{NivelDb:F1} dB";

        [Ignore]
        public string FechaHoraTexto => FechaHora.ToString("dd/MM/yyyy HH:mm");

        [Ignore]
        public string NivelDescripcion => NivelDb switch
        {
            < 40  => "Silencioso",
            < 55  => "Normal",
            < 70  => "Moderado",
            < 85  => "⚠ Elevado",
            < 100 => "⚠ Muy Alto",
            _     => "🔴 Peligroso"
        };

        [Ignore]
        public Color ColorNivel => NivelDb switch
        {
            < 55  => Colors.LightGreen,
            < 70  => Colors.Yellow,
            < 85  => Colors.Orange,
            _     => Colors.Red
        };
    }
}
