using Godot;
using System.Collections.Generic;

public class TurnManager
{
	public List<AbejaReina> JugadoresEnPartida {get; set;}
	public int indiceTurno = 0;
	public AbejaReina jugadorEnTurno {get; set;}

	public TurnManager(List<AbejaReina> jugadores)
	{
		JugadoresEnPartida = jugadores;
	}

	public void EstablecerPrimerTurno(){
		if(JugadoresEnPartida == null || JugadoresEnPartida.Count == 0) return;

		indiceTurno = 0;
		jugadorEnTurno = JugadoresEnPartida[indiceTurno];
		jugadorEnTurno.EsSuTurno = true;
		jugadorEnTurno.Estado = AbejaReina.EstadoTurno.EsperandoDado;
		jugadorEnTurno.MovimientosDisponibles = 0;

		GD.Print("Es turno del jugador " + jugadorEnTurno.Id);
	}

	public void TerminarTurno(){
		if (jugadorEnTurno == null) return;

		jugadorEnTurno.EsSuTurno = false;
		jugadorEnTurno.TiroLosDados = false;
		jugadorEnTurno.SeMovio = false;
		jugadorEnTurno.AtacoRecien = false;
		jugadorEnTurno.Estado = AbejaReina.EstadoTurno.TurnoTerminado;

		GD.Print("Terminó su turno");

		CambiarTurnoASiguienteJugador();
	}

	public void CambiarTurnoASiguienteJugador(){

		if(JugadoresEnPartida == null || JugadoresEnPartida.Count == 0) return;
		
		indiceTurno = (indiceTurno + 1) % JugadoresEnPartida.Count;
		jugadorEnTurno = JugadoresEnPartida[indiceTurno];
		jugadorEnTurno.EsSuTurno = true;
		jugadorEnTurno.Estado = AbejaReina.EstadoTurno.EsperandoDado;
		jugadorEnTurno.MovimientosDisponibles = 0;

		GameManager.Instance.jugadorEnTurno = jugadorEnTurno;

		GD.Print("Turno del jugador " + jugadorEnTurno.Id);
	}
}
