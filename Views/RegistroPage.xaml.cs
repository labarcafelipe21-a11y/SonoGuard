using SonoGuard.Data;
using SonoGuard.Models;

namespace SonoGuard.Views
{
    public partial class RegistroPage : ContentPage
    {
        private readonly DatabaseService _db = new();

        public RegistroPage()
        {
            InitializeComponent();
        }

        private async void OnRegistrarClicked(object sender, EventArgs e)
        {
            LabelMensaje.IsVisible = false;

            var nombre    = EntryNombre.Text?.Trim();
            var email     = EntryEmail.Text?.Trim();
            var password  = EntryPassword.Text;
            var confirmar = EntryConfirmar.Text;

            if (string.IsNullOrEmpty(nombre))
            { MostrarError("Ingresa tu nombre completo."); return; }

            if (string.IsNullOrEmpty(email) || !email.Contains('@'))
            { MostrarError("Ingresa un correo electrónico válido."); return; }

            if (string.IsNullOrEmpty(password) || password.Length < 6)
            { MostrarError("La contraseña debe tener al menos 6 caracteres."); return; }

            if (password != confirmar)
            { MostrarError("Las contraseñas no coinciden."); return; }

            if (await _db.ObtenerPorEmailAsync(email) is not null)
            { MostrarError("Ya existe una cuenta con ese correo."); return; }

            await _db.RegistrarUsuarioAsync(
                new Usuario { Nombre = nombre, Email = email, Password = password });

            MostrarExito("¡Cuenta creada! Ahora inicia sesión.");
            await Task.Delay(1500);
            await Navigation.PopAsync();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
            => await Navigation.PopAsync();

        private void MostrarError(string msg)
        {
            LabelMensaje.TextColor = Color.FromArgb("#E94560");
            LabelMensaje.Text = msg;
            LabelMensaje.IsVisible = true;
        }

        private void MostrarExito(string msg)
        {
            LabelMensaje.TextColor = Colors.LightGreen;
            LabelMensaje.Text = msg;
            LabelMensaje.IsVisible = true;
        }
    }
}
