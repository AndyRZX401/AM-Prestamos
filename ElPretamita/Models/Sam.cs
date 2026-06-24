using System;
using System.Collections.Generic;

namespace ElPretamita.Models
{
    public class Sam
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }

        public decimal MontoOriginal { get; set; }
        public decimal InteresPorcentaje { get; set; }
        public int CantidadSemanas { get; set; }
        
        public decimal TotalPagar { get; set; }
        public decimal CuotaSemanal { get; set; }
        public decimal SaldoRestante { get; set; }

        public DateTime FechaInicio { get; set; }
        
        public DateTime FechaVencimientoSiguiente { get; set; }

        public string Estado { get; set; } = "Activo";

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool RiesgoDeMora => (FechaVencimientoSiguiente - ElPretamita.Services.TimeService.GetRealTime()).TotalHours <= 24 && Estado != "Pagado";

        public Cliente Cliente { get; set; } = default!;
        
        public ICollection<PagoSam> Pagos { get; set; } = new List<PagoSam>();
    }
}