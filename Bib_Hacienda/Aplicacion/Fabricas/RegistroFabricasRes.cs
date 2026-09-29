using System;
using System.Collections.Generic;

namespace Bib_Hacienda.Aplicacion
{
    public class RegistroFabricasRes : IRegistroFabricasRes
    {
        private Dictionary<string, IFabricaRes> fabricas;

        public RegistroFabricasRes(Dictionary<string, IFabricaRes> fabricas)
        {
            this.fabricas = fabricas;
        }

        public IFabricaRes ObtenerFabrica(string tipo)
        {
            if (tipo == null || !fabricas.TryGetValue(tipo, out IFabricaRes fabrica))
            {
                throw new Exception($"No existe una fábrica registrada para el tipo de res '{tipo}'.");
            }
            return fabrica;
        }
    }
}
