using System;
using Bib_Hacienda.Dominio;
using Bib_Hacienda.Dominio.Eventos;
using Bib_Hacienda.Dominio.Reglas;
using Xunit;

namespace Bib_Hacienda.Tests.Dominio
{
    public class PotreroTests
    {
        private static Potrero NuevoPotrero(string id = "P1")
        {
            return new Potrero(id, "ternero", new PublisherPotreroMitad(), new PublisherPotreroLleno());
        }

        private static void LlenarConReses(Potrero potrero, int cantidad)
        {
            for (int i = 0; i < cantidad; i++)
            {
                potrero.AgregarRes(new Ternero($"Res{i}", 200, 5));
            }
        }

        [Fact]
        public void TieneCapacidad_PotreroVacio_DebeRetornarTrue()
        {
            var potrero = NuevoPotrero();
            Assert.True(potrero.TieneCapacidad());
        }

        [Fact]
        public void TieneCapacidad_PotreroLleno_DebeRetornarFalse()
        {
            var potrero = NuevoPotrero();
            LlenarConReses(potrero, ReglaPotrero.max_reses_potrero);

            Assert.False(potrero.TieneCapacidad());
        }

        [Fact]
        public void AgregarRes_ResNula_DebeRetornarFallo()
        {
            var potrero = NuevoPotrero();

            ResultadoOperacion resultado = potrero.AgregarRes(null!);

            Assert.False(resultado.Exito);
            Assert.Equal("La res a añadir no puede ser nula", resultado.Mensaje);
            Assert.Empty(potrero.L_reses);
        }

        [Fact]
        public void AgregarRes_ConCapacidad_DebeRetornarExitoYAgregarLaRes()
        {
            var potrero = NuevoPotrero("Potrero1");
            var res = new Ternero("Vaca1", 200, 5);

            ResultadoOperacion resultado = potrero.AgregarRes(res);

            Assert.True(resultado.Exito);
            Assert.Contains("Vaca1", resultado.Mensaje);
            Assert.Contains("Potrero1", resultado.Mensaje);
            Assert.Single(potrero.L_reses);
            Assert.Same(res, potrero.L_reses[0]);
        }

        [Fact]
        public void AgregarRes_AlAlcanzarLaMitad_DebeIncluirMensajeDeEventoDeMitad()
        {
            var potrero = NuevoPotrero("Potrero1");
            LlenarConReses(potrero, ReglaPotrero.max_reses_potrero / 2 - 1);

            ResultadoOperacion resultado = potrero.AgregarRes(new Ternero("ResMitad", 200, 5));

            Assert.True(resultado.Exito);
            Assert.Contains("ha alcanzado la mitad de su capacidad", resultado.Mensaje);
        }

        [Fact]
        public void AgregarRes_AlAlcanzarElMaximo_DebeIncluirMensajeDeEventoDeLleno()
        {
            var potrero = NuevoPotrero("Potrero1");
            LlenarConReses(potrero, ReglaPotrero.max_reses_potrero - 1);

            ResultadoOperacion resultado = potrero.AgregarRes(new Ternero("ResFinal", 200, 5));

            Assert.True(resultado.Exito);
            Assert.Contains("ha alcanzado su capacidad máxima de reses", resultado.Mensaje);
        }

        [Fact]
        public void AgregarRes_PotreroYaLleno_DebeRetornarFallo()
        {
            var potrero = NuevoPotrero("Potrero1");
            LlenarConReses(potrero, ReglaPotrero.max_reses_potrero);

            ResultadoOperacion resultado = potrero.AgregarRes(new Ternero("ResExtra", 200, 5));

            Assert.False(resultado.Exito);
            Assert.Contains("está lleno", resultado.Mensaje);
            Assert.Equal(ReglaPotrero.max_reses_potrero, potrero.L_reses.Count);
        }

        [Fact]
        public void BuscarRes_NombreVacio_DebeLanzarExcepcion()
        {
            var potrero = NuevoPotrero();

            var ex = Assert.Throws<Exception>(() => potrero.BuscarRes("   "));
            Assert.Contains("no puede estar vacío", ex.Message);
        }

        [Fact]
        public void BuscarRes_SinCoincidencias_DebeLanzarExcepcion()
        {
            var potrero = NuevoPotrero();
            potrero.AgregarRes(new Ternero("Vaca1", 200, 5));

            var ex = Assert.Throws<Exception>(() => potrero.BuscarRes("Inexistente"));
            Assert.Contains("No se encontró", ex.Message);
        }

        [Fact]
        public void BuscarRes_MultiplesCoincidencias_DebeLanzarExcepcion()
        {
            var potrero = NuevoPotrero();
            potrero.AgregarRes(new Ternero("Vaca1", 200, 5));
            potrero.AgregarRes(new Ternero("Vaca10", 200, 5));

            var ex = Assert.Throws<Exception>(() => potrero.BuscarRes("Vaca1"));
            Assert.Contains("mas de una res", ex.Message);
        }

        [Fact]
        public void BuscarRes_UnaCoincidencia_DebeRetornarLaRes()
        {
            var potrero = NuevoPotrero();
            var res = new Ternero("VacaUnica", 200, 5);
            potrero.AgregarRes(res);

            Res encontrada = potrero.BuscarRes("VacaUnica");

            Assert.Same(res, encontrada);
        }

        [Fact]
        public void BuscarRes_CoincidenciaParcialIgnorandoMayusculas_DebeRetornarLaRes()
        {
            var potrero = NuevoPotrero();
            var res = new Ternero("VacaEspecial", 200, 5);
            potrero.AgregarRes(res);

            Res encontrada = potrero.BuscarRes("especial");

            Assert.Same(res, encontrada);
        }
    }
}
