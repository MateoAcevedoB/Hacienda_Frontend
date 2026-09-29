using System.ComponentModel.DataAnnotations;

namespace Api_Hacienda.Models
{
    
    public class AplicarVacunaRequest
    {
        
        [Required(ErrorMessage = "El identificador del potrero es obligatorio.")]
        public string PotreroId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de la res es obligatorio.")]
        public string NombreRes { get; set; } = string.Empty;

        [Required(ErrorMessage = "El lote de la vacuna es obligatorio.")]
        public string LoteVacuna { get; set; } = string.Empty;
    }
}
