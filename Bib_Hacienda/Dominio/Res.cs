using Bib_Hacienda.Dominio.Eventos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bib_Hacienda.Dominio
{
    public abstract class Res : IActivoVendible
    {
        private string nombre;
        private uint peso;
        private ushort edad;
        private List<Vacuna> l_vacunas_aplicadas;

        public Res(string nombre, uint peso, ushort edad)
        {
            this.Nombre = nombre;
            this.Peso = peso;
            this.Edad = edad;
            this.l_vacunas_aplicadas = new List<Vacuna>();
        }

       
        public int Id { get; private set; }

        public string Nombre { get => nombre; set => nombre = value; }
        public uint Peso { get => peso; set => peso = value; }
        public ushort Edad { get => edad; set => edad = value; }
        public List<Vacuna> L_vacunas_aplicadas { get => l_vacunas_aplicadas; set => l_vacunas_aplicadas = value; }

        public string Identificador => Nombre;

        public abstract ushort PesoMinimo { get; }
        public abstract ushort PesoRecomendadoVenta { get; }
        public abstract byte MaxVacunasBacterianas { get; }
        public abstract byte MaxVacunasVivas { get; }
        public abstract bool EsEdadValida(ushort edad);
    }
}
