using Bib_Hacienda.Dominio;
using Bib_Hacienda.Dominio.Reglas;
using Xunit;

namespace Bib_Hacienda.Tests.Dominio
{

    public class CebonTests
    {
        [Theory]
        [InlineData(12, false)]
        [InlineData(13, true)]
        [InlineData(30, true)]
        [InlineData(48, true)]
        [InlineData(49, false)]
        public void EsEdadValida_DebeRespetarRangoDeCebon(ushort edad, bool esperado)
        {
            var cebon = new Cebon("Cebon1", 300, 20);
            Assert.Equal(esperado, cebon.EsEdadValida(edad));
        }

        [Fact]
        public void PropiedadesPolimorficas_DebenCoincidirConReglaRes()
        {
            var cebon = new Cebon("Cebon1", 300, 20);

            Assert.Equal(ReglaRes.peso_min_cebon, cebon.PesoMinimo);
            Assert.Equal(ReglaRes.peso_recom_venta_cebon, cebon.PesoRecomendadoVenta);
            Assert.Equal(ReglaVacuna.max_bac_cebon, cebon.MaxVacunasBacterianas);
            Assert.Equal(ReglaVacuna.max_viv_cebon, cebon.MaxVacunasVivas);
        }

        [Fact]
        public void Constructor_DebeAsignarNombrePesoYEdad()
        {
            var cebon = new Cebon("Cebon1", 300, 20);

            Assert.Equal("Cebon1", cebon.Nombre);
            Assert.Equal((uint)300, cebon.Peso);
            Assert.Equal((ushort)20, cebon.Edad);
            Assert.Equal("Cebon1", cebon.Identificador);
            Assert.Empty(cebon.L_vacunas_aplicadas);
        }
    }

    public class NovilloTests
    {
        [Theory]
        [InlineData(47, false)]
        [InlineData(48, false)]
        [InlineData(49, true)]
        [InlineData(100, true)]
        public void EsEdadValida_DebeRespetarRangoDeNovillo(ushort edad, bool esperado)
        {
            var novillo = new Novillo("Novillo1", 450, 60);
            Assert.Equal(esperado, novillo.EsEdadValida(edad));
        }

        [Fact]
        public void PropiedadesPolimorficas_DebenCoincidirConReglaRes()
        {
            var novillo = new Novillo("Novillo1", 450, 60);

            Assert.Equal(ReglaRes.peso_min_novillo, novillo.PesoMinimo);
            Assert.Equal(ReglaRes.peso_recom_venta_novillo, novillo.PesoRecomendadoVenta);
            Assert.Equal(ReglaVacuna.max_bac_novillo, novillo.MaxVacunasBacterianas);
            Assert.Equal(ReglaVacuna.max_viv_novillo, novillo.MaxVacunasVivas);
        }

        [Fact]
        public void Constructor_DebeAsignarNombrePesoYEdad()
        {
            var novillo = new Novillo("Novillo1", 450, 60);

            Assert.Equal("Novillo1", novillo.Nombre);
            Assert.Equal((uint)450, novillo.Peso);
            Assert.Equal((ushort)60, novillo.Edad);
            Assert.Equal("Novillo1", novillo.Identificador);
            Assert.Empty(novillo.L_vacunas_aplicadas);
        }
    }
}
