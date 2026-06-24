using System.Windows;
using ElPretamita.Views.Login;
using ElPretamita.DataBase;
using Microsoft.EntityFrameworkCore;

namespace ElPretamita
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Asegurar que la base de datos esté creada y actualizada al iniciar
                using (var context = new AppDbContext())
                {
                    await context.Database.MigrateAsync();
                }

                // Iniciar sincronización de hora anti-fraude (no bloquea UI)
                await ElPretamita.Services.TimeService.InitializeAsync();

                // Iniciar aplicación con el Login
                LoginWindow login = new LoginWindow();
                login.Show();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error crítico al iniciar la aplicación:\n\n{ex.Message}\n\nDetalles:\n{ex.StackTrace}", "Error Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}