using Godot;
using System.Collections.Generic;

public partial class PlayerManager : Node3D
{
	[Export] private Camera3D camera;
	[Export] private Tablero tablero; // Asegúrate de asignarlo en el Inspector de Godot

	private List<Node3D> VisualesJugadores = new List<Node3D>();
	private List<PackedScene> Assets = new List<PackedScene>();
	
	private Dictionary<Node3D, Celda> _celdaActualPorJugador = new Dictionary<Node3D, Celda>();

	public override void _Ready()
	{
		InstanciarJugadores();
		// Esperamos un frame a que Tablero ejecute su _Ready y SetTiles
		CallDeferred(nameof(EstablecerSpawnsEnCeldas));
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!GameManager.Instance.PuedeMover())
			return;

		if (@event.IsActionPressed("move"))
		{
			if (GetViewport().GuiGetHoveredControl() != null)
				return;

			if (camera == null)
				camera = GetViewport().GetCamera3D();

			IntentarMoverJugador();
		}
	}

	private void IntentarMoverJugador()
	{
		// Si no le quedan movimientos, no intenta procesar nada
		if (GameManager.Instance.jugadorEnTurno.MovimientosDisponibles <= 0)
			return;
	
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
				_celdaActualPorJugador[reinaActual] = ObtenerCeldaDesdePosicion(reinaActual.GlobalPosition);
			}
	
			Celda celdaOrigen = _celdaActualPorJugador[reinaActual];
	
			// No puedes hacer clic sobre la misma celda en la que ya estás parado
			if (celdaCliqueada == celdaOrigen) return;
	
			// Obtenemos únicamente las celdas vecinas a 1 paso de distancia
			List<Celda> vecinosAdyacentes = tablero.ObtenerVecinos(celdaOrigen);
	
			if (vecinosAdyacentes.Contains(celdaCliqueada))
			{
				MoverAbejaACelda(reinaActual, celdaCliqueada);
			}
			else
			{
				GD.Print("Solo puedes moverte a una celda contigua/vecina.");
			}
		}
	}

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

	private Celda ObtenerCeldaDesdePosicion(Vector3 posicion)
	{
		if (tablero == null || tablero.Celdas == null || tablero.Celdas.Count == 0)
			return null;

		Celda celdaMasCercana = null;
		float distanciaMinima = float.MaxValue;

		foreach (Celda celda in tablero.Celdas)
		{
			if (celda.Tile == null) continue;

			float dist = celda.Tile.GlobalPosition.DistanceTo(posicion);
			if (dist < distanciaMinima)
			{
				distanciaMinima = dist;
				celdaMasCercana = celda;
			}
		}

		return celdaMasCercana;
	}

	private void MoverAbejaACelda(Node3D reina, Celda celdaDestino)
	{
		Vector3 targetPos = celdaDestino.Tile.GlobalPosition;
		targetPos.Y = reina.GlobalPosition.Y; 

		reina.GlobalPosition = targetPos;
		_celdaActualPorJugador[reina] = celdaDestino;

		GameManager.Instance.ConsumirMovimiento();
	}

	public void Atacar()
	{
		if (!GameManager.Instance.PuedeAtacar())
			return;

		GD.Print("Atacó recién");
		GameManager.Instance.ConsumirAtaque();
	}

	public void CargarAssets()
	{
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

	public void EstablecerSpawnsEnCeldas()
	{
		if (tablero == null || tablero.Celdas == null || tablero.Celdas.Count == 0)
			return;

		// Asignamos esquinas opuestas o celdas separadas del tablero para cada jugador
		int totalCeldas = tablero.Celdas.Count;

		for (int i = 0; i < VisualesJugadores.Count; i++)
		{
			// Distribuimos los spawns a lo largo de la lista de celdas para que no aparezcan juntas
			int indiceCelda = (i * (totalCeldas / VisualesJugadores.Count)) % totalCeldas;
			
			Celda celdaInicio = tablero.Celdas[indiceCelda]; 
			Node3D reina = VisualesJugadores[i];

			Vector3 targetPos = celdaInicio.Tile.GlobalPosition;
			targetPos.Y = reina.GlobalPosition.Y;

			reina.GlobalPosition = targetPos;
			_celdaActualPorJugador[reina] = celdaInicio;
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
