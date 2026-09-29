using System;
using Bib_Hacienda.Dominio;
using Bib_Hacienda.Dominio.Eventos;
using Xunit;

namespace Bib_Hacienda.Tests.Eventos
{

    public class PublisherPotreroLlenoTests
    {
        private static Potrero NuevoPotrero() =>
            new Potrero("P1", "ternero", new PublisherPotreroMitad(), new PublisherPotreroLleno());

        [Fact]
        public void InformarPotreroLleno_CantidadIgualAlMaximo_DebeDispararElEvento()
        {
            var publisher = new PublisherPotreroLleno();
            var potrero = NuevoPotrero();
            string? mensajeRecibido = null;
            publisher.evt_potrero_lleno += m => mensajeRecibido = m;

            publisher.Informar_Potrero_Lleno(Bib_Hacienda.Dominio.Reglas.ReglaPotrero.max_reses_potrero, potrero);

            Assert.NotNull(mensajeRecibido);
            Assert.Contains("capacidad máxima", mensajeRecibido);
        }

        [Fact]
        public void InformarPotreroLleno_CantidadMenorAlMaximo_NoDebeDispararElEvento()
        {
            var publisher = new PublisherPotreroLleno();
            var potrero = NuevoPotrero();
            bool disparado = false;
            publisher.evt_potrero_lleno += _ => disparado = true;

            publisher.Informar_Potrero_Lleno(1, potrero);

            Assert.False(disparado);
        }

        [Fact]
        public void InformarPotreroLleno_SinSuscriptores_NoDebeLanzarExcepcion()
        {
            var publisher = new PublisherPotreroLleno();
            var potrero = NuevoPotrero();

            var ex = Record.Exception(() =>
                publisher.Informar_Potrero_Lleno(Bib_Hacienda.Dominio.Reglas.ReglaPotrero.max_reses_potrero, potrero));

            Assert.Null(ex);
        }
    }

    public class PublisherPotreroMitadTests
    {
        private static Potrero NuevoPotrero() =>
            new Potrero("P1", "ternero", new PublisherPotreroMitad(), new PublisherPotreroLleno());

        [Fact]
        public void InformarPotreroMitad_CantidadIgualALaMitad_DebeDispararElEvento()
        {
            var publisher = new PublisherPotreroMitad();
            var potrero = NuevoPotrero();
            string? mensajeRecibido = null;
            publisher.evt_potrero_mitad += m => mensajeRecibido = m;

            ushort mitad = (ushort)(Bib_Hacienda.Dominio.Reglas.ReglaPotrero.max_reses_potrero / 2);
            publisher.Informar_Potrero_Mitad(mitad, potrero);

            Assert.NotNull(mensajeRecibido);
            Assert.Contains("mitad de su capacidad", mensajeRecibido);
        }

        [Fact]
        public void InformarPotreroMitad_CantidadDistintaDeLaMitad_NoDebeDispararElEvento()
        {
            var publisher = new PublisherPotreroMitad();
            var potrero = NuevoPotrero();
            bool disparado = false;
            publisher.evt_potrero_mitad += _ => disparado = true;

            publisher.Informar_Potrero_Mitad(1, potrero);

            Assert.False(disparado);
        }

        [Fact]
        public void InformarPotreroMitad_SinSuscriptores_NoDebeLanzarExcepcion()
        {
            var publisher = new PublisherPotreroMitad();
            var potrero = NuevoPotrero();
            ushort mitad = (ushort)(Bib_Hacienda.Dominio.Reglas.ReglaPotrero.max_reses_potrero / 2);

            var ex = Record.Exception(() => publisher.Informar_Potrero_Mitad(mitad, potrero));

            Assert.Null(ex);
        }
    }

    public class PublisherPesoMinTests
    {
        [Fact]
        public void InformarPesoMin_PesoDebajoDelMinimo_DebeDispararElEvento()
        {
            var publisher = new PublisherPesoMin();
            var res = new Ternero("Vaca1", 50, 5);
            string? mensajeRecibido = null;
            publisher.evt_peso_min += m => mensajeRecibido = m;

            publisher.Informar_Peso_Min(res);

            Assert.NotNull(mensajeRecibido);
            Assert.Contains("desnutrición", mensajeRecibido);
        }

        [Fact]
        public void InformarPesoMin_PesoIgualOMayorAlMinimo_NoDebeDispararElEvento()
        {
            var publisher = new PublisherPesoMin();
            var res = new Ternero("Vaca1", 200, 5);
            bool disparado = false;
            publisher.evt_peso_min += _ => disparado = true;

            publisher.Informar_Peso_Min(res);

            Assert.False(disparado);
        }

        [Fact]
        public void InformarPesoMin_SinSuscriptores_NoDebeLanzarExcepcion()
        {
            var publisher = new PublisherPesoMin();
            var res = new Ternero("Vaca1", 50, 5);

            var ex = Record.Exception(() => publisher.Informar_Peso_Min(res));

            Assert.Null(ex);
        }
    }

    public class PublisherPesoVentaTests
    {
        [Fact]
        public void InformarPesoVenta_PesoAptoParaVenta_DebeDispararElEvento()
        {
            var publisher = new PublisherPesoVenta();
            var res = new Ternero("Vaca1", 300, 5);
            string? mensajeRecibido = null;
            publisher.evt_peso_venta += m => mensajeRecibido = m;

            publisher.Informar_Peso_Venta(res);

            Assert.NotNull(mensajeRecibido);
            Assert.Contains("apta para venta", mensajeRecibido);
        }

