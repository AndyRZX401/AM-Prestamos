using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace ElPretamita.Views.Compartido
{
    public partial class ReciboWindow : Window
    {
        public ReciboWindow(string cliente, decimal montoPagado, decimal saldoRestante, int semanaActual, int totalSemanas, DateTime fecha)
        {
            InitializeComponent();

            // El usuario solicitó que el número de ticket concuerde con la semana actual
            TxtTicketNum.Text = semanaActual.ToString("D2");
            
            TxtFecha.Text = fecha.ToString("dd/MM/yyyy HH:mm:ss");
            TxtCliente.Text = cliente;
            
            TxtSemanaActual.Text = $"{semanaActual} de {totalSemanas}";
            int semanasRestantes = totalSemanas - semanaActual;
            TxtSemanasRestantes.Text = semanasRestantes < 0 ? "0" : semanasRestantes.ToString();

            TxtMontoPagado.Text = montoPagado.ToString("C");
            TxtSaldoRestante.Text = saldoRestante.ToString("C");
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnImprimir_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintVisual(TicketImprimible, $"Recibo Ticket N° {TxtTicketNum.Text}");
            }
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
