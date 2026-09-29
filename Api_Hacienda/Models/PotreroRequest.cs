using System.ComponentModel.DataAnnotations;

namespace Api_Hacienda.Models
{
   
    public class PotreroRequest
    {
        
        [Required(ErrorMessage = "La identificación del potrero es obligatoria.")]
        [StringLength(100, ErrorMessage = "La identificación no puede superar los 100 caracteres.")]
        public string Identificacion { get; set; } = string.Empty;

        // Mismo valor de texto que espera IRegistroFabricasRes al añadir una
        // Res a este potrero (ver RegistroFabricasRes/Program.cs): "ternero",
        // "cebon" o "novillo". [StringLength(100)] refleja el mismo límite
        // que HaciendaDbContext.ConfigurarPotrero (Tipo_potrero
        // HasMaxLength(100)). NO se valida aquí la pertenencia al conjunto
        // {ternero, cebon, novillo} con [RegularExpression]/enum: esa
        // responsabilidad ya existe más adelante, en
        // IRegistroFabricasRes.ObtenerFabrica, cuando efectivamente se
        // necesita (al añadir una Res); duplicarla en el DTO repetiría una
        // regla que no está declarada en ServicioPotreros ni en
        // ValidadorPotrero (ver diagnóstico previo).
        [Required(ErrorMessage = "El tipo de potrero es obligatorio.")]
        [StringLength(100, ErrorMessage = "El tipo de potrero no puede superar los 100 caracteres.")]
        public string TipoPotrero { get; set; } = string.Empty;
    }
}
