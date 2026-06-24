using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using ElPretamita.Views.Dashboard;

namespace ElPretamita
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Cargar la vista principal al inicio
            NavegarA(new MainDashboardView(this));
        }

        // Permite arrastrar la ventana sin bordes
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        // Minimiza la ventana
        private void BtnMinimizar_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Cierra la aplicación completa
        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Sistema de navegación fluido
        public async void NavegarA(UserControl nuevaVista)
        {
            if (MainContainer.Content != null)
            {
                // Reproducir animación de salida para la vista actual
                var fadeOut = (Storyboard)this.Resources["FadeOutTransition"];
                fadeOut.Begin(MainContainer);
                
                // Esperar a que termine la animación
                await Task.Delay(300);
            }

            // Cambiar el contenido
            MainContainer.Content = nuevaVista;

            // Reproducir animación de entrada
            var fadeIn = (Storyboard)this.Resources["FadeInTransition"];
            fadeIn.Begin(MainContainer);
        }
    }
}