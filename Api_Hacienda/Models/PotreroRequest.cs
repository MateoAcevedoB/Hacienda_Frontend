using System.ComponentModel.DataAnnotations;

namespace Api_Hacienda.Models
{
   
    public class PotreroRequest
    {
        
        [Required(ErrorMessage = "La identificación del potrero es obligatoria.")]
        [StringLength(100, ErrorMessage = "La identificación no puede superar los 100 caracteres.")]
        public string Identificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de potrero es obligatorio.")]
        [StringLength(100, ErrorMessage = "El tipo de potrero no puede superar los 100 caracteres.")]
        public string TipoPotrero { get; set; } = string.Empty;
    }
}
