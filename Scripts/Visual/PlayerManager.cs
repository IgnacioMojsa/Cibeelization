using Godot;
using System.Collections.Generic;

public partial class PlayerManager : Node3D
{
	[Export] private Camera3D camera;
	[Export] private Tablero tablero; 

	public readonly List<Node3D> VisualesJugadores = new();
	public readonly List<Node3D> OutlinesJugadores = new();
	private readonly List<PackedScene> Assets = new();
	private readonly Dictionary<Node3D, Celda> CeldaActualPorJugador = new();

	private MovimientoManager movimientoManager;
	private AtaqueManager ataqueManager;

	public Node3D VisualJugadorActual;
	public Vector3 PosicionEnMundo3D;
	public Celda CeldaCliqueada;
	public Celda CeldaOrigen;

	public override void _Ready()
	{
		movimientoManager = new MovimientoManager(tablero);
		ataqueManager = new AtaqueManager();

		InstanciarJugadores();
		GuardarOutlines();
		CallDeferred(nameof(EstablecerSpawnsEnCeldas));
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!movimientoManager.PuedeMover(GameManager.Instance.jugadorEnTurno))
			return;

		if (!@event.IsActionPressed("move"))
		return; 

		if (GetViewport().GuiGetHoveredControl() != null)
				return;

