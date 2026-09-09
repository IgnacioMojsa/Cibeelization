using Godot;
using System.Collections.Generic;

public partial class TroopsManager : Node3D
{
    private readonly List<PackedScene> Assets = new();
	public Dictionary<AbejaReina, Node3D> TropasJugadores = new();

    public override void _Ready()
	{
		CargarAssets();
		CargarAlmacenDeTropasPara(GameManager.Instance.cantidadJugadores);
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

    public void InstanciarAbeja(Vector3 posicion, Celda celdaCliqueada){
		var abejaNueva = new Abeja();
		var InstanciaNueva = Assets[0].Instantiate<Node3D>();

		abejaNueva.CeldaActual = celdaCliqueada;
		GameManager.Instance.GenerarAbejaNueva(abejaNueva);

		TropasJugadores[GameManager.Instance.jugadorEnTurno].AddChild(InstanciaNueva);
        EstablecerPosicionDeAbeja(InstanciaNueva, posicion);

		GD.Print("Abeja instanciada en " + posicion);
    }

    public void EstablecerPosicionDeAbeja(Node3D instanciaNueva, Vector3 posicion){
		instanciaNueva.GlobalPosition = posicion;
    }

	public void CargarAlmacenDeTropasPara(int cantidadJugadores){
		for (int j = 0; j < cantidadJugadores; j++)
		{
			var almacenJugador = new Node3D();
			almacenJugador.Name = "AlmacenJugador" + GameManager.Instance.JugadoresEnPartida[j].Id;

			TropasJugadores.Add(GameManager.Instance.JugadoresEnPartida[j], almacenJugador);

			AddChild(almacenJugador);
		}
	}
}