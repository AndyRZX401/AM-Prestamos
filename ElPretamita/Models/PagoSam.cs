using System;

namespace ElPretamita.Models
{
    public class PagoSam
    {
        public int Id { get; set; }
        
        public int SamId { get; set; }
        
        public decimal MontoPagado { get; set; }
        
        public DateTime FechaPago { get; set; }
        
        public Sam Sam { get; set; } = default!;
    }
}
