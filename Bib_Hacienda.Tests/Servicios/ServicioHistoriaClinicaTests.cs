using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;
using Bib_Hacienda.Dominio.Eventos;
using Moq;
using Xunit;

namespace Bib_Hacienda.Tests.Servicios
{
    
    public class ServicioHistoriaClinicaTests
    {
        private static Potrero NuevoPotrero(string identificacion, string tipo = "ternero")
        {
            return new Potrero(identificacion, tipo, new PublisherPotreroMitad(), new PublisherPotreroLleno());
        }

        // ------------------------------------------------------------
        // RegistrarEvento
        // ------------------------------------------------------------

        [Fact]
        public void RegistrarEvento_TipoEventoVacio_DebeRetornarFalloYNoAgregar()
        {
            // Arrange
            var repoPotrerosMock = new Mock<IRepositorioPotreros>();
            var repoHistoriaMock = new Mock<IRepositorioHistoriaClinica>();
            var servicio = new ServicioHistoriaClinica(repoPotrerosMock.Object, repoHistoriaMock.Object);

            // Act
            ResultadoOperacion resultado = servicio.RegistrarEvento("Potrero1", "Bessie", DateTime.Now, "   ", "Fiebre alta");

            // Assert
            Assert.False(resultado.Exito);
            Assert.Contains("tipo de evento clínico no puede estar vacío", resultado.Mensaje);
            repoHistoriaMock.Verify(r => r.Agregar(It.IsAny<EventoClinico>()), Times.Never);
        }

        [Fact]
        public void RegistrarEvento_DescripcionVacia_DebeRetornarFalloYNoAgregar()
        {
            // Arrange
            var repoPotrerosMock = new Mock<IRepositorioPotreros>();
            var repoHistoriaMock = new Mock<IRepositorioHistoriaClinica>();
            var servicio = new ServicioHistoriaClinica(repoPotrerosMock.Object, repoHistoriaMock.Object);

            // Act
            ResultadoOperacion resultado = servicio.RegistrarEvento("Potrero1", "Bessie", DateTime.Now, "Revision", "");

            // Assert
            Assert.False(resultado.Exito);
            Assert.Contains("descripción del evento clínico no puede estar vacía", resultado.Mensaje);
            repoHistoriaMock.Verify(r => r.Agregar(It.IsAny<EventoClinico>()), Times.Never);
        }

        [Fact]
        public void RegistrarEvento_PotreroNoExiste_DebeRetornarFalloYNoAgregar()
        {
            // Arrange
            var repoPotrerosMock = new Mock<IRepositorioPotreros>();
            repoPotrerosMock.Setup(r => r.ObtenerPorId("PotreroX")).Returns((Potrero)null!);
            var repoHistoriaMock = new Mock<IRepositorioHistoriaClinica>();
            var servicio = new ServicioHistoriaClinica(repoPotrerosMock.Object, repoHistoriaMock.Object);

            // Act
            ResultadoOperacion resultado = servicio.RegistrarEvento("PotreroX", "Bessie", DateTime.Now, "Revision", "Chequeo general");

            // Assert
            Assert.False(resultado.Exito);
            Assert.Contains("No se encontró el potrero", resultado.Mensaje);
            repoHistoriaMock.Verify(r => r.Agregar(It.IsAny<EventoClinico>()), Times.Never);
        }

        [Fact]
        public void RegistrarEvento_ResNoExisteEnElPotrero_DebeRetornarFalloYNoAgregar()
        {
            // Arrange: Potrero.BuscarRes lanza si no encuentra coincidencias;
            // RegistrarEvento la atrapa específicamente y arma su propio mensaje.
            var potrero = NuevoPotrero("Potrero1");
            var repoPotrerosMock = new Mock<IRepositorioPotreros>();
            repoPotrerosMock.Setup(r => r.ObtenerPorId("Potrero1")).Returns(potrero);
            var repoHistoriaMock = new Mock<IRepositorioHistoriaClinica>();
            var servicio = new ServicioHistoriaClinica(repoPotrerosMock.Object, repoHistoriaMock.Object);

            // Act
            ResultadoOperacion resultado = servicio.RegistrarEvento("Potrero1", "Inexistente", DateTime.Now, "Revision", "Chequeo general");

            // Assert
            Assert.False(resultado.Exito);
            Assert.Contains("No se encontró la res", resultado.Mensaje);
            repoHistoriaMock.Verify(r => r.Agregar(It.IsAny<EventoClinico>()), Times.Never);
        }

