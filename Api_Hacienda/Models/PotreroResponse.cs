namespace Api_Hacienda.Models
{
    
    public class PotreroResponse
    {
        public string Identificacion { get; set; } = string.Empty;
        public string TipoPotrero { get; set; } = string.Empty;
        public int CantidadReses { get; set; }
    }
}
