
using Godot;
using System;

public partial class GameManager {
	
	public static GameManager Instance { get; private set; }
	
	
	public int TirarDado(){
		int numeroAleatorio = GD.RandRange(1, 6);
		
		return numeroAleatorio;
	}

	/*public bool PuedeCanjearRecurso(string unRecurso, Colmena unaColmena){

	}

	public void RecolectarRecurso(unRecurso){

	}

	public void TransformarAbeja(unaAbeja, otraAbeja){

	}

	public void MoverAbeja(unaAbeja, cantidad){

	}

	public void Atacar(unaAbeja, otraAbeja){

	}

	public bool PuedeAtacar(unaAbeja, otraAbeja){
		
	}
	*/
}
