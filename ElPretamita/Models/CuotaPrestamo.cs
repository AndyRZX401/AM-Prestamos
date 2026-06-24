namespace ElPretamita.Models
{
    public class CuotaPrestamo
    {
        public int Id { get; set; }

        public int PrestamoId { get; set; }

        public decimal MontoCuota { get; set; }

        public DateTime FechaPago { get; set; }

        public bool Pagada { get; set; }

        public DateTime? FechaRealPago { get; set; }

        public Prestamo Prestamo { get; set; } = default!;
    }
}