using Godot;
using System.Collections.Generic;

public partial class GameManager {
	
	public static GameManager Instance { get; } = new GameManager();

	public List<AbejaReina> JugadoresEnPartida = new List<AbejaReina>();

	public AbejaReina jugadorEnTurno ;

	private GameManager(){}

	public int cantidadJugadores {get; set;}
	
	public int TirarDado(){
		if(jugadorEnTurno.EsSuTurno && !jugadorEnTurno.TiroLosDados){
			int numeroAleatorio = GD.RandRange(1, 6);

			return numeroAleatorio;
		}
		else{
			return 1;
		}
	}

	public void TransformarAbejaObrera(Abeja unaAbeja, Abeja otraAbeja, Colmena unaColmena){
		if(unaAbeja.AptaParaTransformar(otraAbeja, unaColmena)){
			// Deberiamos eliminar la instancia de abeja obrera y crear una nueva instancia con el tipo de abeja seleccionada
		}
	}

	public void CargarJugadores(int cantidadDeJugadores){
		for (int i = 1; i <= cantidadDeJugadores; i++){
			var NuevoJugador = new AbejaReina(i);
			
			JugadoresEnPartida.Add(NuevoJugador);
		}	
	}

	public void EstablecerPrimerTurno(){
		jugadorEnTurno = JugadoresEnPartida[0];
		jugadorEnTurno.EsSuTurno = true;
	}
	/*
	public void CambiarTurnoASiguienteJugador(){

	}

	public void RecolectarRecurso(string unRecurso, Colmena unaColmena){

	}

	public void Atacar(unaAbeja, otraAbeja){

	}

	public bool PuedeAtacar(unaAbeja, otraAbeja){
		
	}
	*/
}
