using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Microsoft.EntityFrameworkCore;
using ElPretamita.DataBase;
using ElPretamita.Models;
using ElPretamita.Views.Dashboard;

namespace ElPretamita.Views.Prestamos
{
    public partial class PrestamosView : UserControl
    {
        private MainWindow _mainWindow;
        private List<Models.Prestamo> _listaPrestamosActivos = new();
        private List<Models.Cliente> _listaClientes = new();

        public PrestamosView(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarDatos();
        }

        private async System.Threading.Tasks.Task CargarDatos()
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    await context.Database.MigrateAsync();
                    
                    _listaClientes = await context.Clientes.ToListAsync();

                    var listaPrestamos = await context.Prestamos
                        .Include(p => p.Cliente)
                        .OrderByDescending(p => p.FechaPrestamo)
                        .ToListAsync();
                        
                    DateTime fechaActual = ElPretamita.Services.TimeService.GetRealTime();
                    bool huboCambios = false;

                    foreach (var p in listaPrestamos)
                    {
                        if (fechaActual > p.FechaVencimientoSiguiente && p.BalancePendiente > 0)
                        {
                            int diasAtraso = (fechaActual - p.FechaVencimientoSiguiente).Days;
                            if (diasAtraso > 0)
                            {
                                decimal moraASumar = p.AplicaMoraFija ? (p.ValorMora * diasAtraso) : (p.BalancePendiente * (p.ValorMora / 100m) * diasAtraso);
                                
                                p.TotalMoraAcumulada += moraASumar;
                                p.BalancePendiente += moraASumar;
                                
                                p.FechaVencimientoSiguiente = p.FechaVencimientoSiguiente.AddMonths(1);
                                huboCambios = true;
                            }
                        }
                    }

                    if (huboCambios)
                    {
                        await context.SaveChangesAsync();
                    }

