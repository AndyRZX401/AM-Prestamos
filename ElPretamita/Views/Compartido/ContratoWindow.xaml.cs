using System;
using System.Windows;
using System.Windows.Controls;

namespace ElPretamita.Views.Compartido
{
    public partial class ContratoWindow : Window
    {
        public bool Aceptado { get; private set; } = false;

        public ContratoWindow(string nombreCliente, string cedula, decimal montoPrestado, decimal totalAPagar, int cantidadCuotas, string tipoCuota, decimal valorCuota, string textoMora = "un interés moratorio del cinco por ciento (5%) mensual sobre el saldo en atraso")
        {
            InitializeComponent();

            DateTime fecha = DateTime.Now;
            string[] meses = { "enero", "febrero", "marzo", "abril", "mayo", "junio", "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre" };
            TxtFecha.Text = $"{fecha.Day} de {meses[fecha.Month - 1]} de {fecha.Year}";
            
            string nombreStr = string.IsNullOrWhiteSpace(nombreCliente) ? "__________" : nombreCliente;
            string cedulaStr = string.IsNullOrWhiteSpace(cedula) ? "__________" : cedula;
            
            RunNombre.Text = nombreStr;
            RunCedula.Text = cedulaStr;
            
            RunMonto.Text = montoPrestado.ToString("C");
            RunTotal.Text = totalAPagar.ToString("C");
            
            RunCuotas.Text = cantidadCuotas.ToString();
            RunTipoCuota.Text = tipoCuota;
            
            RunValorCuota.Text = valorCuota.ToString("C");

            // Textos adicionales para el pagaré
            RunNombrePagare.Text = nombreStr;
            RunTotalPagare.Text = totalAPagar.ToString("C");
            RunTextoMora.Text = textoMora;

            // Firmas
            RunNombreFirma.Text = nombreStr;
            RunCedulaFirma.Text = cedulaStr;
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Aceptado = false;
            this.Close();
        }

        private void BtnProceder_Click(object sender, RoutedEventArgs e)
        {
            Aceptado = true;
            this.Close();
        }

        private void BtnImprimir_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintVisual(AreaImprimible, "Impresión de Contrato Amigable");
            }
        }
    }
}
