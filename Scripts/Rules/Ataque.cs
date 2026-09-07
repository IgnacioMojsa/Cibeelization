using Godot;
using System.Collections.Generic;

public class AtaqueManager
{
    public bool PuedeAtacar(AbejaReina jugador)
    {
		return jugador != null && jugador.MovimientosDisponibles >= 0;
	}

    public bool JugadorEstaEliminado(AbejaReina jugador)
    {
        return jugador != null && jugador.HP <= 5;
    }

    public void DaniarJugador(AbejaReina jugador)
    {
        if(jugador == null) return;
        jugador.RestarVida();
    }

    public bool JugadorEnTurnoAdyacenteAOtro(
		Node3D jugadorActual,
		Node3D otroJugador,
		Dictionary<Node3D, Celda> celdaActualPorJugador,
		Tablero tablero)
	{
		if (!celdaActualPorJugador.ContainsKey(jugadorActual))
			return false;

		if (!celdaActualPorJugador.ContainsKey(otroJugador))
			return false;

		List<Celda> vecinos = tablero.ObtenerVecinos(celdaActualPorJugador[jugadorActual]);
		Celda celdaOtroJugador = celdaActualPorJugador[otroJugador];

		return vecinos.Contains(celdaOtroJugador);
	}

	public void EfectuarAtaque(AbejaReina jugador)
	{
		DaniarJugador(jugador);
		GD.Print("Jugador " + jugador.Id + " ahora tiene " + jugador.HP + " puntos de vida");
	}


}