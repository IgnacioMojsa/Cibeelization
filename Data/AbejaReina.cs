using Godot;
using System;

public partial class AbejaReina : Abeja
{
	public void Mover(Vector3 nuevaPosicion)
    {
        GlobalPosition = nuevaPosicion;
    }
}
