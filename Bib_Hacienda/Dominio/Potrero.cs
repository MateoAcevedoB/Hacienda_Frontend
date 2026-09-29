using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bib_Hacienda.Dominio.Eventos;
using Bib_Hacienda.Dominio.Reglas;

namespace Bib_Hacienda.Dominio
{
    
    public class Potrero
    {
        private string identificacion;
        private List<Res> l_reses;
        private string tipo_potrero;

        private PublisherPotreroMitad publisher_potrero_mitad;
        private PublisherPotreroLleno publisher_potrero_lleno;

        public Potrero(string identificacion, string tipo_potrero, PublisherPotreroMitad potreroMitad, PublisherPotreroLleno potreroLleno)
        {
            this.Identificacion = identificacion;
            this.Tipo_potrero = tipo_potrero;
            this.publisher_potrero_mitad = potreroMitad;
            this.publisher_potrero_lleno = potreroLleno;
            this.l_reses = new List<Res>();
        }

        
        private Potrero()
        {
    this.l_reses = new List<Res>();
    this.publisher_potrero_mitad = new PublisherPotreroMitad();
    this.publisher_potrero_lleno = new PublisherPotreroLleno();
        }

        public string Identificacion { get => identificacion; set => identificacion = value; }
        public List<Res> L_reses { get => l_reses; set => l_reses = value; }
        public string Tipo_potrero { get => tipo_potrero; set => tipo_potrero = value; }

        public bool TieneCapacidad()
        {
            return l_reses.Count() < ReglaPotrero.max_reses_potrero;
        }

        public ResultadoOperacion AgregarRes(Res res)
        {
            try
            {
                if (res == null)
                {
                    return ResultadoOperacion.Fallo("La res a añadir no puede ser nula");
                }

                if (!TieneCapacidad())
                {
                    return ResultadoOperacion.Fallo($"La res no puede ser añadida al potrero {this.identificacion} porque este está lleno");
                }

                l_reses.Add(res);

                ushort cantidad_reses = (ushort)L_reses.Count();

                string mensajes_eventos = "";

                publisher_potrero_mitad.evt_potrero_mitad += mensaje =>
                {
                    if (!string.IsNullOrEmpty(mensaje))
                        mensajes_eventos += mensaje + "\n";
                };

                publisher_potrero_lleno.evt_potrero_lleno += mensaje =>
                {
                    if (!string.IsNullOrEmpty(mensaje))
                        mensajes_eventos += mensaje + "\n";
                };

                publisher_potrero_mitad.Informar_Potrero_Mitad(cantidad_reses, this);
                publisher_potrero_lleno.Informar_Potrero_Lleno(cantidad_reses, this);

                string mensaje_final = $"La res {res.Nombre} ha sido añadida al potrero {this.identificacion} con exito.";
                if (!string.IsNullOrEmpty(mensajes_eventos))
                {
                    mensaje_final += "\n" + mensajes_eventos.TrimEnd();
                }

                return ResultadoOperacion.Ok(mensaje_final);
            }
            catch (Exception ex)
            {
                return ResultadoOperacion.Fallo("Error inesperado en el metodo AgregarRes: " + ex.Message);
            }
        }

        
        public Res BuscarRes(string nombre)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    throw new ArgumentException("El nombre de búsqueda no puede estar vacío.");
                }

                var res_encontrada = l_reses
                    .Where(p => p.Nombre.IndexOf(nombre, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                if (res_encontrada.Count == 0)
                {
                    throw new Exception($"No se encontró ningúna vaca con el nombre o coincidencia '{nombre}'.");
                }

                if (res_encontrada.Count > 1)
                {
                    throw new Exception($" se encontró mas de una res con el nombre o coincidencia '{nombre}'.");
                }

                return res_encontrada.First();
            }
            catch (Exception er)
            {
                throw new Exception("Error inesperado en el método buscar_potrero: " + er.Message);
            }
        }
    }
}
