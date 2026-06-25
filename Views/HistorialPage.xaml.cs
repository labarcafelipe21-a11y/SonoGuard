using SonoGuard.Data;
using SonoGuard.Models;
using SonoGuard.Services;

namespace SonoGuard.Views
{
    public partial class HistorialPage : ContentPage
    {
        private readonly DatabaseService _db = new();
        private List<Medicion> _todas = new();
        private bool _soloDenuncias = false;

        public HistorialPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarAsync();
        }

        private async Task CargarAsync()
        {
            if (!SesionService.HaySesion) return;
            _todas = await _db.ObtenerMedicionesAsync(SesionService.UsuarioActual!.Id);
            AplicarFiltro();
            ActualizarEstadisticas();
        }

        private void AplicarFiltro()
        {
            var lista = _soloDenuncias
                ? _todas.Where(m => m.EsDenuncia).ToList()
                : _todas;
            ListaMediciones.ItemsSource = null;
            ListaMediciones.ItemsSource = lista;
        }

        private void ActualizarEstadisticas()
        {
            LabelTotal.Text   = _todas.Count.ToString();
            LabelMax.Text     = _todas.Count > 0 ? $"{_todas.Max(m => m.NivelDb):F1} dB" : "0 dB";
            LabelPromedio.Text = _todas.Count > 0 ? $"{_todas.Average(m => m.NivelDb):F1} dB" : "0 dB";
        }

        private void OnFiltroTodosClicked(object sender, EventArgs e)
        {
            _soloDenuncias = false;
            BtnTodos.BackgroundColor     = Color.FromArgb("#E94560");
            BtnTodos.TextColor           = Colors.White;
            BtnDenuncias.BackgroundColor = Color.FromArgb("#16213E");
            BtnDenuncias.TextColor       = Color.FromArgb("#A0A0B0");
            AplicarFiltro();
        }

        private void OnFiltroDenunciasClicked(object sender, EventArgs e)
        {
            _soloDenuncias = true;
            BtnDenuncias.BackgroundColor = Color.FromArgb("#E94560");
            BtnDenuncias.TextColor       = Colors.White;
            BtnTodos.BackgroundColor     = Color.FromArgb("#16213E");
            BtnTodos.TextColor           = Color.FromArgb("#A0A0B0");
            AplicarFiltro();
        }

        private async void OnMedicionSeleccionada(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is not Medicion m) return;
            ListaMediciones.SelectedItem = null;
            await Navigation.PushAsync(new DetallePage(m));
        }

        private async void OnEliminarSwipe(object sender, EventArgs e)
        {
            if (sender is SwipeItem sw && sw.BindingContext is Medicion m)
            {
                bool ok = await DisplayAlertAsync("Eliminar",
                    $"¿Eliminar el registro de {m.NivelDbTexto}?", "Sí", "No");
                if (!ok) return;
                await _db.EliminarMedicionAsync(m);
                await CargarAsync();
            }
        }
    }
}
