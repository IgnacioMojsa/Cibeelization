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
	public void EstablecerPrimerTurno_AsignaJugadorCorrecto()
	{
		var turnManager = new TurnManager(jugadores);

		turnManager.EstablecerPrimerTurno();

		AssertThat(turnManager.indiceTurno).IsEqual(0);
		AssertThat(turnManager.jugadorEnTurno).IsEqual(jugadores[0]);
		AssertThat(turnManager.jugadorEnTurno.EsSuTurno).IsTrue();
		AssertThat(turnManager.jugadorEnTurno.Estado).IsEqual(AbejaReina.EstadoTurno.EsperandoDado);
	}
}
}