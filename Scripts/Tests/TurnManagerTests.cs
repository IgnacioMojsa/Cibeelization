using Godot;
using GdUnit4;
using System.Collections.Generic;
using static GdUnit4.Assertions;

/*NOTA MUY IMPORTANTE para hacer tests

[TestSuite] 
public class TestCSharp {}

el nombre de la clase PUBLICA debe ser el mismo que el del archivo, en este caso TestCSharp.cs*/

namespace JuegoAbeja.Tests{

[TestSuite]
public class TurnManagerTests
{
	private List<AbejaReina> jugadores;

	[BeforeTest]
	public void Setup()
	{
		jugadores = new List<AbejaReina>
		{
			new AbejaReina(1) {FueraDeJuego = false},
			new AbejaReina(2) {FueraDeJuego = false},
			new AbejaReina(3) {FueraDeJuego = true}
		};
	}

	[TestCase]

	//Al comenzar la partida, se establece el primer turno al jugador 1 (indice=0)
	public void AsignarJugadorCorrectoEnElPrimerTurno()
	{
		var turnManager = new TurnManager(jugadores);

		turnManager.EstablecerPrimerTurno();

		AssertThat(turnManager.indiceTurno).IsEqual(0);
		AssertThat(turnManager.jugadorEnTurno).IsEqual(jugadores[0]);
		AssertThat(turnManager.jugadorEnTurno.EsSuTurno).IsTrue();
		AssertThat(turnManager.jugadorEnTurno.Estado).IsEqual(AbejaReina.EstadoTurno.EsperandoDado);
	}



	[TestCase]
	//En el setup, el jugador 3 (indice=2) está fuera de juego, por lo que una ves termine el turno del jugador 1 y el 2, regresará al 1.
    public void OmitirJugadorFueraDeJuegoAlCambiarDeTurno()
    {
        var turnManager = new TurnManager(jugadores);
        turnManager.EstablecerPrimerTurno();
        turnManager.TerminarTurno();

        AssertThat(turnManager.indiceTurno).IsEqual(1);
        AssertThat(turnManager.jugadorEnTurno.Id).IsEqual(2);
	
        turnManager.TerminarTurno(); 

        AssertThat(turnManager.indiceTurno).IsEqual(0);
        AssertThat(turnManager.jugadorEnTurno.Id).IsEqual(1);
    }
}
}