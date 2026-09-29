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

        // Solo para vacunas Bacteriana.
        public uint? PeriodoAplicacion { get; set; }

        // Solo para vacunas Viva. Valores reales del dominio: 10, 20, 30
        // (Viva.enum_l_atenuaciones). NO se agrega [Range(10,30)] aquí:
        // los únicos valores válidos del enum ya se verifican en
        // VacunasController.Crear con Enum.IsDefined (ver comentario ahí),
        // que es más preciso que un rango numérico (10/20/30 no son un
        // rango continuo, un [Range(10,30)] dejaría pasar 15 o 25).
        public int? AtenuacionGrado { get; set; }
    }
}
