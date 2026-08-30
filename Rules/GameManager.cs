using Godot;
using System.Collections.Generic;

public partial class GameManager {
	
	public static GameManager Instance { get; private set; }

	List<AbejaReina> JugadoresEnPartida = new List<AbejaReina>();

	public AbejaReina turnoActual ;
	
	public int TirarDado(){
		int numeroAleatorio = GD.RandRange(1, 6);
		
		return numeroAleatorio;
	}

	public void TransformarAbejaObrera(Abeja unaAbeja, Abeja otraAbeja, Colmena unaColmena){
		if(unaAbeja.AptaParaTransformar(otraAbeja, unaColmena)){
			// Deberiamos eliminar la instancia de abeja obrera y crear una nueva instancia con el tipo de abeja seleccionada
		}
	}

	public void CargarJugadores(int cantidadDeJugadores){
		while(cantidadDeJugadores <= JugadoresEnPartida.Count){
			var NuevoJugador = new AbejaReina(JugadoresEnPartida.Count + 1);
			
			JugadoresEnPartida.Add(NuevoJugador);
		}
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
