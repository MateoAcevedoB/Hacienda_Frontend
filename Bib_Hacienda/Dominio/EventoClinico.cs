using System;

namespace Bib_Hacienda.Dominio
{
    public class EventoClinico
    {
        private string potreroId;
        private string nombreRes;
        private DateTime fecha;
        private string tipoEvento;
        private string descripcion;

        public EventoClinico(string potreroId, string nombreRes, DateTime fecha, string tipoEvento, string descripcion)
        {
            this.PotreroId = potreroId;
            this.NombreRes = nombreRes;
            this.Fecha = fecha;
            this.TipoEvento = tipoEvento;
            this.Descripcion = descripcion;
        }

       
        public int Id { get; private set; }

        public string PotreroId { get => potreroId; set => potreroId = value; }
        public string NombreRes { get => nombreRes; set => nombreRes = value; }
        public DateTime Fecha { get => fecha; set => fecha = value; }
        public string TipoEvento { get => tipoEvento; set => tipoEvento = value; }
        public string Descripcion { get => descripcion; set => descripcion = value; }
    }
}
