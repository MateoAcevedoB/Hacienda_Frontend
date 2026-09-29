using System;

namespace Api_Hacienda.Models
{
    
    public class VentaResponse
    {
        public string PotreroId { get; set; } = string.Empty;
        public string NombreRes { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public uint Monto { get; set; }
    }
}