        [Fact]
        public void InformarPesoVenta_PesoInsuficiente_NoDebeDispararElEvento()
        {
            var publisher = new PublisherPesoVenta();
            var res = new Ternero("Vaca1", 100, 5);
            bool disparado = false;
            publisher.evt_peso_venta += _ => disparado = true;

            publisher.Informar_Peso_Venta(res);

            Assert.False(disparado);
        }
    }

    public class PublisherVacunaVencidaTests
    {
        private static Bacteriana NuevaVacuna(DateTime vencimiento) =>
            new Bacteriana("Aftosa", "L1", vencimiento, DateTime.Now, 3);

        [Fact]
        public void InformarVacunaVencida_VacunaNula_DebeLanzarExceptionQueEnvuelveArgumentNullException()
        {
            var publisher = new PublisherVacunaVencida();

            var ex = Assert.Throws<Exception>(() => publisher.Informar_Vacuna_Vencida(null!));

            Assert.Contains("Informar_Vacuna_Vencida", ex.Message);
            Assert.Contains("La vacuna no puede ser null", ex.Message);
        }

        [Fact]
        public void InformarVacunaVencida_FechaYaPasada_DebeRetornarTrueYMensajeDeVencida()
        {
            var publisher = new PublisherVacunaVencida();
            var vacuna = NuevaVacuna(DateTime.Now.AddDays(-1));
            string? mensajeRecibido = null;
            publisher.evt_vacuna_vencida += m => mensajeRecibido = m;

            bool resultado = publisher.Informar_Vacuna_Vencida(vacuna);

            Assert.True(resultado);
            Assert.Contains("está vencida", mensajeRecibido);
        }

        [Fact]
        public void InformarVacunaVencida_VenceEnMenosDeUnMes_DebeRetornarFalseYMensajeDeAlerta()
        {
            var publisher = new PublisherVacunaVencida();
            var vacuna = NuevaVacuna(DateTime.Now.AddDays(10));
            string? mensajeRecibido = null;
            publisher.evt_vacuna_vencida += m => mensajeRecibido = m;

            bool resultado = publisher.Informar_Vacuna_Vencida(vacuna);

            Assert.False(resultado);
            Assert.Contains("ALERTA", mensajeRecibido);
        }

        [Fact]
        public void InformarVacunaVencida_VenceEnMasDeUnMes_DebeRetornarFalseYMensajeDeValida()
        {
            var publisher = new PublisherVacunaVencida();
            var vacuna = NuevaVacuna(DateTime.Now.AddMonths(3));
            string? mensajeRecibido = null;
            publisher.evt_vacuna_vencida += m => mensajeRecibido = m;

            bool resultado = publisher.Informar_Vacuna_Vencida(vacuna);

            Assert.False(resultado);
            Assert.Contains("es válida", mensajeRecibido);
        }

        [Fact]
        public void InformarVacunaVencida_SinSuscriptores_DebeRetornarResultadoSinLanzarExcepcion()
        {
            var publisher = new PublisherVacunaVencida();
            var vacuna = NuevaVacuna(DateTime.Now.AddDays(-1));

            bool resultado = publisher.Informar_Vacuna_Vencida(vacuna);

            Assert.True(resultado);
        }
    }

    public class PublisherVacunacionCompletadaTests
    {
        [Fact]
        public void InformarVacunacionCompletada_ResNula_DebeLanzarExceptionQueEnvuelveArgumentNullException()
        {
            var publisher = new PublisherVacunacionCompletada();

            var ex = Assert.Throws<Exception>(() => publisher.Informar_Vacunacion_Completada(null!, 0, 0));

            Assert.Contains("Informar_Vacunacion_Completada", ex.Message);
            Assert.Contains("La res no puede ser null", ex.Message);
        }

        [Fact]
        public void InformarVacunacionCompletada_EsquemaCompleto_DebeRetornarTrueYMensajeCorrespondiente()
        {
            var publisher = new PublisherVacunacionCompletada();
            var res = new Ternero("Vaca1", 200, 5);
            string? mensajeRecibido = null;
            publisher.evt_vacunacion_completada += m => mensajeRecibido = m;

            bool resultado = publisher.Informar_Vacunacion_Completada(res, 3, 1);

            Assert.True(resultado);
            Assert.Contains("ha completado su esquema", mensajeRecibido);
        }

        [Fact]
        public void InformarVacunacionCompletada_EsquemaIncompleto_DebeRetornarFalseYMensajeCorrespondiente()
        {
            var publisher = new PublisherVacunacionCompletada();
            var res = new Ternero("Vaca1", 200, 5);
            string? mensajeRecibido = null;
            publisher.evt_vacunacion_completada += m => mensajeRecibido = m;

            bool resultado = publisher.Informar_Vacunacion_Completada(res, 1, 0);

            Assert.False(resultado);
            Assert.Contains("aún no ha completado", mensajeRecibido);
        }

        [Fact]
        public void InformarVacunacionCompletada_SinSuscriptores_DebeRetornarResultadoSinLanzarExcepcion()
        {
            var publisher = new PublisherVacunacionCompletada();
            var res = new Ternero("Vaca1", 200, 5);

            bool resultado = publisher.Informar_Vacunacion_Completada(res, 3, 1);

            Assert.True(resultado);
        }
    }
}
