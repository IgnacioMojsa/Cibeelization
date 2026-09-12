using Godot;
using System;
using System.Collections.Generic;

public class TurnManager
{
	public List<AbejaReina> JugadoresEnPartida {get; set;}
	public int indiceTurno = 0;
	public AbejaReina jugadorEnTurno {get; set;}

	public event Action<AbejaReina> OnCambioDeTurnoJugador;
	public event Action<string> OnTextoInstrucciones;
	public event Action OnTurnoCambiado;

	public TurnManager(List<AbejaReina> jugadores)
	{
		JugadoresEnPartida = jugadores;
	}

	public void EstablecerPrimerTurno(){
		if(JugadoresEnPartida == null || JugadoresEnPartida.Count == 0) return;

		indiceTurno = 0;
		jugadorEnTurno = JugadoresEnPartida[indiceTurno];
		PrepararJugadorParaTurno();
		GD.Print("Es turno del jugador " + jugadorEnTurno.Id);

		OnCambioDeTurnoJugador?.Invoke(jugadorEnTurno);

		// jugadorEnTurno.EsSuTurno = true;
		// jugadorEnTurno.Estado = AbejaReina.EstadoTurno.EsperandoDado;
		// jugadorEnTurno.MovimientosDisponibles = 0;
		
	}

	public void TerminarTurno(){
		if (jugadorEnTurno == null) return;

		jugadorEnTurno.EsSuTurno = false;
		jugadorEnTurno.TiroLosDados = false;
		jugadorEnTurno.SeMovio = false;
		jugadorEnTurno.AtacoRecien = false;
		jugadorEnTurno.ModoAtaque = false;
		jugadorEnTurno.ModoInvocacion = false;
		jugadorEnTurno.InvocoRecien = false; 
		jugadorEnTurno.Estado = AbejaReina.EstadoTurno.TurnoTerminado;

		GD.Print("Terminó su turno");

		CambiarTurnoASiguienteJugador();
	}

	private void BuscarSiguienteJugadorActivo()
	{
		int intentos = 0;
		int totalJugadores = JugadoresEnPartida.Count;

		do
		{
			indiceTurno = (indiceTurno + 1) % totalJugadores;
			intentos++;
		} 
		while (JugadoresEnPartida[indiceTurno].FueraDeJuego && intentos < totalJugadores);
	}

	private void PrepararJugadorParaTurno()
	{
		jugadorEnTurno.EsSuTurno = true;
		jugadorEnTurno.Estado = AbejaReina.EstadoTurno.EsperandoDado;
		jugadorEnTurno.MovimientosDisponibles = 0;

		GameManager.Instance.jugadorEnTurno = jugadorEnTurno;
	}

	private void IniciarTurnoJugadorActual()
	{
		jugadorEnTurno = JugadoresEnPartida[indiceTurno];
		PrepararJugadorParaTurno();

		GD.Print("Turno del jugador " + jugadorEnTurno.Id);

		OnCambioDeTurnoJugador?.Invoke(jugadorEnTurno);
		OnTextoInstrucciones?.Invoke("Tirá el dado para continuar.");
		OnTurnoCambiado?.Invoke();

		// jugadorEnTurno.EsSuTurno = true;
		// jugadorEnTurno.Estado = AbejaReina.EstadoTurno.EsperandoDado;
		// jugadorEnTurno.MovimientosDisponibles = 0;
		// GameManager.Instance.jugadorEnTurno = jugadorEnTurno;

	}

	public void CambiarTurnoASiguienteJugador(){

		if(JugadoresEnPartida == null || JugadoresEnPartida.Count == 0) return;
		
		BuscarSiguienteJugadorActivo();
		IniciarTurnoJugadorActual();
	}
}
