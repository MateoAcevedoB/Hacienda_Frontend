namespace Api_Hacienda.Models
{
    
    public class ResResponse
    {
        public string Nombre { get; set; } = string.Empty;
        public uint Peso { get; set; }
        public ushort Edad { get; set; }
        public string Tipo { get; set; } = string.Empty;
    }
}