		camera ??= GetViewport().GetCamera3D();
		IntentarMoverJugador();
		
	}

	private void IntentarMoverJugador()
	{		
		if (GameManager.Instance.jugadorEnTurno.MovimientosDisponibles <= 0)
			return;

		VisualJugadorActual = VisualesJugadores[GameManager.Instance.jugadorEnTurno.Id - 1];

		Vector2 mousePosition = GetViewport().GetMousePosition();
		Vector3 rayOrigin = camera.ProjectRayOrigin(mousePosition);
		Vector3 rayEnd = rayOrigin + camera.ProjectRayNormal(mousePosition) * 1000.0f;
	
		var spaceState = GetWorld3D().DirectSpaceState;
		var query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
		var result = spaceState.IntersectRay(query);

		if(result.Count == 0)
		return;

		PosicionEnMundo3D = result["position"].AsVector3();
		CeldaCliqueada = movimientoManager.ObtenerCeldaDesdePosicion(tablero.Celdas, PosicionEnMundo3D);
	
		if(CeldaCliqueada == null)
		return;

		EstablecerCeldaParaJugadorEnTurno();

		if(movimientoManager.PuedeMoverseEntre(CeldaOrigen, CeldaCliqueada))
		{
			MoverAbejaACelda(VisualJugadorActual, CeldaCliqueada);
		}
		else
		{
			GD.Print("Solo puedes moverte a una celda contigua o vecina.");
		}

	}

	private void EstablecerCeldaParaJugadorEnTurno(){
		//if (CeldaCliqueada == null) return;
	
		if (!CeldaActualPorJugador.ContainsKey(VisualJugadorActual))
		{
			CeldaActualPorJugador[VisualJugadorActual] = movimientoManager.ObtenerCeldaDesdePosicion(tablero.Celdas, VisualJugadorActual.GlobalPosition);
		}
	
		CeldaOrigen = CeldaActualPorJugador[VisualJugadorActual];
	
		if (CeldaCliqueada == CeldaOrigen) return;
	}

	/* private void MoverJugadorACeldasAdyacentes(Node3D jugador,Celda unaCelda)
	{
		List<Celda> VecinosAdyacentes = tablero.ObtenerVecinos(unaCelda);
	
		if (VecinosAdyacentes.Contains(CeldaCliqueada))
		{
			MoverAbejaACelda(jugador, CeldaCliqueada);
		}
		else
		{
			GD.Print("Solo puedes moverte a una celda contigua/vecina.");
		}
	} */

	private bool JugadorEnTurnoAdyacenteAOtro(Node3D otroJugador){
		List<Celda> VecinosAdyacentes = tablero.ObtenerVecinos(CeldaActualPorJugador[VisualJugadorActual]);

		Celda CeldaOtroJugador = CeldaActualPorJugador[otroJugador];

		return VecinosAdyacentes.Contains(CeldaOtroJugador);
	}

	private void MoverAbejaACelda(Node3D jugador, Celda celdaDestino)
	{
		Vector3 targetPos = celdaDestino.Tile.GlobalPosition;
		targetPos.Y = jugador.GlobalPosition.Y; 

		jugador.GlobalPosition = targetPos;
		CeldaActualPorJugador[jugador] = celdaDestino;

		GameManager.Instance.ConsumirMovimiento();
	}

	public void Atacar()
	{
		if (!GameManager.Instance.PuedeAtacar())
			return;

		for (int j = 0; j < VisualesJugadores.Count; j++) 
		{
			Node3D JugadorAEvaluar = VisualesJugadores[j];

			if(JugadorEnTurnoAdyacenteAOtro(JugadorAEvaluar)){
				GD.Print("Atacaste al jugador " + GameManager.Instance.JugadoresEnPartida[j].Id);
				
				EfectuarAtaque(JugadorAEvaluar, j);
			}
			else{
				GD.Print("No atacaste a nadie");
			}
		}

		GameManager.Instance.ConsumirAtaque();
	}

	private void EfectuarAtaque(Node3D unJugador, int Id){

		AbejaReina jugador = GameManager.Instance.JugadoresEnPartida[Id];

		if(ataqueManager.JugadorEstaEliminado(jugador)){
			EliminarInstanciaDeJugador(unJugador, Id);
		}
		else{
			ataqueManager.DaniarJugador(jugador);
			GD.Print("Jugador " + jugador.Id + " ahora tiene " + jugador.HP + " puntos de vida");
		}
	}

	private void EliminarInstanciaDeJugador(Node3D unJugador, int Id){
		GameManager.Instance.EliminarJugador(Id);
		unJugador.QueueFree();
	}

	public void CargarAssets()
	{
		List<PackedScene> meshPlayers = new List<PackedScene>(){
			GD.Load<PackedScene>("res://Scenes/AbejaReina.tscn"),
			GD.Load<PackedScene>("res://Scenes/AbejaReina2.tscn"),
			GD.Load<PackedScene>("res://Scenes/AbejaReina.tscn"),
			GD.Load<PackedScene>("res://Scenes/Zangano.tscn")
		};

		for (int i = 0; i < GameManager.Instance.JugadoresEnPartida.Count; i++)
		{
			if (i < meshPlayers.Count)
			{
				Assets.Add(meshPlayers[i]);
			}
		}
	}

	public void InstanciarJugadores()
	{
		CargarAssets(); 

		for (int j = 0; j < GameManager.Instance.cantidadJugadores; j++)
		{
			var InstanciaNueva = Assets[j].Instantiate<Node3D>();
			AddChild(InstanciaNueva);
			VisualesJugadores.Add(InstanciaNueva);
		}
	}

	public void GuardarOutlines(){
		for (int j = 0; j < GameManager.Instance.cantidadJugadores; j++)
		{
			var contornoJugador = VisualesJugadores[j].GetNode<Node3D>("Outline");
			OutlinesJugadores.Add(contornoJugador);
		}
	}

	public void EstablecerSpawnsEnCeldas()
	{
		if (tablero == null || tablero.Celdas == null || tablero.Celdas.Count == 0)
			return;

		int totalCeldas = tablero.Celdas.Count;

		for (int i = 0; i < VisualesJugadores.Count; i++)
		{
			int indiceCelda = (i * (totalCeldas / VisualesJugadores.Count)) % totalCeldas;
			
			Celda celdaInicio = tablero.Celdas[indiceCelda]; 
			Node3D reina = VisualesJugadores[i];

			Vector3 targetPos = celdaInicio.Tile.GlobalPosition;
			targetPos.Y = reina.GlobalPosition.Y;

			reina.GlobalPosition = targetPos;
			CeldaActualPorJugador[reina] = celdaInicio;
			GameManager.Instance.JugadoresEnPartida[i].UbicacionActual = celdaInicio;

			GD.Print(
				"Jugador " + i + " ubicado en " 
				+ GameManager.Instance.JugadoresEnPartida[i].UbicacionActual 
				+ (GameManager.Instance.JugadoresEnPartida[i].UbicacionActual.Q,
				GameManager.Instance.JugadoresEnPartida[i].UbicacionActual.R)
			);
		}
	}
}

