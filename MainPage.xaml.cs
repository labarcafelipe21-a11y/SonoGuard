using SonoGuard.Views;

namespace SonoGuard
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnIniciarSesionClicked(object sender, EventArgs e)
            => await Navigation.PushAsync(new LoginPage());

        private async void OnRegistrarseClicked(object sender, EventArgs e)
            => await Navigation.PushAsync(new RegistroPage());
    }
}
