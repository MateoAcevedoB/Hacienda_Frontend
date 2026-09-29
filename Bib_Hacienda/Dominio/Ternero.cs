using Bib_Hacienda.Dominio.Reglas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bib_Hacienda.Dominio
{
    public class Ternero : Res
    {
        public Ternero(string nombre, uint peso, ushort edad) : base(nombre, peso, edad)
        {
        }

        public override bool EsEdadValida(ushort edad)
        {
            return edad <= ReglaRes.edad_max_ternero;
        }

        public override ushort PesoMinimo => ReglaRes.peso_min_ternero;
        public override ushort PesoRecomendadoVenta => ReglaRes.peso_recom_venta_ternero;
        public override byte MaxVacunasBacterianas => ReglaVacuna.max_bac_ternero;
        public override byte MaxVacunasVivas => ReglaVacuna.max_viv_ternero;
    }
}
