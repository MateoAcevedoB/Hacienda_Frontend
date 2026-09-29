using Bib_Hacienda.Dominio.Reglas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bib_Hacienda.Dominio
{
    public class Novillo : Res
    {
        public Novillo(string nombre, uint peso, ushort edad) : base(nombre, peso, edad)
        {
        }

        public override bool EsEdadValida(ushort edad)
        {
            return edad > ReglaRes.edad_max_cebon;
        }

        public override ushort PesoMinimo => ReglaRes.peso_min_novillo;
        public override ushort PesoRecomendadoVenta => ReglaRes.peso_recom_venta_novillo;
        public override byte MaxVacunasBacterianas => ReglaVacuna.max_bac_novillo;
        public override byte MaxVacunasVivas => ReglaVacuna.max_viv_novillo;
    }
}
