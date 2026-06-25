using SonoGuard.Data;
using SonoGuard.Models;

namespace SonoGuard.Views
{
    public partial class DetallePage : ContentPage
    {
        private readonly DatabaseService _db = new();
        private readonly Medicion _medicion;

        public DetallePage(Medicion medicion)
        {
            InitializeComponent();
            _medicion = medicion;
            CargarDatos();
        }

        private void CargarDatos()
        {
            LabelNivelGrande.Text = _medicion.NivelDbTexto;
            LabelNivelExacto.Text = $"{_medicion.NivelDb:F4} dB";
            LabelFecha.Text       = _medicion.FechaHoraTexto;
            LabelUbicacion.Text   = string.IsNullOrEmpty(_medicion.Ubicacion)
                ? "Sin ubicación especificada" : _medicion.Ubicacion;
            LabelDescripcion.Text = string.IsNullOrEmpty(_medicion.Descripcion)
                ? "Sin descripción" : _medicion.Descripcion;

            string desc; Color color;
            double db = _medicion.NivelDb;
            if      (db < 40)  { desc = "Silencioso";   color = Colors.LightGreen; }
            else if (db < 55)  { desc = "Normal";        color = Colors.YellowGreen; }
            else if (db < 70)  { desc = "Moderado";      color = Colors.Yellow; }
            else if (db < 85)  { desc = "⚠ Elevado";    color = Colors.Orange; }
            else if (db < 100) { desc = "⚠ Muy Alto";   color = Color.FromArgb("#FF5722"); }
            else               { desc = "🔴 Peligroso"; color = Colors.Red; }

            LabelNivelGrande.TextColor = color;
            LabelNivelDesc.Text        = desc;
            LabelNivelDesc.TextColor   = color;

            if (_medicion.EsDenuncia)
            {
                BadgeBorder.BackgroundColor = Color.FromArgb("#E94560");
                LabelBadge.Text             = "⚖ Denuncia Formal";
                BtnConvertir.IsVisible      = false;
            }
            else
            {
                BadgeBorder.BackgroundColor = Color.FromArgb("#16213E");
                LabelBadge.Text             = "📋 Evidencia registrada";
                BtnConvertir.IsVisible      = db >= 55;
            }
        }

        private async void OnConvertirClicked(object sender, EventArgs e)
        {
            bool ok = await DisplayAlertAsync("Convertir a Denuncia",
                $"¿Marcar esta medición de {_medicion.NivelDbTexto} como denuncia formal?",
                "Sí, denunciar", "Cancelar");
            if (!ok) return;

            _medicion.EsDenuncia = true;
            await _db.ActualizarMedicionAsync(_medicion);
            await DisplayAlertAsync("✅ Denuncia registrada",
                "La evidencia fue marcada como denuncia formal.", "OK");
            CargarDatos();
        }

        private async void OnEliminarClicked(object sender, EventArgs e)
        {
            bool ok = await DisplayAlertAsync("Eliminar Registro",
                "¿Seguro que quieres eliminar este registro? No se puede deshacer.",
                "Eliminar", "Cancelar");
            if (!ok) return;
            await _db.EliminarMedicionAsync(_medicion);
            await Navigation.PopAsync();
        }

        private async void OnVolverClicked(object sender, EventArgs e)
            => await Navigation.PopAsync();
    }
}
