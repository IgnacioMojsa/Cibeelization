using Godot;
using System;

public partial class Abeja{
	public int HP { get; private set; } = 5;
	public string RecursoDeTransformacion { get ; private set; } 
	public Vector3I Posicion { get; private set; }

	public bool AptaParaTransformar(Abeja otraAbeja, Colmena unaColmena){
        return unaColmena.TieneRecurso(otraAbeja.RecursoDeTransformacion);
    }

	public void CambiarPosicion(Vector3I nuevaPosicion){
		Posicion = nuevaPosicion;
	}

	/*public override void _Input(InputEvent @event){
		if (@event is not InputEventMouseButton mouse ||
            mouse.ButtonIndex != MouseButton.Left ||
            !mouse.Pressed)
            return; 
			GD.Print("Hiciste click");
	}*/

}
