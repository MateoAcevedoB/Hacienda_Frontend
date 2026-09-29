using System.ComponentModel.DataAnnotations;

namespace Api_Hacienda.Models
{
    
    public class ResRequest
    {
        
        [Required(ErrorMessage = "El nombre de la res es obligatorio.")]
        [StringLength(200, ErrorMessage = "El nombre no puede superar los 200 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        // NO se agrega [Range] sobre Edad/Peso: ValidadorRes.EsValido ya
        // exige Peso > 0 y Edad > 0 como regla de NEGOCIO (ver
        // Bib_Hacienda/Dominio/Validacion/ValidadorRes.cs), aplicada más
        // adelante por el repositorio antes de persistir. Duplicarla aquí
        // como [Range] contradiría la instrucción explícita de la fase de
        // no repetir reglas de negocio ya implementadas.
        public ushort Edad { get; set; }
        public uint Peso { get; set; }
    }
}
