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

    public void InstanciarAbeja(Vector3 posicion, Celda celdaCliqueada)
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

			AlmacenJugadores.Add(GameManager.Instance.JugadoresEnPartida[j], almacenJugador);

			AddChild(almacenJugador);
		}
	}
}