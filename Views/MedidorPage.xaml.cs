using SonoGuard.Data;
using SonoGuard.Models;
using SonoGuard.Services;

namespace SonoGuard.Views
{
    public partial class MedidorPage : ContentPage
    {
        private readonly DatabaseService   _db          = new();
        private readonly MicrophoneService _microfono   = new();
        private double  _ultimoNivelDb  = 0;
        private double  _anchoBarraMax  = 300;

        public MedidorPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (SesionService.HaySesion)
                LabelSaludo.Text = $"Hola, {SesionService.UsuarioActual!.Nombre} 👋";

            _microfono.NuevaMedicion += OnNuevaMedicion;

            bool permiso = await _microfono.SolicitarPermisoAsync();
            if (!permiso)
                await DisplayAlertAsync("Permiso requerido",
                    "Esta app necesita acceso al micrófono para medir el ruido.",
                    "Entendido");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _microfono.NuevaMedicion -= OnNuevaMedicion;
            _microfono.DetenerMedicion();
            ActualizarBotones(false);
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (width > 0) _anchoBarraMax = width - 80;
        }

        private void OnNuevaMedicion(double db)
        {
            _ultimoNivelDb = db;

            LabelDb.Text = $"{db:F1} dB";

            string desc; Color color;
            if      (db < 40)  { desc = "Silencioso";    color = Colors.LightGreen; }
            else if (db < 55)  { desc = "Normal";         color = Colors.YellowGreen; }
            else if (db < 70)  { desc = "Moderado";       color = Colors.Yellow; }
            else if (db < 85)  { desc = "⚠ Elevado";     color = Colors.Orange; }
            else if (db < 100) { desc = "⚠ Muy Alto";    color = Color.FromArgb("#FF5722"); }
            else               { desc = "🔴 Peligroso";  color = Colors.Red; }

            LabelNivelDesc.Text      = desc;
            LabelNivelDesc.TextColor = color;
            LabelDb.TextColor        = color;

            double pct = Math.Clamp(db / 120.0, 0, 1);
            BarraProgreso.WidthRequest      = Math.Max(4, _anchoBarraMax * pct);
            BarraProgreso.BackgroundColor   = color;

            if (db > 85)
                try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(200)); }
                catch { }
        }

        private void OnIniciarClicked(object sender, EventArgs e)
        {
            _microfono.IniciarMedicion();
            ActualizarBotones(true);
        }

        private void OnDetenerClicked(object sender, EventArgs e)
        {
            _microfono.DetenerMedicion();
            ActualizarBotones(false);
        }

        private void ActualizarBotones(bool midiendo)
        {
            BtnIniciar.IsEnabled        = !midiendo;
            BtnDetener.IsEnabled        = midiendo;
            BtnIniciar.BackgroundColor  = midiendo
                ? Color.FromArgb("#616161") : Color.FromArgb("#2E7D32");
            BtnDetener.BackgroundColor  = midiendo
                ? Color.FromArgb("#C62828") : Color.FromArgb("#616161");
        }

        private async void OnGuardarClicked(object sender, EventArgs e)
        {
            if (!SesionService.HaySesion) return;

            if (_ultimoNivelDb == 0)
            {
                await DisplayAlertAsync("Atención",
                    "Inicia la medición antes de guardar una evidencia.", "OK");
                return;
            }

            var ubicacion = EntryUbicacion.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(ubicacion))
            {
                await DisplayAlertAsync("Atención",
                    "Ingresa una ubicación para identificar el lugar.", "OK");
                return;
            }

            var medicion = new Medicion
            {
                NivelDb     = _ultimoNivelDb,
                FechaHora   = DateTime.Now,
                Ubicacion   = ubicacion,
                Descripcion = EditorDescripcion.Text?.Trim() ?? string.Empty,
                EsDenuncia  = CheckDenuncia.IsChecked,
                UsuarioId   = SesionService.UsuarioActual!.Id
            };

            await _db.GuardarMedicionAsync(medicion);

            string tipo = medicion.EsDenuncia ? "denuncia formal" : "evidencia";
            await DisplayAlertAsync("✅ Guardado",
                $"Se registró la {tipo} con {_ultimoNivelDb:F1} dB.", "OK");

            EntryUbicacion.Text      = string.Empty;
            EditorDescripcion.Text   = string.Empty;
            CheckDenuncia.IsChecked  = false;
        }

        private async void OnHistorialClicked(object sender, EventArgs e)
            => await Navigation.PushAsync(new HistorialPage());

        private async void OnCerrarSesionClicked(object sender, EventArgs e)
        {
            bool ok = await DisplayAlertAsync("Cerrar Sesión",
                "¿Seguro que deseas cerrar sesión?", "Sí", "No");
            if (!ok) return;

            _microfono.DetenerMedicion();
            SesionService.CerrarSesion();
            await Navigation.PopToRootAsync();
        }
    }
}
