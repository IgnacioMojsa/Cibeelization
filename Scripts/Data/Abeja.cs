using Godot;
using System;

public partial class Abeja{
	public int HP { get; private set; } = 15;
	public string RecursoDeTransformacion { get ; private set; } 
	public bool FueraDeJuego {get; set;} = false;
	public Colmena ColmenaHogar {get; set;}
	public Celda CeldaActual { get; set; }
	public Node3D InstanciaVisual {get; set;}

	public bool AptaParaTransformar(Abeja otraAbeja, Colmena unaColmena){
		return unaColmena.TieneRecurso(otraAbeja.RecursoDeTransformacion);
	}

	public void RestarVida(){
		HP -= 5;
	}

	public void MatarAbeja(){
		HP -= HP;
		FueraDeJuego = true;
	}

	/*public override void _Input(InputEvent @event){
		if (@event is not InputEventMouseButton mouse ||
			mouse.ButtonIndex != MouseButton.Left ||
			!mouse.Pressed)
			return; 
			GD.Print("Hiciste click");
	}*/

}
