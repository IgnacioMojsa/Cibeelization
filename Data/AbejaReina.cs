using Godot;
using System;

public partial class AbejaReina : Abeja
{
    public bool EsSuTurno {get; set;} = false;
    public int Id {get; set;}

    public AbejaReina(int id){
        Id = id;
    }
    /*
    public void Mover(Vector3 nuevaPosicion)
    {
        GlobalPosition = nuevaPosicion;
    }
    */
}
