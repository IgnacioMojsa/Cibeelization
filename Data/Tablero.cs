using Godot;
using System.Collections.Generic;

public partial class Tablero : Node
{
    private List<Celda> celdas = new List<Celda>();

    public override void _Ready()
    {
        celdas.Add(new CeldaSinReforzar(
            0,
            new Vector3I(-7, 0, 11),
            new Vector3I(-7, 0, 10)
        ));

        celdas.Add(new CeldaSinReforzar(
            1,
            new Vector3I(-7, 0, 7),
            new Vector3I(-7, 0, 6)
        ));

        celdas.Add(new CeldaSinReforzar(
            2,
            new Vector3I(-7, 0, 5),
            new Vector3I(-7, 0, 4)
        ));
    }
}