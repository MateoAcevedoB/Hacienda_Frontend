using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bib_Hacienda.Dominio
{
    public class Venta
    {
        private Potrero potrero;
        private DateTime fecha;
        private IActivoVendible activo;
        private uint monto;

        public Venta(Potrero potrero, DateTime fecha, IActivoVendible activo, uint monto)
        {
            this.Potrero = potrero;
            this.Fecha = fecha;
            this.Activo = activo;
            this.Monto = monto;
        }

       
        private Venta()
        {
        }

        public Potrero Potrero { get => potrero; set => potrero = value; }
        public DateTime Fecha { get => fecha; set => fecha = value; }
        public IActivoVendible Activo { get => activo; set => activo = value; }
        public uint Monto { get => monto; set => monto = value; }
    }
}
