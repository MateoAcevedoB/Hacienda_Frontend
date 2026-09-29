using System;

namespace Api_Hacienda.Models
{
    
    public class VacunaResponse
    {
        public string Nombre { get; set; } = string.Empty;
        public string Lote { get; set; } = string.Empty;
        public DateTime FechaVencimiento { get; set; }
        public DateTime FechaAplicacion { get; set; }
        public string Tipo { get; set; } = string.Empty;

        public uint? PeriodoAplicacion { get; set; }
    }
}
