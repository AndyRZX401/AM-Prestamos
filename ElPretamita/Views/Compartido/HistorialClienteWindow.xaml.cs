using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using ElPretamita.DataBase;

namespace ElPretamita.Views.Compartido
{
    public partial class HistorialClienteWindow : Window
    {
        private int _clienteId;
        private string _nombreCliente;

        public HistorialClienteWindow(int clienteId, string nombreCliente)
        {
            InitializeComponent();
            _clienteId = clienteId;
            _nombreCliente = nombreCliente;
            TxtTitulo.Text = $"Historial de: {_nombreCliente}";
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    var historialSams = await context.Sams
                        .Where(s => s.ClienteId == _clienteId)
                        .OrderByDescending(s => s.FechaInicio)
                        .ToListAsync();

                    DgHistorialSams.ItemsSource = historialSams;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el historial: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
