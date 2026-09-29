using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;
using Bib_Hacienda.Dominio.Eventos;
using Moq;
using Xunit;

namespace Bib_Hacienda.Tests.Servicios
{
   
    
    public class ServicioPotrerosTests
    {
        private static Potrero NuevoPotrero(string identificacion, string tipo = "ternero")
        {
            return new Potrero(identificacion, tipo, new PublisherPotreroMitad(), new PublisherPotreroLleno());
        }


        [Fact]
        public void CrearPotrero_DatosValidos_DebeRetornarExitoYAgregarPotrero()
        {
            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.Existe("Potrero1")).Returns(false);
            var fabricasMock = new Mock<IRegistroFabricasRes>();
            var servicio = new ServicioPotreros(repoMock.Object, fabricasMock.Object);

            ResultadoOperacion resultado = servicio.CrearPotrero("Potrero1", "ternero");

            Assert.True(resultado.Exito);
            Assert.Contains("Potrero1", resultado.Mensaje);
            repoMock.Verify(r => r.Agregar(It.Is<Potrero>(p => p.Identificacion == "Potrero1" && p.Tipo_potrero == "ternero")), Times.Once);
        }

        [Fact]
        public void CrearPotrero_IdentificacionVacia_DebeRetornarFalloYNoConsultarRepositorio()
        {
            var repoMock = new Mock<IRepositorioPotreros>();
            var fabricasMock = new Mock<IRegistroFabricasRes>();
            var servicio = new ServicioPotreros(repoMock.Object, fabricasMock.Object);

            ResultadoOperacion resultado = servicio.CrearPotrero("   ", "ternero");

            Assert.False(resultado.Exito);
            Assert.Equal("El nombre de la res no puede estar vacío", resultado.Mensaje);
            repoMock.Verify(r => r.Existe(It.IsAny<string>()), Times.Never);
            repoMock.Verify(r => r.Agregar(It.IsAny<Potrero>()), Times.Never);
        }

        [Fact]
        public void CrearPotrero_YaExiste_DebeRetornarFalloYNoAgregar()
        {
            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.Existe("Potrero1")).Returns(true);
            var fabricasMock = new Mock<IRegistroFabricasRes>();
            var servicio = new ServicioPotreros(repoMock.Object, fabricasMock.Object);

            ResultadoOperacion resultado = servicio.CrearPotrero("Potrero1", "ternero");

