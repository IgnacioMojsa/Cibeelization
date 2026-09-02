using Godot;
using System.Collections.Generic;

public partial class GameManager {
	
	public static GameManager Instance { get; } = new GameManager();

	public List<AbejaReina> JugadoresEnPartida = new List<AbejaReina>();

	public AbejaReina jugadorEnTurno ;

	private int indiceTurno = 0;

	private GameManager(){}

	//[Export] private PlayerManager playerManager;

	public int cantidadJugadores {get; set;}
	
	/* public int TirarDado(){
		if(jugadorEnTurno == null) return 1;
		if(!jugadorEnTurno.EsSuTurno) return 1;
		if(jugadorEnTurno.Estado != AbejaReina.EstadoTurno.EsperandoDado) return 1;
		if(jugadorEnTurno.TiroLosDados) return 1;
		
		int numeroAleatorio = GD.RandRange(1, 6);
		jugadorEnTurno.TiroLosDados = true;
		//Acá abajo, si sale un 6, te deja mover la abeja 6 veces pq no está definido todavía. Si quieren permitir 1 solo movimiento hay que cambiar numeroAleatorio por 1 ahí abajo.
		jugadorEnTurno.MovimientosDisponibles = numeroAleatorio;
		//jugadorEnTurno.MovimientosDisponibles = 1;
		jugadorEnTurno.Estado = AbejaReina.EstadoTurno.EsperandoAccion;

		return numeroAleatorio;
	} */
	
	public int TirarDado()
	{
		if(jugadorEnTurno == null) return 1;
		if(!jugadorEnTurno.EsSuTurno) return 1;
		if(jugadorEnTurno.Estado != AbejaReina.EstadoTurno.EsperandoDado) return 1;
		if(jugadorEnTurno.TiroLosDados) return 1;

		int numeroAleatorio = GD.RandRange(1, 6);
		jugadorEnTurno.TiroLosDados = true;

		// Guardamos la cantidad de pasos que el dado otorgó
		jugadorEnTurno.MovimientosDisponibles = numeroAleatorio;
		jugadorEnTurno.Estado = AbejaReina.EstadoTurno.EsperandoAccion;

		return numeroAleatorio;
	}

	public void ConsumirMovimiento()
	{
		if (jugadorEnTurno == null) return;

		// Descontamos 1 movimiento por cada paso realizado
		jugadorEnTurno.MovimientosDisponibles--;

		GD.Print("Movimientos restantes: " + jugadorEnTurno.MovimientosDisponibles);

		if (jugadorEnTurno.MovimientosDisponibles <= 0)
		{
			TerminarTurno();
		}
	}

	public bool PuedeMover(){
		return jugadorEnTurno != null &&
			   jugadorEnTurno.EsSuTurno &&
			   jugadorEnTurno.Estado == AbejaReina.EstadoTurno.EsperandoAccion;
			   //&&
			   //Esta otra linea limita los movimientos de la misma abeja
			   //jugadorEnTurno.MovimientosDisponibles > 0;
	}

	 public bool PuedeAtacar(){
		return jugadorEnTurno != null &&
			   jugadorEnTurno.EsSuTurno &&
			   jugadorEnTurno.Estado == AbejaReina.EstadoTurno.EsperandoAccion;
	}

	/* public void ConsumirMovimiento(){
		if (jugadorEnTurno == null) return;

		jugadorEnTurno.MovimientosDisponibles--;
		if (jugadorEnTurno.MovimientosDisponibles <= 0)
			TerminarTurno();
	} */

	public void ConsumirAtaque(){
		if (jugadorEnTurno == null) return;

		jugadorEnTurno.AtacoRecien = true;
		TerminarTurno();
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
		if(jugadorEnTurno == JugadoresEnPartida[JugadoresEnPartida.Count - 1]){
			jugadorEnTurno = JugadoresEnPartida[0];
		}
		
		indiceTurno = (indiceTurno + 1) % JugadoresEnPartida.Count;
		jugadorEnTurno = JugadoresEnPartida[indiceTurno];
		jugadorEnTurno.EsSuTurno = true;
		jugadorEnTurno.Estado = AbejaReina.EstadoTurno.EsperandoDado;
		jugadorEnTurno.MovimientosDisponibles = 0;

		GD.Print("Turno del jugador " + jugadorEnTurno.Id);
	}

	public void TransformarAbejaObrera(Abeja unaAbeja, Abeja otraAbeja, Colmena unaColmena){
		if(unaAbeja.AptaParaTransformar(otraAbeja, unaColmena)){
			// Deberiamos eliminar la instancia de abeja obrera y crear una nueva instancia con el tipo de abeja seleccionada
		}
	}

	public void CargarJugadores(int cantidadDeJugadores)
	{
		JugadoresEnPartida.Clear();
		for (int i = 1; i <= cantidadDeJugadores; i++){
			var NuevoJugador = new AbejaReina(i);
			JugadoresEnPartida.Add(NuevoJugador);
		}	
	}

	public void EstablecerPrimerTurno(){
		indiceTurno = 0;
		jugadorEnTurno = JugadoresEnPartida[indiceTurno];
		jugadorEnTurno.EsSuTurno = true;
		jugadorEnTurno.Estado = AbejaReina.EstadoTurno.EsperandoDado;
	}

	/*

	public void RecolectarRecurso(string unRecurso, Colmena unaColmena){

	}

	public void Atacar(unaAbeja, otraAbeja){

	}

	public bool PuedeAtacar(unaAbeja, otraAbeja){
		
	}
	*/
}
