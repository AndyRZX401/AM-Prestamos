using Microsoft.EntityFrameworkCore;
using ElPretamita.Models;

namespace ElPretamita.DataBase
{
    public class AppDbContext : DbContext
    {
        public DbSet<Cliente> Clientes { get; set; } = default!;

        public DbSet<Sam> Sams { get; set; } = default!;

        public DbSet<PagoSam> PagosSam { get; set; } = default!;

        public DbSet<Prestamo> Prestamos { get; set; } = default!;

        public DbSet<CuotaPrestamo> CuotasPrestamo { get; set; } = default!;

        public DbSet<Usuario> Usuarios { get; set; } = default!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                "Server=.\\SQLEXPRESS;Database=ElPretamitaDB;Trusted_Connection=True;TrustServerCertificate=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Semilla de datos para el usuario administrador inicial
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    Id = 1,
                    NombreUsuario = "admin",
                    Contrasena = ElPretamita.Helpers.SecurityHelper.HashPassword("admin1234"),
                    Rol = "Administrador"
                }
            );
        }
    }
}