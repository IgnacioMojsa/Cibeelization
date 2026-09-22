using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
public class GameManagerTests
{
    private GameManager gameManager;

    [BeforeTest]
    public void Setup()
    {
        gameManager = GameManager.Instance;
        gameManager.ResetearEstadoPartida();
    }

    [AfterTest]
    public void TearDown()
    {
        gameManager.ResetearEstadoPartida();
    }

    [TestCase]
    //carga dos jugadores y le asigna 3 movimientos restantes al jugador 1, el turno termina y pasa al jugador 2.
    public void ConsumirMovimientosEnUnTurnoYPasarAlSiguienteJugador()
    {
        gameManager.CargarJugadores(2);
        gameManager.jugadorEnTurno.MovimientosDisponibles = 3;
        var jugadorInicial = gameManager.jugadorEnTurno;

        gameManager.ConsumirMovimiento();
        gameManager.ConsumirMovimiento();
        gameManager.ConsumirMovimiento();

        AssertThat(gameManager.jugadorEnTurno).IsNotEqual(jugadorInicial);
        AssertThat(gameManager.jugadorEnTurno.Id).IsEqual(2);
    }

    [TestCase]
    //En una partida con 4 jugadores, 3 quedan eliminados, y se cumple la condicion de victoria con el jugador 1 (indice = 0).
    public void QuedaUnJugadorEnPieYSeCumpleLaCondicionDeVictoria()
    {
        gameManager.CargarJugadores(4);
        gameManager.cantidadJugadores = 4;

        gameManager.EliminarJugador(1);
        gameManager.EliminarJugador(2);
        gameManager.EliminarJugador(3);

        AssertThat(gameManager.CondicionVictoria()).IsTrue();

        gameManager.EstablecerJugadorGanador();
        AssertThat(gameManager.JugadorGanador).IsNotNull();
        AssertThat(gameManager.JugadorGanador.Id).IsEqual(1);
    }
}