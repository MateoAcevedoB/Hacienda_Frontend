using System.ComponentModel.DataAnnotations;

namespace Api_Hacienda.Models
{
    
    public class ResRequest
    {
        
        [Required(ErrorMessage = "El nombre de la res es obligatorio.")]
        [StringLength(200, ErrorMessage = "El nombre no puede superar los 200 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        public ushort Edad { get; set; }
        public uint Peso { get; set; }
    }
}
