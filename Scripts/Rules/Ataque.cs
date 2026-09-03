using Godot;

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
}