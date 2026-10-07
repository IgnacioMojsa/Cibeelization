using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class TroopsManager : Node3D
{
    [Export] private Tablero tableroActual;
	[Export] private PlayerManager playerManager;
	[Export] public CeldasManager celdasManager;
	private readonly List<PackedScene> Assets = new();
	public Dictionary<Abeja, Node3D> VisualAbejas = new();
	public Dictionary<AbejaReina, Node3D> AlmacenJugadores = new();
	public List<Abeja> AbejasCercanas = new();
	public Node3D VisualSubditoActual;
	public Abeja SubditoActual;
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

		celdasManager.PintarCeldaDeAbeja(celdaCliqueada, abejaNueva);

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

	public void AbsorberSubdito(Celda unaCelda)
	{
		var abejaSacrificio = GameManager.Instance.jugadorEnTurno.ColmenaDeReina.AbejasDeColmena.Find(a => a.CeldaActual == unaCelda);

		celdasManager.DespintarCeldaDeAbeja(unaCelda);
		GameManager.Instance.ConsumirAbsorcion(abejaSacrificio);
		AbejasCercanas.Remove(abejaSacrificio);
		VisualAbejas[abejaSacrificio].QueueFree();

		GD.Print("El jugador " + GameManager.Instance.jugadorEnTurno.Id + " tiene " + GameManager.Instance.jugadorEnTurno.HP + " puntos de vida");
	}

	public void MostrarAbejasCercanas()
	{
		var materialAtaque = GD.Load<StandardMaterial3D>("res://outlineAttack.tres");
		
		var reinaActual = GameManager.Instance.jugadorEnTurno;
		var celdasAdyacentes = tableroActual.ObtenerVecinos(GameManager.Instance.jugadorEnTurno.UbicacionActual);

		AbejasCercanas = reinaActual.ColmenaDeReina.AbejasDeColmena.Where(a => celdasAdyacentes.Contains(a.CeldaActual)).ToList();

		foreach (var abeja in AbejasCercanas)
		{
			playerManager.vfxManager.EstablecerNextPass(VisualAbejas[abeja], materialAtaque);

			//GD.Print(VisualAbejas[abeja]);
		} 
	}

	public void OcultarAbejasCercanas()
	{
		var materialOriginal = GD.Load<StandardMaterial3D>("res://outlineBase.tres");

		foreach (var abeja in AbejasCercanas)
		{
			playerManager.vfxManager.EstablecerNextPass(VisualAbejas[abeja], materialOriginal);

			//GD.Print(VisualAbejas[abeja]);
		} 
	}

	public bool PuedeAbsorberSubdito()
	{
		if (tableroActual == null) return false;
		
		var reinaActual = GameManager.Instance.jugadorEnTurno;
		var celdasAdyacentes = tableroActual.ObtenerVecinos(GameManager.Instance.jugadorEnTurno.UbicacionActual);

		if (celdasAdyacentes == null || celdasAdyacentes.Any(c => c == null)) return false;

		return celdasAdyacentes.Any(c => reinaActual.ColmenaDeReina.AbejasDeColmena.Any(abeja => abeja.CeldaActual == c)) && reinaActual.HP < 15;
	}

	public void IntentarMoverSubdito(Celda unaCelda)
	{
		var jugador = GameManager.Instance.jugadorEnTurno;

		if (jugador.MovimientosDisponibles <= 0) return;
		
		if (!playerManager.movimientoManager.CeldasSonAdyacentes(SubditoActual.CeldaActual, unaCelda)) return;
    	if (playerManager.movimientoManager.CeldaTieneOtraAbeja(unaCelda)) return;
    	if (GameManager.Instance.JugadoresEnPartida.Any(j => !j.FueraDeJuego && j.UbicacionActual == unaCelda)) return;

		MoverSubdito(unaCelda);
		GameManager.Instance.CamaraActual.EnfocarNodo(VisualSubditoActual, 2, 1);
	}

	private void MoverSubdito(Celda unaCelda)
	{
    	celdasManager.DespintarCeldaDeAbeja(SubditoActual.CeldaActual);

    	Vector3 pos = unaCelda.Tile.GlobalPosition;
    	pos.Y = VisualSubditoActual.GlobalPosition.Y;
    	VisualSubditoActual.GlobalPosition = pos;

    	SubditoActual.CeldaActual = unaCelda;
    	celdasManager.PintarCeldaDeAbeja(unaCelda, SubditoActual);

    	GameManager.Instance.ConsumirMovimiento();
    	GameManager.Instance.NotificarCambioDeEstado();
    	AudioManager.Instance.PlaySound(AudioManager.Instance.GameAudio.Sound2);
	}

	public void ControlarAbeja(Celda unaCelda)
	{	
		Abeja subditoAControlar = AbejasCercanas.Find(a => a.CeldaActual == unaCelda && !a.FueraDeJuego);
		
		if (!VisualAbejas.TryGetValue(subditoAControlar, out var visual) || !IsInstanceValid(visual))
        	return;

		if (subditoAControlar == null)
    	{
    	    EventosUI.MostrarMensaje("Elegí un subdito cercano para controlar.");
    	    return;
    	}

		GameManager.Instance.CamaraActual.EnfocarNodo(visual, 2, 1);
		GameManager.Instance.jugadorEnTurno.MoviendoSubdito = true;
		VisualSubditoActual = visual;
		SubditoActual = subditoAControlar;
	}
}
