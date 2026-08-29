
using Godot;
using System;

public partial class GameManager {
	
	public static GameManager Instance { get; private set; }
	
	public int TirarDado(){
		int numeroAleatorio = GD.RandRange(1, 6);
		
		return numeroAleatorio;
	}

	public void TransformarAbejaObrera(Abeja unaAbeja, Abeja otraAbeja, Colmena unaColmena){
		if(unaAbeja.AptaParaTransformar(otraAbeja, unaColmena)){
			// Deberiamos eliminar la instancia de abeja obrera y crear una nueva instancia con el tipo de abeja seleccionada
		}
	}

	public void MoverAbeja(Abeja unaAbeja, Celda unaCelda){
		unaAbeja.CambiarPosicion(unaCelda.PosicionA);
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
