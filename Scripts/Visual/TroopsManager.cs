using Godot;
using System.Collections.Generic;

public partial class TroopsManager : Node3D
{
    private readonly List<PackedScene> Assets = new();
	public Dictionary<Abeja, Node3D> VisualAbejas = new();
	public Dictionary<AbejaReina, Node3D> AlmacenJugadores = new();
    public override void _Ready()
	{
	    AlmacenJugadores.Clear();
	    CargarAssets();
	    if (GameManager.Instance.JugadoresEnPartida.Count > 0)
	        CargarAlmacenDeTropasPara(GameManager.Instance.cantidadJugadores);
	}

	public void CargarAssets()
	{
	    // Limpiamos la lista por si venimos de un reinicio
	    Assets.Clear();
	
	    // Cargamos los tipos de abejas disponibles como catálogo
	    Assets.Add(GD.Load<PackedScene>("res://Scenes/Zangano.tscn"));
	
	    // Si más adelante agregás otros tipos, simplemente los sumás acá:
	    // Assets.Add(GD.Load<PackedScene>("res://Scenes/AbejaGuerrera.tscn"));
	    // Assets.Add(GD.Load<PackedScene>("res://Scenes/AbejaSanadora.tscn"));
	}

    public void InstanciarAbeja(Celda celdaCliqueada)
	{
	    if (Assets.Count == 0)
	    {
	        GD.PrintErr("No hay assets cargados para invocar abejas");
	        return;
	    }

	    if (!AlmacenJugadores.ContainsKey(GameManager.Instance.jugadorEnTurno))
	    {
	        GD.PrintErr("No existe almacén de tropas para este jugador");
	        return;
	    }

	    var abejaNueva = new Abeja();
	    var InstanciaNueva = Assets[0].Instantiate<Node3D>();

	    abejaNueva.ColmenaHogar = GameManager.Instance.jugadorEnTurno.ColmenaDeReina;
	    abejaNueva.CeldaActual = celdaCliqueada;
	    GameManager.Instance.GenerarAbejaNueva(abejaNueva);

	    VisualAbejas.Add(abejaNueva, InstanciaNueva);
		AlmacenJugadores[GameManager.Instance.jugadorEnTurno].AddChild(InstanciaNueva);
	    EstablecerPosicionDeAbeja(InstanciaNueva, celdaCliqueada.Tile.GlobalPosition);

		PintarCeldaDeAbeja(celdaCliqueada, abejaNueva);

	    GD.Print("Abeja instanciada en " + celdaCliqueada);
	}

    public void EstablecerPosicionDeAbeja(Node3D instanciaNueva, Vector3 posicion){
		instanciaNueva.GlobalPosition = posicion;
    }

	public void CargarAlmacenDeTropasPara(int cantidadJugadores){
		for (int j = 0; j < cantidadJugadores; j++)
		{
			var almacenJugador = new Node3D();
			almacenJugador.Name = "AlmacenJugador" + GameManager.Instance.JugadoresEnPartida[j].Id;

			AlmacenJugadores.Add(GameManager.Instance.JugadoresEnPartida[j], almacenJugador);

			AddChild(almacenJugador);
		}
	}

	public void PintarCeldaDeAbeja(Celda unaCelda, Abeja unaAbeja)
	{
		var jugadorActual = unaAbeja.ColmenaHogar.ReinaDeColmena;
		int indiceJugador = GameManager.Instance.JugadoresEnPartida.IndexOf(jugadorActual);
		
		var colorJ1 = Color.Color8(106, 38, 143, 255);
		var colorJ2 = Color.Color8(76, 29, 174, 255);
		var colorJ3 = Color.Color8(179, 54, 113, 255);
		var colorJ4 = Color.Color8(130, 96, 229, 255); 

		List<Color> coloresDeJugadores = new List<Color>{colorJ1, colorJ2, colorJ3, colorJ4};
		var hexagono = unaCelda.Tile.GetNode<Node3D>("hexagon_tile");
		var materialDeMesh = hexagono.GetChild<MeshInstance3D>(0).GetActiveMaterial(0); 

		StandardMaterial3D nuevoMaterial = (StandardMaterial3D)materialDeMesh.Duplicate();
		nuevoMaterial.AlbedoColor = coloresDeJugadores[indiceJugador];

		hexagono.GetChild<MeshInstance3D>(0).SetSurfaceOverrideMaterial(0, nuevoMaterial);
	}

	public void DespintarCeldaDeAbeja(Celda unaCelda, Abeja unaAbeja)
	{
		var jugadorActual = unaAbeja.ColmenaHogar.ReinaDeColmena;
		int indiceJugador = GameManager.Instance.JugadoresEnPartida.IndexOf(jugadorActual);

		var hexagono = unaCelda.Tile.GetNode<Node3D>("hexagon_tile");
		var materialDeMesh = hexagono.GetChild<MeshInstance3D>(0).GetActiveMaterial(0); 

		StandardMaterial3D nuevoMaterial = (StandardMaterial3D)materialDeMesh.Duplicate();
		nuevoMaterial.AlbedoColor = Color.Color8(144, 101, 27, 255);

		hexagono.GetChild<MeshInstance3D>(0).SetSurfaceOverrideMaterial(0, nuevoMaterial);
	}
}