            Assert.False(resultado.Exito);
            Assert.Contains("Ya existe un potrero", resultado.Mensaje);
            repoMock.Verify(r => r.Agregar(It.IsAny<Potrero>()), Times.Never);
        }


        [Fact]
        public void BuscarPotrero_CoincidenciaUnica_DebeRetornarPotrero()
        {
            var potrero = NuevoPotrero("PotreroNorte");
            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.ObtenerTodos()).Returns(new List<Potrero> { potrero });
            var fabricasMock = new Mock<IRegistroFabricasRes>();
            var servicio = new ServicioPotreros(repoMock.Object, fabricasMock.Object);

            Potrero encontrado = servicio.BuscarPotrero("Norte");

            Assert.Same(potrero, encontrado);
        }

        [Fact]
        public void BuscarPotrero_SinCoincidencias_DebeLanzarExcepcion()
        {
            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.ObtenerTodos()).Returns(new List<Potrero>());
            var fabricasMock = new Mock<IRegistroFabricasRes>();
            var servicio = new ServicioPotreros(repoMock.Object, fabricasMock.Object);

            Assert.Throws<Exception>(() => servicio.BuscarPotrero("Inexistente"));
        }

        [Fact]
        public void BuscarPotrero_MultiplesCoincidencias_DebeLanzarExcepcion()
        {
            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.ObtenerTodos()).Returns(new List<Potrero>
            {
                NuevoPotrero("PotreroNorte"),
                NuevoPotrero("PotreroNorte2")
            });
            var fabricasMock = new Mock<IRegistroFabricasRes>();
            var servicio = new ServicioPotreros(repoMock.Object, fabricasMock.Object);

            Assert.Throws<Exception>(() => servicio.BuscarPotrero("Norte"));
        }

        [Fact]
        public void BuscarPotrero_NombreVacio_DebeLanzarExcepcion()
        {
            var repoMock = new Mock<IRepositorioPotreros>();
            var fabricasMock = new Mock<IRegistroFabricasRes>();
            var servicio = new ServicioPotreros(repoMock.Object, fabricasMock.Object);

            var ex = Assert.Throws<Exception>(() => servicio.BuscarPotrero("   "));
            Assert.Contains("no puede estar vacío", ex.Message);
            repoMock.Verify(r => r.ObtenerTodos(), Times.Never);
        }


        [Fact]
        public void AnadirResAPotrero_PotreroNoExiste_DebeRetornarFalloYNoConsultarFabricas()
        {
            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.ObtenerTodos()).Returns(new List<Potrero>());
            var fabricasMock = new Mock<IRegistroFabricasRes>();
            var servicio = new ServicioPotreros(repoMock.Object, fabricasMock.Object);

            ResultadoOperacion resultado = servicio.AnadirResAPotrero("PotreroX", "Bessie", 5, 100);

            Assert.False(resultado.Exito);
            Assert.Contains("No se encontró el potrero", resultado.Mensaje);
            fabricasMock.Verify(f => f.ObtenerFabrica(It.IsAny<string>()), Times.Never);
            repoMock.Verify(r => r.Actualizar(It.IsAny<Potrero>()), Times.Never);
        }

        [Fact]
        public void AnadirResAPotrero_ResYaExiste_DebeRetornarFalloYNoConsultarFabricas()
        {
            var potrero = NuevoPotrero("Potrero1");
            potrero.L_reses.Add(new Ternero("Bessie", 200, 5));

            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.ObtenerTodos()).Returns(new List<Potrero> { potrero });
            var fabricasMock = new Mock<IRegistroFabricasRes>();
            var servicio = new ServicioPotreros(repoMock.Object, fabricasMock.Object);

            ResultadoOperacion resultado = servicio.AnadirResAPotrero("Potrero1", "Bessie", 5, 100);

            Assert.False(resultado.Exito);
            Assert.Contains("Ya existe una res", resultado.Mensaje);
            fabricasMock.Verify(f => f.ObtenerFabrica(It.IsAny<string>()), Times.Never);
            repoMock.Verify(r => r.Actualizar(It.IsAny<Potrero>()), Times.Never);
        }

        [Fact]
        public void AnadirResAPotrero_OperacionValida_DebeRetornarExitoYActualizarRepositorio()
        {
            var potrero = NuevoPotrero("Potrero1", "ternero");
            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.ObtenerTodos()).Returns(new List<Potrero> { potrero });

            var fabricaMock = new Mock<IFabricaRes>();
            fabricaMock.Setup(f => f.Crear("Bessie", 200u, (ushort)5)).Returns(new Ternero("Bessie", 200, 5));
            var fabricasMock = new Mock<IRegistroFabricasRes>();
            fabricasMock.Setup(f => f.ObtenerFabrica("ternero")).Returns(fabricaMock.Object);

            var servicio = new ServicioPotreros(repoMock.Object, fabricasMock.Object);

            ResultadoOperacion resultado = servicio.AnadirResAPotrero("Potrero1", "Bessie", 5, 200);

            Assert.True(resultado.Exito);
            Assert.Contains("Bessie", resultado.Mensaje);
            Assert.Contains(potrero.L_reses, r => r.Nombre == "Bessie");
            repoMock.Verify(r => r.Actualizar(potrero), Times.Once);
        }

        [Fact]
        public void AnadirResAPotrero_PotreroLleno_DebeRetornarFalloYNoActualizar()
        {
            var potrero = NuevoPotrero("Potrero1", "ternero");
            for (int i = 0; i < 150; i++)
            {
                potrero.L_reses.Add(new Ternero($"Res{i}", 200, 5));
            }

            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.ObtenerTodos()).Returns(new List<Potrero> { potrero });

            var fabricaMock = new Mock<IFabricaRes>();
            fabricaMock.Setup(f => f.Crear("Bessie", 200u, (ushort)5)).Returns(new Ternero("Bessie", 200, 5));
            var fabricasMock = new Mock<IRegistroFabricasRes>();
            fabricasMock.Setup(f => f.ObtenerFabrica("ternero")).Returns(fabricaMock.Object);

            var servicio = new ServicioPotreros(repoMock.Object, fabricasMock.Object);

            ResultadoOperacion resultado = servicio.AnadirResAPotrero("Potrero1", "Bessie", 5, 200);

            Assert.False(resultado.Exito);
            Assert.Contains("lleno", resultado.Mensaje);
            repoMock.Verify(r => r.Actualizar(It.IsAny<Potrero>()), Times.Never);
        }

        [Fact]
        public void AnadirResAPotrero_EdadInvalidaParaElTipo_DebeRetornarFalloYNoActualizar()
        {
            var potrero = NuevoPotrero("Potrero1", "ternero");
            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.ObtenerTodos()).Returns(new List<Potrero> { potrero });

            var fabricaMock = new Mock<IFabricaRes>();
            fabricaMock.Setup(f => f.Crear("Bessie", 200u, (ushort)13)).Returns(new Ternero("Bessie", 200, 13));
            var fabricasMock = new Mock<IRegistroFabricasRes>();
            fabricasMock.Setup(f => f.ObtenerFabrica("ternero")).Returns(fabricaMock.Object);

            var servicio = new ServicioPotreros(repoMock.Object, fabricasMock.Object);

            ResultadoOperacion resultado = servicio.AnadirResAPotrero("Potrero1", "Bessie", 13, 200);

            Assert.False(resultado.Exito);
            Assert.Contains("no es válida", resultado.Mensaje);
            repoMock.Verify(r => r.Actualizar(It.IsAny<Potrero>()), Times.Never);
        }


        [Fact]
        public void BuscarRes_Existente_DebeRetornarRes()
        {
            var potrero = NuevoPotrero("Potrero1");
            var res = new Ternero("Bessie", 200, 5);
            potrero.L_reses.Add(res);

            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.ObtenerPorId("Potrero1")).Returns(potrero);
            var fabricasMock = new Mock<IRegistroFabricasRes>();
            var servicio = new ServicioPotreros(repoMock.Object, fabricasMock.Object);

            Res? encontrada = servicio.BuscarRes("Potrero1", "Bessie");

            Assert.Same(res, encontrada);
        }

        [Fact]
        public void BuscarRes_ResInexistenteEnPotreroExistente_DebeRetornarNull()
        {
            var potrero = NuevoPotrero("Potrero1");
            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.ObtenerPorId("Potrero1")).Returns(potrero);
            var fabricasMock = new Mock<IRegistroFabricasRes>();
            var servicio = new ServicioPotreros(repoMock.Object, fabricasMock.Object);

            Res? encontrada = servicio.BuscarRes("Potrero1", "Inexistente");

            Assert.Null(encontrada);
        }

        [Fact]
        public void BuscarRes_PotreroInexistente_DebeRetornarNull()
        {
            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.ObtenerPorId("PotreroX")).Returns((Potrero)null!);
            var fabricasMock = new Mock<IRegistroFabricasRes>();
            var servicio = new ServicioPotreros(repoMock.Object, fabricasMock.Object);

            Res? encontrada = servicio.BuscarRes("PotreroX", "Bessie");

            Assert.Null(encontrada);
        }


        [Fact]
        public void ObtenerTodosLosPotreros_DebeRetornarOrdenadosPorIdentificacion()
        {
            var potreroZ = NuevoPotrero("PotreroZ");
            var potreroA = NuevoPotrero("PotreroA");
            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.ObtenerTodos()).Returns(new List<Potrero> { potreroZ, potreroA });
            var fabricasMock = new Mock<IRegistroFabricasRes>();
            var servicio = new ServicioPotreros(repoMock.Object, fabricasMock.Object);

            List<Potrero> resultado = servicio.ObtenerTodosLosPotreros();

            Assert.Equal(new List<Potrero> { potreroA, potreroZ }, resultado);
        }

        [Fact]
        public void ObtenerTodosLosPotreros_SinPotreros_DebeRetornarListaVacia()
        {
            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.ObtenerTodos()).Returns(new List<Potrero>());
            var fabricasMock = new Mock<IRegistroFabricasRes>();
            var servicio = new ServicioPotreros(repoMock.Object, fabricasMock.Object);

            List<Potrero> resultado = servicio.ObtenerTodosLosPotreros();

            Assert.Empty(resultado);
        }


        [Fact]
        public void ObtenerPotreroPorIdentificacion_Existente_DebeRetornarPotrero()
        {
            var potrero = NuevoPotrero("Potrero1");
            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.ObtenerPorId("Potrero1")).Returns(potrero);
            var fabricasMock = new Mock<IRegistroFabricasRes>();
            var servicio = new ServicioPotreros(repoMock.Object, fabricasMock.Object);

            Potrero? resultado = servicio.ObtenerPotreroPorIdentificacion("Potrero1");

            Assert.Same(potrero, resultado);
        }

        [Fact]
        public void ObtenerPotreroPorIdentificacion_RepositorioLanzaExcepcion_DebeRetornarNull()
        {
            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.ObtenerPorId("PotreroX")).Throws(new Exception("fallo de infraestructura"));
            var fabricasMock = new Mock<IRegistroFabricasRes>();
            var servicio = new ServicioPotreros(repoMock.Object, fabricasMock.Object);

            Potrero? resultado = servicio.ObtenerPotreroPorIdentificacion("PotreroX");

            Assert.Null(resultado);
        }


        [Fact]
        public void ObtenerTodasLasReses_ConVariosPotrerosYReses_DebeRetornarParesPotreroRes()
        {
            var potreroA = NuevoPotrero("PotreroA");
            var resA1 = new Ternero("A1", 200, 5);
            potreroA.L_reses.Add(resA1);
            var potreroB = NuevoPotrero("PotreroB");
            var resB1 = new Ternero("B1", 210, 6);
            var resB2 = new Ternero("B2", 220, 7);
            potreroB.L_reses.Add(resB1);
            potreroB.L_reses.Add(resB2);

            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.ObtenerTodos()).Returns(new List<Potrero> { potreroA, potreroB });
            var fabricasMock = new Mock<IRegistroFabricasRes>();
            var servicio = new ServicioPotreros(repoMock.Object, fabricasMock.Object);

            List<(Potrero Potrero, Res Res)> resultado = servicio.ObtenerTodasLasReses();

            Assert.Equal(3, resultado.Count);
            Assert.Contains(resultado, par => par.Potrero == potreroA && par.Res == resA1);
            Assert.Contains(resultado, par => par.Potrero == potreroB && par.Res == resB1);
            Assert.Contains(resultado, par => par.Potrero == potreroB && par.Res == resB2);
        }

        [Fact]
        public void ObtenerTodasLasReses_SinPotreros_DebeRetornarListaVacia()
        {
            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.ObtenerTodos()).Returns(new List<Potrero>());
            var fabricasMock = new Mock<IRegistroFabricasRes>();
            var servicio = new ServicioPotreros(repoMock.Object, fabricasMock.Object);

            List<(Potrero Potrero, Res Res)> resultado = servicio.ObtenerTodasLasReses();

            Assert.Empty(resultado);
        }


        [Fact]
        public void ObtenerEstadisticas_ConPotrerosYRazasVariadas_DebeCalcularTotalesYPromedio()
        {
            var potreroConReses = NuevoPotrero("Potrero1", "ternero");
            potreroConReses.L_reses.Add(new Ternero("T1", 200, 5));
            potreroConReses.L_reses.Add(new Cebon("C1", 300, 15));
            potreroConReses.L_reses.Add(new Novillo("N1", 400, 30));
            var potreroVacio = NuevoPotrero("Potrero2", "ternero");

            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.ObtenerTodos()).Returns(new List<Potrero> { potreroConReses, potreroVacio });
            var fabricasMock = new Mock<IRegistroFabricasRes>();
            var servicio = new ServicioPotreros(repoMock.Object, fabricasMock.Object);

            Dictionary<string, object> estadisticas = servicio.ObtenerEstadisticas();

            Assert.Equal(2, estadisticas["TotalPotreros"]);
            Assert.Equal(3, estadisticas["TotalReses"]);
            Assert.Equal(1, estadisticas["PotrerosVacios"]);
            Assert.Equal(1, estadisticas["PotrerosConReses"]);
            Assert.Equal(1, estadisticas["Terneros"]);
            Assert.Equal(1, estadisticas["Cebones"]);
            Assert.Equal(1, estadisticas["Novillos"]);
            Assert.Equal(300.0, estadisticas["PesoPromedio"]);
        }

        [Fact]
        public void ObtenerEstadisticas_SinPotreros_DebeRetornarPesoPromedioCero()
        {
            var repoMock = new Mock<IRepositorioPotreros>();
            repoMock.Setup(r => r.ObtenerTodos()).Returns(new List<Potrero>());
            var fabricasMock = new Mock<IRegistroFabricasRes>();
            var servicio = new ServicioPotreros(repoMock.Object, fabricasMock.Object);

            Dictionary<string, object> estadisticas = servicio.ObtenerEstadisticas();

            Assert.Equal(0, estadisticas["TotalPotreros"]);
            Assert.Equal(0, estadisticas["TotalReses"]);
            Assert.Equal(0.0, estadisticas["PesoPromedio"]);
        }
    }
}