                    _listaPrestamosActivos = listaPrestamos;
                    DgPrestamos.ItemsSource = _listaPrestamosActivos;
                    DgDirectorioClientes.ItemsSource = _listaClientes;
                    PanelDetalles.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando Préstamos: {ex.Message}");
            }
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavegarA(new MainDashboardView(_mainWindow));
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox txt && (txt.Text == "0" || txt.Text == "0.00" || txt.Text == "0.0" || string.IsNullOrWhiteSpace(txt.Text)))
            {
                txt.Text = "";
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox txt && string.IsNullOrWhiteSpace(txt.Text))
            {
                if (txt.Name == "TxtFormMora")
                {
                    txt.Text = "0";
                }
            }
        }

        private void TxtBuscarPrestamo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_listaPrestamosActivos == null || DgPrestamos == null) return;
            
            string filtro = TxtBuscarPrestamo?.Text?.Trim().ToLower() ?? "";
            
            if (string.IsNullOrWhiteSpace(filtro))
            {
                DgPrestamos.ItemsSource = _listaPrestamosActivos;
            }
            else
            {
                DgPrestamos.ItemsSource = _listaPrestamosActivos.Where(p => 
                    (p.Cliente?.Nombre?.ToLower().Contains(filtro) == true) || 
                    (p.Cliente?.Cedula?.Contains(filtro) == true)).ToList();
            }
        }

        private void BtnInactivos_Click(object sender, RoutedEventArgs e)
        {
            ModalOverlayClientes.Opacity = 1;
            ModalOverlayClientes.Visibility = Visibility.Visible;
        }

        private void BtnCerrarInactivos_Click(object sender, RoutedEventArgs e)
        {
            ModalOverlayClientes.Visibility = Visibility.Collapsed;
        }

        private void TxtBuscarDirectorio_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_listaClientes == null || DgDirectorioClientes == null) return;
            
            string filtro = TxtBuscarDirectorio?.Text?.Trim().ToLower() ?? "";
            
            if (string.IsNullOrWhiteSpace(filtro))
            {
                DgDirectorioClientes.ItemsSource = _listaClientes;
            }
            else
            {
                DgDirectorioClientes.ItemsSource = _listaClientes.Where(c => 
                    (c.Nombre?.ToLower().Contains(filtro) == true) || 
                    (c.Cedula?.Contains(filtro) == true)).ToList();
            }
        }

        private void BtnVerHistorial_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Models.Cliente cliente)
            {
                AbrirHistorialCliente(cliente);
            }
        }

        private void DgDirectorioClientes_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DgDirectorioClientes.SelectedItem is Models.Cliente cliente)
            {
                AbrirHistorialCliente(cliente);
            }
        }

        private void AbrirHistorialCliente(Models.Cliente cliente)
        {
            var ventana = new ElPretamita.Views.Compartido.HistorialPrestamosClienteWindow(cliente.Id, cliente.Nombre);
            ventana.ShowDialog();
        }

        private void DgPrestamos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgPrestamos.SelectedItem is Models.Prestamo prestamoSeleccionado)
            {
                TxtDetalleNombre.Text = prestamoSeleccionado.Cliente?.Nombre ?? "---";
                TxtDetalleCedula.Text = prestamoSeleccionado.Cliente?.Cedula ?? "---";
                
                TxtDetalleMonto.Text = prestamoSeleccionado.Monto.ToString("C");
                TxtDetalleInteres.Text = $"{prestamoSeleccionado.Interes}%";
                TxtDetalleBalance.Text = prestamoSeleccionado.BalancePendiente.ToString("C");
                
                PanelDetalles.Visibility = Visibility.Visible;
            }
            else
            {
                PanelDetalles.Visibility = Visibility.Hidden;
            }
        }

        private void BtnNuevoPrestamo_Click(object sender, RoutedEventArgs e)
        {
            CmbBuscarCliente.ItemsSource = _listaClientes;
            CmbBuscarCliente.SelectedItem = null;
            CmbBuscarCliente.Text = "";

            TxtFormNombre.Text = "";
            TxtFormCedula.Text = "";
            TxtFormTelefono.Text = "";
            TxtFormMonto.Text = "0";
            TxtFormInteres.Text = "0";
            TxtFormCuotas.Text = "1";
            
            var fadeIn = (Storyboard)this.Resources["FadeInModal"];
            fadeIn.Begin();
        }

        private void CmbBuscarCliente_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbBuscarCliente.SelectedItem is Models.Cliente cliente)
            {
                TxtFormNombre.Text = cliente.Nombre;
                TxtFormCedula.Text = cliente.Cedula;
                TxtFormTelefono.Text = cliente.Telefono;
            }
        }

        private void BtnCerrarModal_Click(object sender, RoutedEventArgs e)
        {
            CerrarModal();
        }

        private void CerrarModal()
        {
            var fadeOut = (Storyboard)this.Resources["FadeOutModal"];
            fadeOut.Begin();
        }

        private void TxtFormCedula_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;

            string digits = new string(tb.Text.Where(char.IsDigit).ToArray());
            if (digits.Length > 11) digits = digits.Substring(0, 11);

            string formatted = "";
            for (int i = 0; i < digits.Length; i++)
            {
                if (i == 3 || i == 10) formatted += "-";
                formatted += digits[i];
            }

            if (tb.Text != formatted)
            {
                tb.Text = formatted;
                tb.SelectionStart = formatted.Length;
            }
        }

        private void TxtFormTelefono_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;

            string digits = new string(tb.Text.Where(char.IsDigit).ToArray());
            if (digits.Length > 10) digits = digits.Substring(0, 10);

            string formatted = "";
            for (int i = 0; i < digits.Length; i++)
            {
                if (i == 3 || i == 6) formatted += "-";
                formatted += digits[i];
            }

            if (tb.Text != formatted)
            {
                tb.Text = formatted;
                tb.SelectionStart = formatted.Length;
            }
        }

        private async void BtnGuardarPrestamo_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtFormNombre.Text)) return;
            
            decimal monto = 0, interes = 0, valorMora = 0;
            int cuotas = 1;

            decimal.TryParse(TxtFormMonto.Text, out monto);
            decimal.TryParse(TxtFormInteres.Text, out interes);
            int.TryParse(TxtFormCuotas.Text, out cuotas);
            decimal.TryParse(TxtFormMora.Text, out valorMora);

            bool aplicaMoraFija = CmbTipoMora.SelectedIndex == 0;

            decimal montoInteres = monto * (interes / 100);
            decimal balanceTotal = monto + montoInteres;
            decimal montoCuota = balanceTotal / cuotas;

            string textoMora = aplicaMoraFija 
                ? $"un cargo fijo por mora de {valorMora:C} por cada atraso sobre la cuota vencida" 
                : $"un interés moratorio del {valorMora}% sobre el saldo en atraso";

            var contrato = new ElPretamita.Views.Compartido.ContratoWindow(
                TxtFormNombre.Text.Trim(),
                TxtFormCedula.Text.Trim(),
                monto,
                balanceTotal,
                cuotas,
                " cuotas mensuales consecutivas",
                montoCuota,
                textoMora
            );
            contrato.ShowDialog();
            
            if (!contrato.Aceptado) return;
            
            DateTime fechaActual = ElPretamita.Services.TimeService.GetRealTime();

            try
            {
                using (var context = new AppDbContext())
                {
                    string cedula = TxtFormCedula.Text.Trim();
                    var clienteDB = await context.Clientes.FirstOrDefaultAsync(c => c.Cedula == cedula);
                    
                    if (clienteDB != null)
                    {
                        clienteDB.Nombre = TxtFormNombre.Text.Trim();
                        clienteDB.Telefono = TxtFormTelefono.Text.Trim();
                        context.Clientes.Update(clienteDB);
                    }
                    else
                    {
                        clienteDB = new Cliente
                        {
                            Nombre = TxtFormNombre.Text.Trim(),
                            Cedula = cedula,
                            Telefono = TxtFormTelefono.Text.Trim(),
                            Direccion = "No especificada"
                        };
                        context.Clientes.Add(clienteDB);
                    }
                    
                    await context.SaveChangesAsync();

                    var nuevoPrestamo = new Models.Prestamo
                    {
                        ClienteId = clienteDB.Id,
                        Monto = monto,
                        Interes = interes,
                        CantidadCuotas = cuotas,
                        BalancePendiente = balanceTotal,
                        FechaPrestamo = fechaActual,
                        FechaVencimientoSiguiente = fechaActual.AddMonths(1), // Cobro mensual
                        AplicaMoraFija = aplicaMoraFija,
                        ValorMora = valorMora,
                        TotalMoraAcumulada = 0
                    };

                    context.Prestamos.Add(nuevoPrestamo);
                    await context.SaveChangesAsync();
                }

                BtnCerrarModal_Click(this, new RoutedEventArgs());
                await CargarDatos();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error guardando Préstamo: {ex.Message}");
                MessageBox.Show("Hubo un error al guardar los datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnAbonarCuota_Click(object sender, RoutedEventArgs e)
        {
            if (DgPrestamos.SelectedItem is Models.Prestamo prestamoSeleccionado)
            {
                if (prestamoSeleccionado.BalancePendiente <= 0)
                {
                    MessageBox.Show("Este Préstamo ya está totalmente pagado.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                decimal montoTotalInicial = prestamoSeleccionado.Monto + (prestamoSeleccionado.Monto * (prestamoSeleccionado.Interes / 100m));
                decimal montoCuota = montoTotalInicial / prestamoSeleccionado.CantidadCuotas;

                if (prestamoSeleccionado.BalancePendiente < montoCuota)
                    montoCuota = prestamoSeleccionado.BalancePendiente;

                var result = MessageBox.Show($"¿Deseas registrar el pago de una cuota por {montoCuota:C}?", "Confirmar Pago", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var context = new AppDbContext())
                        {
                            var prestamoDb = await context.Prestamos.FindAsync(prestamoSeleccionado.Id);
                            if (prestamoDb != null)
                            {
                                var pago = new CuotaPrestamo
                                {
                                    PrestamoId = prestamoDb.Id,
                                    MontoCuota = montoCuota,
                                    FechaPago = prestamoDb.FechaVencimientoSiguiente,
                                    Pagada = true,
                                    FechaRealPago = ElPretamita.Services.TimeService.GetRealTime()
                                };
                                context.CuotasPrestamo.Add(pago);
                                
                                prestamoDb.BalancePendiente -= montoCuota;
                                
                                if (prestamoDb.BalancePendiente < 1m)
                                {
                                    prestamoDb.BalancePendiente = 0;
                                }
                                
                                prestamoDb.FechaVencimientoSiguiente = prestamoDb.FechaVencimientoSiguiente.AddMonths(1);
                                
                                await context.SaveChangesAsync();
                                
                                int cuotaActual = await context.CuotasPrestamo.CountAsync(c => c.PrestamoId == prestamoDb.Id);

                                var reciboWin = new ElPretamita.Views.Compartido.ReciboWindow(
                                    prestamoSeleccionado.Cliente.Nombre,
                                    montoCuota,
                                    prestamoDb.BalancePendiente,
                                    cuotaActual,
                                    prestamoDb.CantidadCuotas,
                                    ElPretamita.Services.TimeService.GetRealTime()
                                );
                                reciboWin.ShowDialog();
                            }
                        }
                        await CargarDatos();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al procesar pago: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
