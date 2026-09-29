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


        [Fact]
        public void RegistrarEvento_TipoEventoVacio_DebeRetornarFalloYNoAgregar()
        {
            var repoPotrerosMock = new Mock<IRepositorioPotreros>();
            var repoHistoriaMock = new Mock<IRepositorioHistoriaClinica>();
            var servicio = new ServicioHistoriaClinica(repoPotrerosMock.Object, repoHistoriaMock.Object);

            ResultadoOperacion resultado = servicio.RegistrarEvento("Potrero1", "Bessie", DateTime.Now, "   ", "Fiebre alta");

            Assert.False(resultado.Exito);
            Assert.Contains("tipo de evento clínico no puede estar vacío", resultado.Mensaje);
            repoHistoriaMock.Verify(r => r.Agregar(It.IsAny<EventoClinico>()), Times.Never);
        }

        [Fact]
        public void RegistrarEvento_DescripcionVacia_DebeRetornarFalloYNoAgregar()
        {
            var repoPotrerosMock = new Mock<IRepositorioPotreros>();
            var repoHistoriaMock = new Mock<IRepositorioHistoriaClinica>();
            var servicio = new ServicioHistoriaClinica(repoPotrerosMock.Object, repoHistoriaMock.Object);

            ResultadoOperacion resultado = servicio.RegistrarEvento("Potrero1", "Bessie", DateTime.Now, "Revision", "");

            Assert.False(resultado.Exito);
            Assert.Contains("descripción del evento clínico no puede estar vacía", resultado.Mensaje);
            repoHistoriaMock.Verify(r => r.Agregar(It.IsAny<EventoClinico>()), Times.Never);
        }

        [Fact]
        public void RegistrarEvento_PotreroNoExiste_DebeRetornarFalloYNoAgregar()
        {
            var repoPotrerosMock = new Mock<IRepositorioPotreros>();
            repoPotrerosMock.Setup(r => r.ObtenerPorId("PotreroX")).Returns((Potrero)null!);
            var repoHistoriaMock = new Mock<IRepositorioHistoriaClinica>();
            var servicio = new ServicioHistoriaClinica(repoPotrerosMock.Object, repoHistoriaMock.Object);

            ResultadoOperacion resultado = servicio.RegistrarEvento("PotreroX", "Bessie", DateTime.Now, "Revision", "Chequeo general");

            Assert.False(resultado.Exito);
            Assert.Contains("No se encontró el potrero", resultado.Mensaje);
            repoHistoriaMock.Verify(r => r.Agregar(It.IsAny<EventoClinico>()), Times.Never);
        }

        [Fact]
        public void RegistrarEvento_ResNoExisteEnElPotrero_DebeRetornarFalloYNoAgregar()
        {
            var potrero = NuevoPotrero("Potrero1");
            var repoPotrerosMock = new Mock<IRepositorioPotreros>();
            repoPotrerosMock.Setup(r => r.ObtenerPorId("Potrero1")).Returns(potrero);
            var repoHistoriaMock = new Mock<IRepositorioHistoriaClinica>();
            var servicio = new ServicioHistoriaClinica(repoPotrerosMock.Object, repoHistoriaMock.Object);

            ResultadoOperacion resultado = servicio.RegistrarEvento("Potrero1", "Inexistente", DateTime.Now, "Revision", "Chequeo general");

            Assert.False(resultado.Exito);
            Assert.Contains("No se encontró la res", resultado.Mensaje);
            repoHistoriaMock.Verify(r => r.Agregar(It.IsAny<EventoClinico>()), Times.Never);
        }

        [Fact]
        public void RegistrarEvento_DatosValidos_DebeAgregarEventoConPotreroIdYNombreRes()
        {
            var res = new Ternero("Bessie", 260, 5);
            var potrero = NuevoPotrero("Potrero1");
            potrero.L_reses.Add(res);
            var repoPotrerosMock = new Mock<IRepositorioPotreros>();
            repoPotrerosMock.Setup(r => r.ObtenerPorId("Potrero1")).Returns(potrero);
            var repoHistoriaMock = new Mock<IRepositorioHistoriaClinica>();
            var servicio = new ServicioHistoriaClinica(repoPotrerosMock.Object, repoHistoriaMock.Object);
            var fecha = new DateTime(2026, 3, 1);

            ResultadoOperacion resultado = servicio.RegistrarEvento("Potrero1", "Bessie", fecha, "Revision", "Chequeo general");

            Assert.True(resultado.Exito);
            Assert.Contains("Bessie", resultado.Mensaje);
            repoHistoriaMock.Verify(r => r.Agregar(It.Is<EventoClinico>(
                e => e.PotreroId == "Potrero1"
                  && e.NombreRes == "Bessie"
                  && e.Fecha == fecha
                  && e.TipoEvento == "Revision"
                  && e.Descripcion == "Chequeo general")), Times.Once);
        }


        [Fact]
        public void ConsultarHistoria_ConEventos_DebeRetornarLosEventosDelRepositorio()
        {
            var evento1 = new EventoClinico("Potrero1", "Bessie", new DateTime(2026, 1, 1), "Revision", "Chequeo");
            var evento2 = new EventoClinico("Potrero1", "Bessie", new DateTime(2026, 2, 1), "Tratamiento", "Antibiotico");
            var repoHistoriaMock = new Mock<IRepositorioHistoriaClinica>();
            repoHistoriaMock.Setup(r => r.ObtenerPorRes("Potrero1", "Bessie")).Returns(new List<EventoClinico> { evento1, evento2 });
            var servicio = new ServicioHistoriaClinica(new Mock<IRepositorioPotreros>().Object, repoHistoriaMock.Object);

            List<EventoClinico> resultado = servicio.ConsultarHistoria("Potrero1", "Bessie");

            Assert.Equal(new List<EventoClinico> { evento1, evento2 }, resultado);
        }

        [Fact]
        public void ConsultarHistoria_SinEventos_DebeRetornarListaVacia()
        {
            var repoHistoriaMock = new Mock<IRepositorioHistoriaClinica>();
            repoHistoriaMock.Setup(r => r.ObtenerPorRes("Potrero1", "Bessie")).Returns(new List<EventoClinico>());
            var servicio = new ServicioHistoriaClinica(new Mock<IRepositorioPotreros>().Object, repoHistoriaMock.Object);

            List<EventoClinico> resultado = servicio.ConsultarHistoria("Potrero1", "Bessie");

            Assert.Empty(resultado);
        }

        [Fact]
        public void ConsultarHistoria_RepositorioLanzaExcepcion_DebeRetornarListaVacia()
        {
            var repoHistoriaMock = new Mock<IRepositorioHistoriaClinica>();
            repoHistoriaMock.Setup(r => r.ObtenerPorRes("PotreroX", "Bessie")).Throws(new Exception("fallo de infraestructura"));
            var servicio = new ServicioHistoriaClinica(new Mock<IRepositorioPotreros>().Object, repoHistoriaMock.Object);

            List<EventoClinico> resultado = servicio.ConsultarHistoria("PotreroX", "Bessie");

            Assert.Empty(resultado);
        }
    }
}
