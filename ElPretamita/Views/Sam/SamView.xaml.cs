using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Microsoft.EntityFrameworkCore;
using ElPretamita.DataBase;
using ElPretamita.Models;
using ElPretamita.Views.Dashboard;

namespace ElPretamita.Views.Sam
{
    public partial class SamView : UserControl
    {
        private MainWindow _mainWindow;
        private System.Collections.Generic.List<Models.Sam> _listaSamsActivos = new System.Collections.Generic.List<Models.Sam>();
        private System.Collections.Generic.List<Models.Cliente> _listaClientes = new System.Collections.Generic.List<Models.Cliente>();

        public SamView(MainWindow mainWindow)
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
                    // Asegurar que haya base de datos
                    await context.Database.MigrateAsync();
                    
                    var listaSams = await context.Sams
                        .Include(s => s.Cliente)
                        .Include(s => s.Pagos)
                        .OrderByDescending(s => s.FechaInicio)
                        .ToListAsync();
                        
                    DateTime fechaActual = ElPretamita.Services.TimeService.GetRealTime();
                    bool huboCambios = false;

                    // Actualizar estados
                    foreach (var s in listaSams)
                    {
                        if (s.SaldoRestante <= 0)
                        {
                            if (s.Estado != "Pagado")
                            {
                                s.Estado = "Pagado";
                                huboCambios = true;
                            }
                        }
                        else if (fechaActual.Date > s.FechaVencimientoSiguiente.Date)
                        {
                            if (s.Estado != "Atrasado")
                            {
                                s.Estado = "Atrasado";
                                huboCambios = true;
                            }
                        }
                        else
                        {
                            if (s.Estado != "Activo")
                            {
                                s.Estado = "Activo";
                                huboCambios = true;
                            }
                        }
                    }

                    if (huboCambios)
                    {
                        await context.SaveChangesAsync();
                    }

                    // Separar activos e inactivos (Pagados)
                    _listaSamsActivos = listaSams.Where(s => s.Estado != "Pagado").ToList();
                    _listaClientes = await context.Clientes.ToListAsync();

                    FiltrarGrid();
                    FiltrarGridClientes();
                    PanelDetalles.Visibility = Visibility.Hidden;

                    // Calcular Resúmenes usando TODOS los SAMs
                    TxtResumenPrestado.Text = listaSams.Sum(s => s.MontoOriginal).ToString("C");
                    
                    var totalCobrado = listaSams.SelectMany(s => s.Pagos).Sum(p => p.MontoPagado);
                    TxtResumenCobrado.Text = totalCobrado.ToString("C");
                    
