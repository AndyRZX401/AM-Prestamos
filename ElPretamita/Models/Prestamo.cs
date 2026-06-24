namespace ElPretamita.Models
{
    public class Prestamo
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }

        public decimal Monto { get; set; }

        public decimal Interes { get; set; }

        public int CantidadCuotas { get; set; }

        public decimal BalancePendiente { get; set; }

        public DateTime FechaPrestamo { get; set; }

        public DateTime FechaVencimientoSiguiente { get; set; }

        public bool AplicaMoraFija { get; set; } // True = Monto fijo, False = Porcentaje
        public decimal ValorMora { get; set; } // Valor del cargo por día de atraso o porcentaje
        public decimal TotalMoraAcumulada { get; set; } // Cuánta mora se le ha sumado en total

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool RiesgoDeMora => (FechaVencimientoSiguiente - ElPretamita.Services.TimeService.GetRealTime()).TotalHours <= 24 && BalancePendiente > 0;

        public Cliente Cliente { get; set; } = default!;

        public ICollection<CuotaPrestamo> Cuotas { get; set; } = new List<CuotaPrestamo>();
    }
}