        [Fact]
        public void RegistrarEvento_DatosValidos_DebeAgregarEventoConPotreroIdYNombreRes()
        {
            // Arrange
            var res = new Ternero("Bessie", 260, 5);
            var potrero = NuevoPotrero("Potrero1");
            potrero.L_reses.Add(res);
            var repoPotrerosMock = new Mock<IRepositorioPotreros>();
            repoPotrerosMock.Setup(r => r.ObtenerPorId("Potrero1")).Returns(potrero);
            var repoHistoriaMock = new Mock<IRepositorioHistoriaClinica>();
            var servicio = new ServicioHistoriaClinica(repoPotrerosMock.Object, repoHistoriaMock.Object);
            var fecha = new DateTime(2026, 3, 1);

            // Act
            ResultadoOperacion resultado = servicio.RegistrarEvento("Potrero1", "Bessie", fecha, "Revision", "Chequeo general");

            // Assert
            Assert.True(resultado.Exito);
            Assert.Contains("Bessie", resultado.Mensaje);
            repoHistoriaMock.Verify(r => r.Agregar(It.Is<EventoClinico>(
                e => e.PotreroId == "Potrero1"
                  && e.NombreRes == "Bessie"
                  && e.Fecha == fecha
                  && e.TipoEvento == "Revision"
                  && e.Descripcion == "Chequeo general")), Times.Once);
        }

        // ------------------------------------------------------------
        // ConsultarHistoria
        // ------------------------------------------------------------

        [Fact]
        public void ConsultarHistoria_ConEventos_DebeRetornarLosEventosDelRepositorio()
        {
            // Arrange
            var evento1 = new EventoClinico("Potrero1", "Bessie", new DateTime(2026, 1, 1), "Revision", "Chequeo");
            var evento2 = new EventoClinico("Potrero1", "Bessie", new DateTime(2026, 2, 1), "Tratamiento", "Antibiotico");
            var repoHistoriaMock = new Mock<IRepositorioHistoriaClinica>();
            repoHistoriaMock.Setup(r => r.ObtenerPorRes("Potrero1", "Bessie")).Returns(new List<EventoClinico> { evento1, evento2 });
            var servicio = new ServicioHistoriaClinica(new Mock<IRepositorioPotreros>().Object, repoHistoriaMock.Object);

            // Act
            List<EventoClinico> resultado = servicio.ConsultarHistoria("Potrero1", "Bessie");

            // Assert
            Assert.Equal(new List<EventoClinico> { evento1, evento2 }, resultado);
        }

        [Fact]
        public void ConsultarHistoria_SinEventos_DebeRetornarListaVacia()
        {
            // Arrange
            var repoHistoriaMock = new Mock<IRepositorioHistoriaClinica>();
            repoHistoriaMock.Setup(r => r.ObtenerPorRes("Potrero1", "Bessie")).Returns(new List<EventoClinico>());
            var servicio = new ServicioHistoriaClinica(new Mock<IRepositorioPotreros>().Object, repoHistoriaMock.Object);

            // Act
            List<EventoClinico> resultado = servicio.ConsultarHistoria("Potrero1", "Bessie");

            // Assert
            Assert.Empty(resultado);
        }

        [Fact]
        public void ConsultarHistoria_RepositorioLanzaExcepcion_DebeRetornarListaVacia()
        {
            // Arrange: ConsultarHistoria atrapa cualquier excepción del
            // repositorio y retorna una lista vacía en vez de propagarla.
            var repoHistoriaMock = new Mock<IRepositorioHistoriaClinica>();
            repoHistoriaMock.Setup(r => r.ObtenerPorRes("PotreroX", "Bessie")).Throws(new Exception("fallo de infraestructura"));
            var servicio = new ServicioHistoriaClinica(new Mock<IRepositorioPotreros>().Object, repoHistoriaMock.Object);

            // Act
            List<EventoClinico> resultado = servicio.ConsultarHistoria("PotreroX", "Bessie");

            // Assert
            Assert.Empty(resultado);
        }
    }
}
