using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.EntityFrameworkCore;
using ElPretamita.DataBase;
using ElPretamita.Models;

namespace ElPretamita.Views.Login
{
    public partial class LoginWindow : Window
    {
        private bool _isUpdatingPassword = false;

        public LoginWindow()
        {
            InitializeComponent();
            
            CargarLogoPersonalizado();
            CargarCredenciales();

            // Asegurar estado inicial correcto de las marcas de agua
            this.Loaded += (s, e) =>
            {
                ActualizarUsuarioWatermarkEstado();
                ActualizarPasswordWatermarkEstado();
            };
        }

        private void GuardarCredenciales(string usuario, string password)
        {
            try
            {
                string appDataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ElPretamita");
                if (!System.IO.Directory.Exists(appDataPath)) System.IO.Directory.CreateDirectory(appDataPath);
                
                string credPath = System.IO.Path.Combine(appDataPath, "remember.txt");
                
                if (ChkRecordarme.IsChecked == true)
                {
                    // Codificar en Base64 para que no esté en texto plano
                    string credentials = $"{usuario}|{password}";
                    var bytes = System.Text.Encoding.UTF8.GetBytes(credentials);
                    string base64 = Convert.ToBase64String(bytes);
                    System.IO.File.WriteAllText(credPath, base64);
                }
                else
                {
                    if (System.IO.File.Exists(credPath))
                        System.IO.File.Delete(credPath);
                }
            }
            catch { }
        }

        private void CargarCredenciales()
        {
            try
            {
                string appDataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ElPretamita");
                string credPath = System.IO.Path.Combine(appDataPath, "remember.txt");

                if (System.IO.File.Exists(credPath))
                {
                    string base64 = System.IO.File.ReadAllText(credPath);
                    var bytes = Convert.FromBase64String(base64);
                    string credentials = System.Text.Encoding.UTF8.GetString(bytes);
                    string[] parts = credentials.Split('|');
                    if (parts.Length == 2)
                    {
                        TxtUsuario.Text = parts[0];
                        TxtPassword.Password = parts[1];
                        ChkRecordarme.IsChecked = true;
                    }
                }
            }
            catch { }
        }

        private void CargarLogoPersonalizado()
        {
            try
            {
                string appDataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ElPretamita");
                string customLogoPath = System.IO.Path.Combine(appDataPath, "logo_custom.png");

                if (System.IO.File.Exists(customLogoPath))
                {
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(customLogoPath);
                    bitmap.EndInit();

                    ImgLogo.Source = bitmap;
                }
            }
            catch { }
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

        // Método de animación suave para la opacidad de las marcas de agua (Fade In / Fade Out)
        private void AnimarOpacidadWatermark(TextBlock watermark, bool visible)
        {
            if (watermark == null) return;

            double targetOpacity = visible ? 1.0 : 0.0;
            
            // Si ya está en la opacidad deseada, no volvemos a iniciar la animación
            if (Math.Abs(watermark.Opacity - targetOpacity) < 0.01) return;

            Duration duration = new Duration(TimeSpan.FromSeconds(0.25));
            DoubleAnimation anim = new DoubleAnimation(targetOpacity, duration)
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            watermark.BeginAnimation(UIElement.OpacityProperty, anim);
        }

        // Controladores para el campo Usuario
        private void TxtUsuario_GotFocus(object sender, RoutedEventArgs e) => ActualizarUsuarioWatermarkEstado();
        private void TxtUsuario_LostFocus(object sender, RoutedEventArgs e) => ActualizarUsuarioWatermarkEstado();
        private void TxtUsuario_TextChanged(object sender, TextChangedEventArgs e) => ActualizarUsuarioWatermarkEstado();

        private void ActualizarUsuarioWatermarkEstado()
        {
            // Ocultar si tiene foco o si ya tiene texto escrito
            bool visible = string.IsNullOrEmpty(TxtUsuario.Text) && !TxtUsuario.IsFocused;
            AnimarOpacidadWatermark(TxtUsuarioWatermark, visible);
        }

        // Sincroniza y controla la marca de agua de contraseña
        private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingPassword) return;

