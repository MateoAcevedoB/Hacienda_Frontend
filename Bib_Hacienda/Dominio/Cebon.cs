using Bib_Hacienda.Dominio.Reglas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bib_Hacienda.Dominio
{
    public class Cebon : Res
    {
        public Cebon(string nombre, uint peso, ushort edad) : base(nombre, peso, edad)
        {
        }

        public override bool EsEdadValida(ushort edad)
        {
            return edad > ReglaRes.edad_max_ternero && edad <= ReglaRes.edad_max_cebon;
        }

        public override ushort PesoMinimo => ReglaRes.peso_min_cebon;
        public override ushort PesoRecomendadoVenta => ReglaRes.peso_recom_venta_cebon;
        public override byte MaxVacunasBacterianas => ReglaVacuna.max_bac_cebon;
        public override byte MaxVacunasVivas => ReglaVacuna.max_viv_cebon;
    }
}
