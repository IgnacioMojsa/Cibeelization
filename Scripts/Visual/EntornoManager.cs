using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class EntornoManager : Node3D
{
    [Export] private Tablero tablero;
    private List<MeshInstance3D> FloresDelEntorno = new List<MeshInstance3D>();
    public override void _Ready()
    {
        ObtenerTodasLasFlores();
        ReasignarPosicionesSegunTablero();
    }

    public void ObtenerTodasLasFlores()
    {
        for (int c = 0; c < GetChildCount(); c++)
        {
            var unaFlor = GetChild(c);
            
            if(unaFlor is MeshInstance3D flor){
                FloresDelEntorno.Add(flor);
            }
        }

        FloresDelEntorno = FloresDelEntorno.Where(f => f.Name.ToString().Contains("Flor")).ToList();
    }

    public void ReasignarPosicionesSegunTablero()
    {
        var floresAReasignar = new List<MeshInstance3D>();
        var superficie = GetNode<MeshInstance3D>("Superficie");

        for (int i = 5; i < 10; i++)
        {
            var florAReasignar = FloresDelEntorno.Find(f => f.Name.ToString() == ("Flor" + i));

            floresAReasignar.Add(florAReasignar);
        }

        foreach (var flor in floresAReasignar)
        {
            flor.Position = new Vector3(flor.Position.X * (tablero.WidthRows / 10), flor.Position.Y, flor.Position.Z * (tablero.HeightRows / 10));
        }

        superficie.Scale = new Vector3(superficie.Scale.X * (tablero.WidthRows / 10), superficie.Scale.Y, superficie.Scale.Z * (tablero.HeightRows / 10));
    }
}
