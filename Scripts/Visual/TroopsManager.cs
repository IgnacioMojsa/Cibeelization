using Godot;
using System.Collections.Generic;

public partial class TroopsManager : Node3D
{
    private readonly List<PackedScene> Assets = new();

    public override void _Ready()
	{
		CargarAssets();
	}

    public void CargarAssets()
	{
		List<PackedScene> meshAbejas = new List<PackedScene>(){
			GD.Load<PackedScene>("res://Scenes/Zangano.tscn")
		};

		for (int i = 0; i < GameManager.Instance.TiposDeAbejas.Count; i++)
		{
			if (i < meshAbejas.Count)
			{
				Assets.Add(meshAbejas[i]);
			}
		}
	}

    public void InstanciarAbeja(){
        GameManager.Instance.GenerarAbejaNueva(new Abeja());

        var InstanciaNueva = Assets[0].Instantiate<Node3D>();

		AddChild(InstanciaNueva);

        EstablecerPosicionDeAbeja();
    }

    public void EstablecerPosicionDeAbeja(){

    }
}