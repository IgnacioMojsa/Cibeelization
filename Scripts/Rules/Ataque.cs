public class AttackLogic
{

	private AudioSetting punchSound => AudioManager.Instance?.GameAudio?.Sound3;
	public bool PuedeAtacar(AbejaReina jugador)
	{
		return jugador != null && jugador.MovimientosDisponibles >= 0 && jugador.ModoAtaque;
	}

	public bool JugadorEstaEliminado(AbejaReina jugador)
	{
		return jugador != null && jugador.HP < 5;
	}

	public void DaniarJugador(Abeja atacante,AbejaReina jugadorObjetivo)
	{
		if (jugadorObjetivo == null) return;
		
		jugadorObjetivo.RestarVida(atacante);

		AudioManager.Instance.PlaySound(punchSound);
	}
	
	public void DaniarAbeja(Abeja unaAbeja)
	{
		if(unaAbeja == null) return;

		AudioManager.Instance.PlaySound(punchSound);

		// Aca hay que usar unaAbeja.RestarVida(), el actual es solamente provsiorio;
		
		unaAbeja.MatarAbeja();
	}
}
