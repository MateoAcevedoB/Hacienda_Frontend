namespace Bib_Hacienda.Dominio.Validacion
{
    public class ValidadorEventoClinico : IValidadorEventoClinico
    {
        public bool EsValido(EventoClinico evento)
        {
            if (evento == null
                || string.IsNullOrWhiteSpace(evento.PotreroId)
                || string.IsNullOrWhiteSpace(evento.NombreRes)
                || string.IsNullOrWhiteSpace(evento.TipoEvento)
                || string.IsNullOrWhiteSpace(evento.Descripcion))
            {
                return false;
            }
            return true;
        }
    }
}
