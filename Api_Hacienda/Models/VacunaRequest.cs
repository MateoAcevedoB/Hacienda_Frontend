using System;
using System.ComponentModel.DataAnnotations;

namespace Api_Hacienda.Models
{
    
    public class VacunaRequest
    {
        
        [Required(ErrorMessage = "El nombre de la vacuna es obligatorio.")]
        [StringLength(200, ErrorMessage = "El nombre no puede superar los 200 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El lote de la vacuna es obligatorio.")]
        [StringLength(100, ErrorMessage = "El lote no puede superar los 100 caracteres.")]
        public string Lote { get; set; } = string.Empty;

        public DateTime FechaVencimiento { get; set; }
        public DateTime FechaAplicacion { get; set; }

        public uint? PeriodoAplicacion { get; set; }

        public int? AtenuacionGrado { get; set; }
    }
}