                    TxtResumenActivos.Text = listaSams.Count(s => s.Estado == "Activo").ToString();
                    TxtResumenVencidos.Text = listaSams.Count(s => s.Estado == "Atrasado").ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando SAMs: {ex.Message}");
            }
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavegarA(new MainDashboardView(_mainWindow));
        }

        private void DgSams_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgSams.SelectedItem is Models.Sam samSeleccionado)
            {
                TxtDetalleNombre.Text = samSeleccionado.Cliente?.Nombre ?? "---";
                TxtDetalleMonto.Text = samSeleccionado.MontoOriginal.ToString("C");
                TxtDetalleInteres.Text = $"{samSeleccionado.InteresPorcentaje}% - {samSeleccionado.CantidadSemanas} semanas";
                TxtDetalleTotalPagar.Text = samSeleccionado.TotalPagar.ToString("C");
                TxtDetalleSaldo.Text = samSeleccionado.SaldoRestante.ToString("C");
                
                DgPagosHistorial.ItemsSource = samSeleccionado.Pagos.OrderByDescending(p => p.FechaPago).ToList();
                
                BtnAbonar.IsEnabled = samSeleccionado.SaldoRestante > 0;

                PanelDetalles.Visibility = Visibility.Visible;
            }
            else
            {
                PanelDetalles.Visibility = Visibility.Hidden;
            }
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
                // Solo si es el de mora o algun valor que no deba estar en blanco, podemos poner 0.
                // Como es una forma genérica, para Monto o Interés no afecta mucho si está en blanco temporalmente.
                // Pero para la mora es útil.
                if (txt.Name == "TxtFormMora")
                {
                    txt.Text = "0";
                }
            }
        }

        private void BtnNuevoSam_Click(object sender, RoutedEventArgs e)
        {
            CmbBuscarCliente.ItemsSource = _listaClientes;
            CmbBuscarCliente.SelectedItem = null;
            CmbBuscarCliente.Text = "";

            TxtFormNombre.Text = "";
            TxtFormCedula.Text = "";
            TxtFormTelefono.Text = "";
            TxtFormMonto.Text = "0";
            TxtFormSemanas.Text = "0";
            TxtFormInteres.Text = "0";
            
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

        private void BtnInactivos_Click(object sender, RoutedEventArgs e)
        {
            ModalOverlayInactivos.Opacity = 1;
            ModalOverlayInactivos.Visibility = Visibility.Visible;
        }

        private void BtnCerrarInactivos_Click(object sender, RoutedEventArgs e)
        {
            ModalOverlayInactivos.Visibility = Visibility.Collapsed;
        }

        private void TxtBuscarCliente_TextChanged(object sender, TextChangedEventArgs e)
        {
            FiltrarGrid();
        }

        private void FiltrarGrid()
        {
            if (_listaSamsActivos == null || DgSams == null) return;
            
            string filtro = TxtBuscarCliente?.Text?.Trim().ToLower() ?? "";
            
            if (string.IsNullOrWhiteSpace(filtro))
            {
                DgSams.ItemsSource = _listaSamsActivos;
            }
            else
            {
                DgSams.ItemsSource = _listaSamsActivos.Where(s => 
                    (s.Cliente?.Nombre?.ToLower().Contains(filtro) == true) || 
                    (s.Cliente?.Cedula?.Contains(filtro) == true)).ToList();
            }
        }

        private void TxtBuscarInactivo_TextChanged(object sender, TextChangedEventArgs e)
        {
            FiltrarGridClientes();
        }

        private void FiltrarGridClientes()
        {
            if (_listaClientes == null || DgDirectorioClientes == null) return;
            
            string filtro = TxtBuscarInactivo?.Text?.Trim().ToLower() ?? "";
            
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
            var ventana = new ElPretamita.Views.Compartido.HistorialClienteWindow(cliente.Id, cliente.Nombre);
            ventana.ShowDialog();
        }

        private void BtnCerrarModal_Click(object sender, RoutedEventArgs e)
        {
            var fadeOut = (Storyboard)this.Resources["FadeOutModal"];
            fadeOut.Begin();
        }

        private async void TxtFormCedula_LostFocus(object sender, RoutedEventArgs e)
        {
            string cedula = TxtFormCedula.Text.Trim();
            if (string.IsNullOrWhiteSpace(cedula)) return;

            try
            {
                using (var context = new AppDbContext())
                {
                    var cliente = await context.Clientes.FirstOrDefaultAsync(c => c.Cedula == cedula);
                    if (cliente != null)
                    {
                        TxtFormNombre.Text = cliente.Nombre;
                        TxtFormTelefono.Text = cliente.Telefono;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error buscando cliente: {ex.Message}");
            }
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

        private void TxtForm_TextChanged(object sender, TextChangedEventArgs e)
        {
            decimal monto = 0, interes = 0;
            int semanas = 0;
            
            decimal.TryParse(TxtFormMonto?.Text, out monto);
            decimal.TryParse(TxtFormInteres?.Text, out interes);
            int.TryParse(TxtFormSemanas?.Text, out semanas);

            if (semanas > 0)
            {
                decimal total = monto + (monto * (interes / 100));
                decimal cuota = total / semanas;
                
                if (TxtFormTotalCalc != null) TxtFormTotalCalc.Text = $"Total a Pagar: {total:C}";
                if (TxtFormCuotaCalc != null) TxtFormCuotaCalc.Text = $"Cuota Semanal: {cuota:C}";
            }
        }

        private async void BtnGuardarSam_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtFormNombre.Text)) return;
            
            decimal monto = 0, interes = 0;
            int semanas = 0;
            
            decimal.TryParse(TxtFormMonto.Text, out monto);
            decimal.TryParse(TxtFormInteres.Text, out interes);
            int.TryParse(TxtFormSemanas.Text, out semanas);

            if (monto <= 0 || semanas <= 0)
            {
                MessageBox.Show("Ingrese monto y cantidad de semanas válidos.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal totalPagar = monto + (monto * (interes / 100));
            decimal cuotaSemanal = totalPagar / semanas;

            var contrato = new ElPretamita.Views.Compartido.ContratoWindow(
                TxtFormNombre.Text.Trim(),
                TxtFormCedula.Text.Trim(),
                monto,
                totalPagar,
                semanas,
                " cuotas semanales consecutivas",
                cuotaSemanal
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

                    var nuevoSam = new Models.Sam
                    {
                        ClienteId = clienteDB.Id,
                        MontoOriginal = monto,
                        InteresPorcentaje = interes,
                        CantidadSemanas = semanas,
                        TotalPagar = totalPagar,
                        CuotaSemanal = cuotaSemanal,
                        SaldoRestante = totalPagar,
                        FechaInicio = fechaActual,
                        FechaVencimientoSiguiente = fechaActual.AddDays(7),
                        Estado = "Activo"
                    };

                    context.Sams.Add(nuevoSam);
                    await context.SaveChangesAsync();
                }

                BtnCerrarModal_Click(this, new RoutedEventArgs());
                await CargarDatos();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error guardando nuevo SAM: {ex.Message}");
                MessageBox.Show("Hubo un error al guardar los datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnAbonar_Click(object sender, RoutedEventArgs e)
        {
            if (DgSams.SelectedItem is Models.Sam samSeleccionado)
            {
                if (samSeleccionado.SaldoRestante <= 0) return;

                decimal montoAPagar = samSeleccionado.CuotaSemanal;
                if (samSeleccionado.SaldoRestante < montoAPagar)
                {
                    montoAPagar = samSeleccionado.SaldoRestante;
                }

                MessageBoxResult result = MessageBox.Show($"¿Desea registrar el pago automático de la cuota semanal por {montoAPagar:C}?", "Confirmar Pago", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var context = new AppDbContext())
                        {
                            var samDb = await context.Sams
                                .Include(s => s.Cliente)
                                .FirstOrDefaultAsync(s => s.Id == samSeleccionado.Id);
                                
                            if (samDb != null)
                            {
                                DateTime fechaActual = ElPretamita.Services.TimeService.GetRealTime();
                                
                                var pago = new PagoSam
                                {
                                    SamId = samDb.Id,
                                    MontoPagado = montoAPagar,
                                    FechaPago = fechaActual
                                };
                                
                                context.PagosSam.Add(pago);
                                
                                samDb.SaldoRestante -= montoAPagar;
                                
                                // Si el saldo restante es menor a 1 (solo decimales), redondear a 0
                                if (samDb.SaldoRestante < 1m)
                                {
                                    samDb.SaldoRestante = 0;
                                }
                                
                                samDb.FechaVencimientoSiguiente = samDb.FechaVencimientoSiguiente.AddDays(7);
                                
                                if (samDb.SaldoRestante <= 0)
                                {
                                    samDb.Estado = "Pagado";
                                }
                                
                                await context.SaveChangesAsync();

                                int totalPagos = await context.PagosSam.CountAsync(p => p.SamId == samDb.Id);

                                Application.Current.Dispatcher.Invoke(() => {
                                    var recibo = new ElPretamita.Views.Compartido.ReciboWindow(
                                        samDb.Cliente.Nombre, 
                                        montoAPagar, 
                                        samDb.SaldoRestante, 
                                        totalPagos, 
                                        samDb.CantidadSemanas, 
                                        fechaActual
                                    );
                                    recibo.ShowDialog();
                                });
                            }
                        }
                        await CargarDatos();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error registrando pago: " + ex.Message);
                    }
                }
            }
        }
    }
}