/* public partial class PlayerManager : Node3D
{
	[Export] private Camera3D camera;

	private List<Node3D> VisualesJugadores = new List<Node3D>();
	private List<PackedScene> Assets = new List<PackedScene>();
	private Vector3 posicionNueva; 
	private Node3D ReinaDelJugador; 

	public override void _Ready()
	{
		InstanciarJugadores();
		EstablecerSpawns();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if(!GameManager.Instance.PuedeMover())
			return;

		if (@event.IsActionPressed("move"))
		{
			if (GetViewport().GuiGetHoveredControl() != null)
				return;

			if (camera == null)
				camera = GetViewport().GetCamera3D();

			// Solo movemos si el raycast encontró una posición válida
			if (ObtenerPosicionNueva())
			{
				MoverAbeja();
			}
		}
	}

	public bool ObtenerPosicionNueva()
	{
		Vector2 mousePosition = GetViewport().GetMousePosition();
		Vector3 rayOrigin = camera.ProjectRayOrigin(mousePosition);
		Vector3 rayEnd = rayOrigin + camera.ProjectRayNormal(mousePosition) * 1000.0f;

		var spaceState = GetWorld3D().DirectSpaceState;
		var query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);

		var result = spaceState.IntersectRay(query);

		if (result.Count > 0)
		{
			// Casteo directo desde Godot.Variant a Vector3
			posicionNueva = result["position"].AsVector3();
			GD.Print("Nueva posición guardada: " + posicionNueva);
			return true;
		}

		GD.Print("El raycast no chocó con ningún objeto 3D con colisión.");
		return false;
	}

	public void MoverAbeja()
	{
		if(!GameManager.Instance.PuedeMover())
		return;

		if (VisualesJugadores.Count > 0)
		{
			ReinaDelJugador = VisualesJugadores[GameManager.Instance.jugadorEnTurno.Id - 1]; 

			if (ReinaDelJugador != null)
			{
				ReinaDelJugador.GlobalPosition = posicionNueva;
				GameManager.Instance.ConsumirMovimiento();
			}
		}

	}

	public void Atacar()
	{
		if (!GameManager.Instance.PuedeAtacar())
			return;

		GD.Print("Atacó recién");
		GameManager.Instance.ConsumirAtaque();
	}

	public void CargarAssets(){
		List<PackedScene> escenas = new List<PackedScene>(){
			GD.Load<PackedScene>("res://Scenes/AbejaReina.tscn"),
			GD.Load<PackedScene>("res://Scenes/AbejaReina2.tscn"),
			GD.Load<PackedScene>("res://Scenes/AbejaReina.tscn"),
			GD.Load<PackedScene>("res://Scenes/Zangano.tscn")
		};

		for (int i = 0; i < GameManager.Instance.JugadoresEnPartida.Count; i++)
		{
			if (i < escenas.Count)
			{
				Assets.Add(escenas[i]);
			}
		}
	}

	public void InstanciarJugadores()
	{
		CargarAssets(); 

		for (int j = 0; j < GameManager.Instance.cantidadJugadores; j++)
		{
			var InstanciaNueva = Assets[j].Instantiate<Node3D>();
			AddChild(InstanciaNueva);
			VisualesJugadores.Add(InstanciaNueva);
		}
	}

	public void EstablecerSpawns()
	{ 
		List<Vector3> posiciones = new List<Vector3>() 
		{
			new Vector3(-6, 0, 2), 
			new Vector3(25, 0, 3), 
			new Vector3(3, 0, 14), 
			new Vector3(19, 0, 19)
		};

		for (int i = 0; i < VisualesJugadores.Count; i++)
		{
			if (i < posiciones.Count)
			{
				VisualesJugadores[i].GlobalPosition = posiciones[i];
			}
		}
	}
}
 */

/* private void IntentarMoverJugador()
	{
		Vector2 mousePosition = GetViewport().GetMousePosition();
		Vector3 rayOrigin = camera.ProjectRayOrigin(mousePosition);
		Vector3 rayEnd = rayOrigin + camera.ProjectRayNormal(mousePosition) * 1000.0f;

		var spaceState = GetWorld3D().DirectSpaceState;
		var query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
		var result = spaceState.IntersectRay(query);

		if (result.Count > 0)
		{
			Vector3 impactoWorld = result["position"].AsVector3();
			Celda celdaCliqueada = ObtenerCeldaDesdePosicion(impactoWorld);

			if (celdaCliqueada == null) return;

			Node3D reinaActual = VisualesJugadores[GameManager.Instance.jugadorEnTurno.Id - 1];

			if (!_celdaActualPorJugador.ContainsKey(reinaActual))
			{
				// Si por alguna razón no tenía celda asignada, le asignamos la más cercana a su posición actual
				_celdaActualPorJugador[reinaActual] = ObtenerCeldaDesdePosicion(reinaActual.GlobalPosition);
			}

			Celda celdaOrigen = _celdaActualPorJugador[reinaActual];

			int valorDado = GameManager.Instance.jugadorEnTurno.MovimientosDisponibles;
			
			// Si el dado dio 0 o no se ha tirado, no permite mover
			if (valorDado <= 0) return;

			List<Celda> celdasValidas = tablero.ObtenerCeldasAlcanzables(celdaOrigen, valorDado);

			if (celdasValidas.Contains(celdaCliqueada))
			{
				MoverAbejaACelda(reinaActual, celdaCliqueada);
			}
			else
			{
				GD.Print("La celda está fuera del rango del dado (" + valorDado + ")");
			}
		}
	} */