            _isUpdatingPassword = true;
            TxtPasswordUnmasked.Text = TxtPassword.Password;
            _isUpdatingPassword = false;

            ActualizarPasswordWatermarkEstado();
        }

        private void TxtPasswordUnmasked_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingPassword) return;

            _isUpdatingPassword = true;
            TxtPassword.Password = TxtPasswordUnmasked.Text;
            _isUpdatingPassword = false;

            ActualizarPasswordWatermarkEstado();
        }

        private void TxtPassword_GotFocus(object sender, RoutedEventArgs e) => ActualizarPasswordWatermarkEstado();
        private void TxtPassword_LostFocus(object sender, RoutedEventArgs e) => ActualizarPasswordWatermarkEstado();
        private void TxtPasswordUnmasked_GotFocus(object sender, RoutedEventArgs e) => ActualizarPasswordWatermarkEstado();
        private void TxtPasswordUnmasked_LostFocus(object sender, RoutedEventArgs e) => ActualizarPasswordWatermarkEstado();

        private void ActualizarPasswordWatermarkEstado()
        {
            // Ocultar si tiene texto o alguno de los campos de contraseña tiene el foco
            bool tieneTexto = !string.IsNullOrEmpty(TxtPassword.Password) || !string.IsNullOrEmpty(TxtPasswordUnmasked.Text);
            bool algunEnfocado = TxtPassword.IsFocused || TxtPasswordUnmasked.IsFocused;
            bool visible = !tieneTexto && !algunEnfocado;
            AnimarOpacidadWatermark(TxtPasswordWatermark, visible);
        }

        // Muestra/Oculta la contraseña (botón ojo con formas vectoriales abierta/cerrada)
        private void BtnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            if (TxtPassword.Visibility == Visibility.Visible)
            {
                // Mostrar en texto plano (ojo abierto visible)
                TxtPassword.Visibility = Visibility.Collapsed;
                TxtPasswordUnmasked.Visibility = Visibility.Visible;
                PathEyeOpen.Visibility = Visibility.Visible;
                PathEyeClosed.Visibility = Visibility.Collapsed;

                TxtPasswordUnmasked.Focus();
                TxtPasswordUnmasked.SelectionStart = TxtPasswordUnmasked.Text.Length;
            }
            else
            {
                // Ocultar/Mascarar contraseña (ojo cerrado visible)
                TxtPassword.Visibility = Visibility.Visible;
                TxtPasswordUnmasked.Visibility = Visibility.Collapsed;
                PathEyeOpen.Visibility = Visibility.Collapsed;
                PathEyeClosed.Visibility = Visibility.Visible;

                TxtPassword.Focus();
            }
            ActualizarPasswordWatermarkEstado();
        }

        // Muestra una alerta con animación de desvanecimiento
        private void MostrarAlerta(Border alerta, string mensaje, TextBlock textoAlerta)
        {
            textoAlerta.Text = mensaje;
            alerta.Visibility = Visibility.Visible;
            Storyboard fadeIn = (Storyboard)this.FindResource("FadeInAlert");
            if (fadeIn != null)
            {
                fadeIn.Begin(alerta);
            }
        }

        // Ocultar una alerta con animación de desvanecimiento
        private void OcultarAlerta(Border alerta)
        {
            if (alerta.Visibility == Visibility.Visible)
            {
                Storyboard fadeOut = (Storyboard)this.FindResource("FadeOutAlert");
                if (fadeOut != null)
                {
                    fadeOut.Begin(alerta);
                }
            }
        }

        // Controla el estado de carga (bloquear inputs y mostrar spinner/barra)
        private void SetLoadingState(bool isLoading)
        {
            TxtUsuario.IsEnabled = !isLoading;
            TxtPassword.IsEnabled = !isLoading;
            BtnIngresar.IsEnabled = !isLoading;

            if (isLoading)
            {
                BtnIngresar.Content = "Validando...";
                LoadingProgress.Visibility = Visibility.Visible;
            }
            else
            {
                BtnIngresar.Content = "Entrar";
                LoadingProgress.Visibility = Visibility.Collapsed;
            }
        }

        // Lógica de inicio de sesión asíncrona interactiva con base de datos
        private async void BtnIngresar_Click(object sender, RoutedEventArgs e)
        {
            string usuario = TxtUsuario.Text.Trim();
            string password = TxtPassword.Password;

            // Ocultar alertas previas
            OcultarAlerta(ErrorAlert);
            OcultarAlerta(SuccessAlert);

            // Validar campos vacíos
            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
            {
                MostrarAlerta(ErrorAlert, "Por favor, completa todos los campos.", ErrorAlertText);
                return;
            }

            // Cambiar a estado de cargando
            SetLoadingState(true);

            // Simular un retardo asíncrono para dar interactividad visual
            await Task.Delay(1000);

            try
            {
                // Conectar y buscar usuario en la base de datos SQL Server
                using (var context = new AppDbContext())
                {
                    // Hashear la contraseña antes de compararla
                    string hashedPassword = ElPretamita.Helpers.SecurityHelper.HashPassword(password);

                    // Consulta LINQ para verificar las credenciales del usuario
                    var user = await context.Usuarios
                        .FirstOrDefaultAsync(u => u.NombreUsuario == usuario && u.Contrasena == hashedPassword);

                    if (user != null)
                    {
                        MostrarAlerta(SuccessAlert, $"¡Acceso concedido! Bienvenido, {user.NombreUsuario}.", SuccessAlertText);

                        // Esperar un breve momento para mostrar el éxito al usuario
                        await Task.Delay(1000);

                        // Guardar o eliminar credenciales según el checkbox
                        GuardarCredenciales(usuario, password);

                        // Abrir la MainWindow y cerrar esta ventana de Login
                        MainWindow main = new MainWindow();
                        Application.Current.MainWindow = main;
                        main.Show();
                        this.Close();
                    }
                    else
                    {
                        // Credenciales incorrectas
                        SetLoadingState(false);
                        MostrarAlerta(ErrorAlert, "Usuario o contraseña incorrectos.", ErrorAlertText);
                    }
                }
            }
            catch (Exception ex)
            {
                SetLoadingState(false);
                // Si hay un fallo de base de datos o de red
                MostrarAlerta(ErrorAlert, "Error al conectar con la base de datos. Asegúrate de que el servidor SQL Server esté en ejecución.", ErrorAlertText);
                System.Diagnostics.Debug.WriteLine($"Error de Base de Datos: {ex.Message}");
            }
        }

        private async void BtnOlvideContrasena_Click(object sender, MouseButtonEventArgs e)
        {
            // Ocultar alertas
            OcultarAlerta(ErrorAlert);
            OcultarAlerta(SuccessAlert);

            string usuario = TxtUsuario.Text.Trim();
            if (string.IsNullOrEmpty(usuario))
            {
                MostrarAlerta(ErrorAlert, "Por favor, escribe tu nombre de usuario arriba y haz clic aquí de nuevo.", ErrorAlertText);
                return;
            }

            // Clave maestra de recuperación
            if (TxtPassword.Password == "PRETAMITA2026" || TxtPasswordUnmasked.Text == "PRETAMITA2026")
            {
                try
                {
                    using (var context = new AppDbContext())
                    {
                        var user = await context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == usuario);
                        if (user != null)
                        {
                            user.Contrasena = ElPretamita.Helpers.SecurityHelper.HashPassword("admin1234");
                            await context.SaveChangesAsync();
                            MostrarAlerta(SuccessAlert, "¡Contraseña reseteada a 'admin1234'! Ya puedes entrar.", SuccessAlertText);
                            TxtPassword.Password = "";
                            TxtPasswordUnmasked.Text = "";
                        }
                        else
                        {
                            MostrarAlerta(ErrorAlert, "Usuario no encontrado en la base de datos.", ErrorAlertText);
                        }
                    }
                }
                catch (Exception)
                {
                    MostrarAlerta(ErrorAlert, "Error al intentar resetear la contraseña.", ErrorAlertText);
                }
            }
            else
            {
                MostrarAlerta(ErrorAlert, "MECANISMO DE RECUPERACIÓN:\nEscribe la clave secreta maestra 'PRETAMITA2026' en la casilla de contraseña y vuelve a hacer clic aquí.", ErrorAlertText);
            }
        }
    }
}