using System.ComponentModel.DataAnnotations;

namespace Api_Hacienda.Models
{
    
    // PotreroId + NombreRes: misma identificación compuesta que el resto de
    // la API (ver PotrerosController, ENDPOINT 6 de FASE 3A). No se agrega
    // ResId: el dominio no lo tiene como identificador de negocio.
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
