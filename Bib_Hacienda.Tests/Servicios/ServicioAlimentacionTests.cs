using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;
using Bib_Hacienda.Dominio.Eventos;
using Moq;
using Xunit;

namespace Bib_Hacienda.Tests.Servicios
{
    
    public class ServicioAlimentacionTests
    {
        private static ServicioAlimentacion NuevoServicio(Mock<IRepositorioPotreros> repoMock)
        {
            return new ServicioAlimentacion(repoMock.Object, new PublisherPesoMin(), new PublisherPesoVenta());
        }

        private static Potrero NuevoPotrero(string identificacion, string tipo = "ternero")
        {
            return new Potrero(identificacion, tipo, new PublisherPotreroMitad(), new PublisherPotreroLleno());
        }

        [Fact]
        public void AlimentarRes_PotreroNoExiste_DebeRetornarFallo()
        {
            // Arrange
            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.ObtenerPorId("PotreroX")).Returns((Potrero)null!);
            var servicio = NuevoServicio(repoMock);

            // Act
            ResultadoOperacion resultado = servicio.AlimentarRes("PotreroX", "Bessie");

            // Assert
            Assert.False(resultado.Exito);
            Assert.Contains("No se encontró el potrero", resultado.Mensaje);
            repoMock.Verify(r => r.ObtenerPorId("PotreroX"), Times.Once);
        }

        [Fact]
        public void AlimentarRes_ResNoExisteEnElPotrero_DebeRetornarFallo()
        {
            // Arrange: Potrero.BuscarRes lanza si no encuentra coincidencias;
            // AlimentarInterno la atrapa con su catch genérico.
            var potrero = NuevoPotrero("Potrero1");
            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.ObtenerPorId("Potrero1")).Returns(potrero);
            var servicio = NuevoServicio(repoMock);

            // Act
            ResultadoOperacion resultado = servicio.AlimentarRes("Potrero1", "Inexistente");

            // Assert
            Assert.False(resultado.Exito);
            Assert.Contains("Error inesperado en el metodo AlimentarRes", resultado.Mensaje);
        }

        [Fact]
        public void AlimentarRes_SobrecargaSinCantidad_DebeIncrementarPesoEnUno()
        {
            // Arrange
            var res = new Ternero("Bessie", 200, 5);
            var potrero = NuevoPotrero("Potrero1");
            potrero.L_reses.Add(res);

            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.ObtenerPorId("Potrero1")).Returns(potrero);
            var servicio = NuevoServicio(repoMock);

            // Act
            ResultadoOperacion resultado = servicio.AlimentarRes("Potrero1", "Bessie");

            // Assert
            Assert.True(resultado.Exito);
            Assert.Equal(201u, res.Peso);
            Assert.Contains("201", resultado.Mensaje);
        }

        [Fact]
        public void AlimentarRes_SobrecargaConCantidad_DebeIncrementarPesoEnLaCantidadIndicada()
        {
            // Arrange
            var res = new Ternero("Bessie", 200, 5);
            var potrero = NuevoPotrero("Potrero1");
            potrero.L_reses.Add(res);

            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.ObtenerPorId("Potrero1")).Returns(potrero);
            var servicio = NuevoServicio(repoMock);

            // Act
            ResultadoOperacion resultado = servicio.AlimentarRes("Potrero1", "Bessie", 20u);

            // Assert
            Assert.True(resultado.Exito);
            Assert.Equal(220u, res.Peso);
        }

        [Fact]
        public void AlimentarRes_PesoQuedaPorDebajoDelMinimo_DebeIncluirMensajeDeEventoPesoMin()
        {
            // Arrange: ReglaRes.peso_min_ternero = 150; con 100+1 = 101 sigue
            // por debajo, dispara PublisherPesoMin.Informar_Peso_Min real.
            var res = new Ternero("Flaquita", 100, 5);
            var potrero = NuevoPotrero("Potrero1");
            potrero.L_reses.Add(res);

            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.ObtenerPorId("Potrero1")).Returns(potrero);
            var servicio = NuevoServicio(repoMock);

            // Act
            ResultadoOperacion resultado = servicio.AlimentarRes("Potrero1", "Flaquita");

            // Assert
            Assert.True(resultado.Exito);
            Assert.Contains("desnutrición", resultado.Mensaje);
        }

        [Fact]
        public void AlimentarRes_PesoQuedaAptoParaVenta_DebeIncluirMensajeDeEventoPesoVenta()
        {
            // Arrange: ReglaRes.peso_recom_venta_ternero = 250; con 249+1 = 250
            // dispara PublisherPesoVenta.Informar_Peso_Venta real.
            var res = new Ternero("Gorda", 249, 5);
            var potrero = NuevoPotrero("Potrero1");
            potrero.L_reses.Add(res);

            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.ObtenerPorId("Potrero1")).Returns(potrero);
            var servicio = NuevoServicio(repoMock);

            // Act
            ResultadoOperacion resultado = servicio.AlimentarRes("Potrero1", "Gorda");

            // Assert
            Assert.True(resultado.Exito);
            Assert.Contains("apta para venta", resultado.Mensaje);
        }
    }
}
