using System.Windows;
using System.Windows.Controls;
using ElPretamita.Views.Sam;
using ElPretamita.Views.Prestamos;

namespace ElPretamita.Views.Dashboard
{
    public partial class MainDashboardView : UserControl
    {
        private MainWindow _mainWindow;

        public MainDashboardView(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void BtnSam_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavegarA(new SamView(_mainWindow));
        }

        private void BtnPrestamos_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavegarA(new PrestamosView(_mainWindow));
        }

        private void BtnCambiarLogo_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Archivos de Imagen|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Selecciona un nuevo logo para el Login"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string appDataPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "ElPretamita");
                    if (!System.IO.Directory.Exists(appDataPath))
                    {
                        System.IO.Directory.CreateDirectory(appDataPath);
                    }

                    string destPath = System.IO.Path.Combine(appDataPath, "logo_custom.png");
                    
                    // Copiar y sobreescribir el logo
                    System.IO.File.Copy(openFileDialog.FileName, destPath, true);
                    
                    MessageBox.Show("Logo del Login actualizado correctamente. Lo verás la próxima vez que inicies sesión.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Error al cambiar el logo: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
