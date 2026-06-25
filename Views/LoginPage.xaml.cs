using SonoGuard.Data;
using SonoGuard.Services;

namespace SonoGuard.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly DatabaseService _db = new();

        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            LabelError.IsVisible = false;

            var email    = EntryEmail.Text?.Trim();
            var password = EntryPassword.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MostrarError("Ingresa tu correo y contraseña.");
                return;
            }

            var usuario = await _db.ObtenerUsuarioAsync(email, password);
            if (usuario is null)
            {
                MostrarError("Correo o contraseña incorrectos.");
                return;
            }

            SesionService.IniciarSesion(usuario);
            await Navigation.PushAsync(new MedidorPage());
        }

        private async void OnRegistroClicked(object sender, EventArgs e)
            => await Navigation.PushAsync(new RegistroPage());

        private void MostrarError(string msg)
        {
            LabelError.Text = msg;
            LabelError.IsVisible = true;
        }
    }
}
