using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class PlayerManager : Node3D
{
	[Export] private Camera3D camera;
	[Export] private Tablero tablero; 
	[Export] public TroopsManager tropasManager; 

	public readonly List<Node3D> VisualesJugadores = new();
	public readonly List<Node3D> OutlinesJugadores = new();
	private readonly List<PackedScene> Assets = new();
	private readonly Dictionary<Node3D, Celda> CeldaActualPorJugador = new();
	private List<Celda> CeldasDisponibles = new();
	private List<Abeja> AbejasObjetivo = new();

	private MovimientoManager movimientoManager;
	private AtaqueManager ataqueManager;

	public Node3D VisualJugadorActual;
	public Vector3 PosicionEnMundo3D;
	public Celda CeldaCliqueada;
	public Celda CeldaOrigen;

	public override async void _Ready()
	{
		movimientoManager = new MovimientoManager(tablero);
		ataqueManager = new AtaqueManager();

		InstanciarJugadores();
		//GuardarOutlines();
		CallDeferred(nameof(EstablecerSpawnsEnCeldas));

		if (GameManager.Instance.TurnManager != null)
		{
			GameManager.Instance.TurnManager.OnCambioDeTurnoJugador += OnCambioDeTurnoJugador;
		}

		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		GameManager.Instance.CamaraActual.EnfocarNodo(VisualJugadorActual, 5, 5);
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
		
		if(GameManager.Instance.jugadorEnTurno.TiroLosDados && !GameManager.Instance.jugadorEnTurno.ModoInvocacion && !GameManager.Instance.jugadorEnTurno.ModoAtaque){
			IntentarMoverJugador();
		}
		
		if(GameManager.Instance.jugadorEnTurno.TiroLosDados && GameManager.Instance.jugadorEnTurno.ModoInvocacion && !GameManager.Instance.jugadorEnTurno.ModoAtaque){
			InvocarAbejaNueva();
		}

		if(GameManager.Instance.jugadorEnTurno.TiroLosDados && GameManager.Instance.jugadorEnTurno.ModoAtaque && !GameManager.Instance.jugadorEnTurno.ModoInvocacion){
			Atacar();
		}
	}

	public bool CeldaTieneOtraReina(Celda celda, Node3D jugadorActual, Dictionary<Node3D, Celda> celdasOcupadas, List<Node3D> visualesJugadores)
	{
		foreach (var keyValuePair in celdasOcupadas)
		{
			if (keyValuePair.Key == jugadorActual)
			continue;

			if (keyValuePair.Value == celda)
			{
				int indexJugador = visualesJugadores.IndexOf(keyValuePair.Key);
				if (indexJugador != -1)
				{
					AbejaReina jugadorOcupante = GameManager.Instance.JugadoresEnPartida[indexJugador];

					if (jugadorOcupante.FueraDeJuego)
					{
						continue; 
					}
				}
				return true;
			}
		}
		return false;
	}

	public bool PuedeMoverseEntre(Celda origen, Celda destino, Dictionary<Node3D, Celda> celdasOcupadas,
	Node3D jugadorActual, List<Node3D> visualesJugadores)
	{
		if(origen == null || destino == null)
		return false;

		if(!movimientoManager.CeldasSonAdyacentes(origen, destino))
		return false;

		if(CeldaTieneOtraReina(destino, jugadorActual, celdasOcupadas, visualesJugadores))
		return false;

		if(movimientoManager.CeldaTieneOtraAbeja(destino))
		return false;

		if(GameManager.Instance.jugadorEnTurno.ModoInvocacion)
		return false; 

		if(GameManager.Instance.jugadorEnTurno.ModoAtaque)
		return false; 

		//List<Celda> vecinos = tablero.ObtenerVecinos(origen);
		//return vecinos.Contains(destino);

		return true;
	}

	public Celda ObtenerCeldaDesdePosicion(List<Celda> Celdas, Vector3 posicion) 
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

	private void IntentarMoverJugador()
	{       
		if (GameManager.Instance.jugadorEnTurno.MovimientosDisponibles <= 0)
			return;

		int indexJugador = GameManager.Instance.jugadorEnTurno.Id - 1;
		if (indexJugador < 0 || indexJugador >= VisualesJugadores.Count)
		{
			GD.PrintErr("Índice inválido para VisualesJugadores");
			return;
		}

		VisualJugadorActual = VisualesJugadores[indexJugador];

		Vector2 mousePosition = GetViewport().GetMousePosition();
		Vector3 rayOrigin = camera.ProjectRayOrigin(mousePosition);
		Vector3 rayEnd = rayOrigin + camera.ProjectRayNormal(mousePosition) * 1000.0f;
	
		var spaceState = GetWorld3D().DirectSpaceState;
		var query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
		var result = spaceState.IntersectRay(query);

		if(result.Count == 0)
			return;

		PosicionEnMundo3D = result["position"].AsVector3();
		CeldaCliqueada = ObtenerCeldaDesdePosicion(tablero.Celdas, PosicionEnMundo3D);
	
		if(CeldaCliqueada == null)
			return;

		EstablecerCeldaParaJugadorEnTurno();

		if(PuedeMoverseEntre(CeldaOrigen, CeldaCliqueada, CeldaActualPorJugador, VisualJugadorActual, VisualesJugadores))
		{
			OcultarCeldasDisponiblesParaInvocar();
			OcultarAbejasObjetivo();
			MoverAbejaACelda(VisualJugadorActual, CeldaCliqueada);

		}
		else
		{
			GD.Print("Solo puedes moverte a una celda contigua o vecina vacía.");
		}

	}

	private void EstablecerCeldaParaJugadorEnTurno()
	{
		if (!CeldaActualPorJugador.ContainsKey(VisualJugadorActual))
		{
			CeldaActualPorJugador[VisualJugadorActual] = ObtenerCeldaDesdePosicion(tablero.Celdas, VisualJugadorActual.GlobalPosition);
		}
	
		CeldaOrigen = CeldaActualPorJugador[VisualJugadorActual];
	
		if (CeldaCliqueada == CeldaOrigen) return;
	}

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

		int index = VisualesJugadores.IndexOf(jugador);
		if (index != -1)
		{
			GameManager.Instance.JugadoresEnPartida[index].UbicacionActual = celdaDestino;
			GameManager.Instance.CamaraActual.EnfocarNodo(VisualJugadorActual, 2, 1);
		}

		GameManager.Instance.ConsumirMovimiento();
		GameManager.Instance.NotificarCambioDeEstado();
		
		var sound = AudioManager.Instance.GameAudio.Sound2;
		AudioManager.Instance.PlaySound(sound);
	}

	public void Atacar()
	{
		if (!GameManager.Instance.PuedeAtacar())
			return;

		Vector2 mousePosition = GetViewport().GetMousePosition();
		Vector3 rayOrigin = camera.ProjectRayOrigin(mousePosition);
		Vector3 rayEnd = rayOrigin + camera.ProjectRayNormal(mousePosition) * 1000.0f;
	
		var spaceState = GetWorld3D().DirectSpaceState;
		var query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
		var result = spaceState.IntersectRay(query);

		if(result.Count == 0)
		return;

		PosicionEnMundo3D = result["position"].AsVector3();
		CeldaCliqueada = ObtenerCeldaDesdePosicion(tablero.Celdas, PosicionEnMundo3D);

		if (CeldaCliqueada == null)
		return;
	
		AtacarJugador();
	
		AtacarAbeja();
	}

	private List<MeshInstance3D> ObtenerTodosLosMeshes(Node3D visual){
		var meshes = new List<MeshInstance3D>();

		if (visual == null) return meshes;

		foreach (var node in visual.FindChildren("*", "MeshInstance3D"))
		{
			if (node is MeshInstance3D mesh)
			{
				meshes.Add(mesh);
			}
		}
		return meshes;
	}

	private void EstablecerNextPass(Node3D visual, Material materialOutline)
	{
		var meshes = ObtenerTodosLosMeshes(visual);

		foreach (var mesh in meshes)
		{
			if (mesh.Mesh == null) continue;

			int cantidadSuperficies = mesh.Mesh.GetSurfaceCount();

			for (int i = 0; i < cantidadSuperficies; i++)
			{
				var materialBase = mesh.GetActiveMaterial(i);
				
				if (materialBase == null) continue;

				if (!materialBase.IsLocalToScene())
				{
					materialBase = (Material)materialBase.Duplicate();
					mesh.SetSurfaceOverrideMaterial(i, materialBase);
				}

				if (materialOutline != null && materialBase == materialOutline)
				{
					GD.PrintErr($"[PlayerManager] Conflicto de material en {mesh.Name} (Superficie {i}): El material base y el outline son la misma instancia.");
					continue;
				}

				materialBase.NextPass = materialOutline;
			}
		}
	}

	public void MostrarJugadoresObjetivo(){
		var materialAtaque = GD.Load<StandardMaterial3D>("res://outlineAttack.tres");
		var jugadoresObjetivo = VisualesJugadores.Where(j => JugadorEnTurnoAdyacenteAOtro(j)).ToList();

		foreach (var jugador in jugadoresObjetivo)
		{
			if (jugador != VisualJugadorActual)
			{
				EstablecerNextPass(jugador, materialAtaque);
			}
		}
	}

	public void OutlineJugadorEnTurno(Node3D visualActual){
		var IndiceDeJugadorEnTurno = GameManager.Instance.jugadorEnTurno.Id - 1;
		
		var materialTurnoJ1 = GD.Load<StandardMaterial3D>("res://outlineJugador1.tres");
		var materialTurnoJ2 = GD.Load<StandardMaterial3D>("res://outlineJugador2.tres");
		var materialTurnoJ3 = GD.Load<StandardMaterial3D>("res://outlineJugador3.tres");
		var materialTurnoJ4 = GD.Load<StandardMaterial3D>("res://outlineJugador4.tres");

		List<StandardMaterial3D> OutlinesJugadores = new List<StandardMaterial3D>{ materialTurnoJ1, materialTurnoJ2, materialTurnoJ3, materialTurnoJ4 };

		EsconderOutlineDeJugadores();

		if (visualActual != null)
		{
			EstablecerNextPass(visualActual, OutlinesJugadores[IndiceDeJugadorEnTurno]);
		}
	}

	public void EsconderOutlineDeJugadores()
	{
		foreach (var visual in VisualesJugadores)
		{
			if (visual != VisualJugadorActual)
			{
				EstablecerNextPass(visual, null);
			}
		}
	}

	public bool TieneObjetivosCerca(){
		var otroJugadorCerca = VisualesJugadores.Any(v => JugadorEnTurnoAdyacenteAOtro(v));
		
		return (AbejasObjetivo.Count > 0) || otroJugadorCerca;
	}

	private void AtacarAbeja(){
		var abejaObjetivo = AbejasObjetivo.Find(a => a.CeldaActual == CeldaCliqueada);

		if(abejaObjetivo == null){
			GD.Print("No hay ninguna abeja objetivo en la celda seleccionada.");
			return;
		}

		ataqueManager.DaniarAbeja(abejaObjetivo);
		
		if (tropasManager.VisualAbejas[abejaObjetivo] != null && IsInstanceValid(tropasManager.VisualAbejas[abejaObjetivo]))
		{
			tropasManager.DespintarCeldaDeAbeja(abejaObjetivo.CeldaActual, abejaObjetivo);
			tropasManager.VisualAbejas[abejaObjetivo].QueueFree();
		}

		LimpiarAbejasEliminadas();
		GameManager.Instance.ConsumirAtaque();
	}

	private void AtacarJugador(){
		VisualJugadorActual = VisualesJugadores[GameManager.Instance.jugadorEnTurno.Id - 1];

		Celda celdaAtacante = CeldaActualPorJugador[VisualJugadorActual];

		List<Celda> celdasAdyacentes = tablero.ObtenerVecinos(celdaAtacante);

		AbejaReina reinaObjetivo = GameManager.Instance.JugadoresEnPartida.Find(j => !j.FueraDeJuego && j != GameManager.Instance.jugadorEnTurno && j.UbicacionActual == CeldaCliqueada);

		if (reinaObjetivo != null){
			if (celdasAdyacentes.Any(c => c == reinaObjetivo.UbicacionActual))
			{
				Node3D visualRival = VisualesJugadores[reinaObjetivo.Id - 1];

				GD.Print("¡Ataque exitoso al jugador " + reinaObjetivo.Id + "!");
	
				EfectuarAtaque(visualRival, reinaObjetivo.Id - 1);

				OcultarAbejasObjetivo();
				GameManager.Instance.ConsumirAtaque();
			}
			else
			{
				GD.Print("La Abeja Reina rival está demasiado lejos para ser atacada.");
			}
		}
		else
		{
			GD.Print("No hay ninguna Abeja Reina rival en la celda seleccionada.");
		}
	}

	private void EfectuarAtaque(Node3D enemigo, int Id){
		AbejaReina jugador = GameManager.Instance.JugadoresEnPartida[Id];

		if(jugador is AbejaReina){
			ataqueManager.DaniarJugador(jugador);
			GD.Print("Jugador " + jugador.Id + " ahora tiene " + jugador.HP + " puntos de vida");
		}

		if(ataqueManager.JugadorEstaEliminado(jugador)){
			EliminarInstanciaDeJugador(enemigo, Id);
		}
	}

	private void EliminarInstanciaDeJugador(Node3D unJugador, int Id){
		GameManager.Instance.EliminarJugador(Id);
		unJugador.Visible = false;

		if(GameManager.Instance.CondicionVictoria()){
			GameManager.Instance.EstablecerJugadorGanador();
			GetTree().ChangeSceneToFile("res://Scenes/pantallaVictoria.tscn"); 
		};
	}

	public void CargarAssets()
	{
		List<PackedScene> meshPlayers = new List<PackedScene>(){
			GD.Load<PackedScene>("res://Scenes/AbejaReina.tscn"),
			GD.Load<PackedScene>("res://Scenes/AbejaReina2.tscn"),
			GD.Load<PackedScene>("res://Scenes/AbejaReina3.tscn"),
			GD.Load<PackedScene>("res://Scenes/AbejaReina4.tscn")
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

	public void MostrarAbejasObjetivo(){
		var materialAtaque = GD.Load<StandardMaterial3D>("res://outlineAttack.tres");

		AbejasObjetivo.Clear();
		
		VisualJugadorActual = VisualesJugadores[GameManager.Instance.jugadorEnTurno.Id - 1];

		CeldasDisponibles = tablero.ObtenerVecinos(CeldaActualPorJugador[VisualJugadorActual]);

		foreach (var jugador in GameManager.Instance.JugadoresEnPartida)
		{
			if(jugador == GameManager.Instance.jugadorEnTurno || jugador.FueraDeJuego){
				continue;
			}

			BuscarAbejasObjetivo(jugador, AbejasObjetivo);
		}

		foreach (var abejaVisual in AbejasObjetivo)
		{
			if(tropasManager.VisualAbejas[abejaVisual] != null)
			{
				EstablecerNextPass(tropasManager.VisualAbejas[abejaVisual], materialAtaque);
			} 
		}
	}

	public void OcultarAbejasObjetivo(){
		VisualJugadorActual = VisualesJugadores[GameManager.Instance.jugadorEnTurno.Id - 1];

		CeldasDisponibles = tablero.ObtenerVecinos(CeldaActualPorJugador[VisualJugadorActual]);

		foreach (var jugador in GameManager.Instance.JugadoresEnPartida)
		{
			if(jugador == GameManager.Instance.jugadorEnTurno || jugador.FueraDeJuego){
				continue;
			}

			BuscarAbejasObjetivo(jugador, AbejasObjetivo);
		}

		foreach (var abejaVisual in AbejasObjetivo)
		{
			if(abejaVisual != null)
			{
				EstablecerNextPass(tropasManager.VisualAbejas[abejaVisual], null);
			} 
		}
	}

	public void BuscarAbejasObjetivo(AbejaReina jugadorRival, List<Abeja> listaDeAbejas){
		var jugadorActual = GameManager.Instance.jugadorEnTurno;
		
		foreach (var abejaActual in jugadorRival.ColmenaDeReina.AbejasDeColmena)
		{
			if (CeldasDisponibles.Contains(abejaActual.CeldaActual) 
			&& movimientoManager.CeldasSonAdyacentes(jugadorActual.UbicacionActual, abejaActual.CeldaActual) 
			&& !abejaActual.FueraDeJuego
			&& !jugadorActual.ColmenaDeReina.AbejasDeColmena.Contains(abejaActual))
			{
				listaDeAbejas.Add(abejaActual);
			}
		}
	}

	public void LimpiarAbejasEliminadas(){
		foreach (var abeja in AbejasObjetivo.Where(a => a.FueraDeJuego).ToList())
		{
			if(abeja.FueraDeJuego){
				AbejasObjetivo.Remove(abeja);
				abeja.ColmenaHogar.AbejasDeColmena.Remove(abeja);
			}

			AbejasObjetivo.RemoveAll(a => a.FueraDeJuego);
		}
	}

	public void MostrarCeldasDisponiblesParaInvocar(){
		VisualJugadorActual = VisualesJugadores[GameManager.Instance.jugadorEnTurno.Id - 1];

		CeldasDisponibles = tablero.ObtenerVecinos(CeldaActualPorJugador[VisualJugadorActual]);
		
		foreach (var jugador in VisualesJugadores)
		{
			if(JugadorEnTurnoAdyacenteAOtro(jugador)){
				var celdaOcupada = CeldaActualPorJugador[jugador];

				CeldasDisponibles = CeldasDisponibles.Where(c => c != celdaOcupada).ToList();

				GD.Print("Hay otro jugador cerca");
			}
		}

		foreach (var reina in GameManager.Instance.JugadoresEnPartida)
		{
			for (int e = 0; e < CeldasDisponibles.Count; e++)
			{
				var abejasEnCeldas = reina.ColmenaDeReina.AbejasDeColmena.Where(a => a.CeldaActual == CeldasDisponibles[e] && !a.FueraDeJuego).ToList();

				foreach (var abeja in abejasEnCeldas)
				{
					var celdaOcupada = CeldasDisponibles.Find(c => c == abeja.CeldaActual);

					CeldasDisponibles.Remove(celdaOcupada);
				}
			}
		}

		foreach (var celda in CeldasDisponibles)
		{
			celda.Tile.GetNode<Node3D>("Outline").Visible = true;
		}
	}

	public void OcultarCeldasDisponiblesParaInvocar(){
		foreach (var celda in CeldasDisponibles)
		{
			celda.Tile.GetNode<Node3D>("Outline").Visible = false;
		}
	}

	public void InvocarAbejaNueva(){
		Vector2 mousePosition = GetViewport().GetMousePosition();
		Vector3 rayOrigin = camera.ProjectRayOrigin(mousePosition);
		Vector3 rayEnd = rayOrigin + camera.ProjectRayNormal(mousePosition) * 1000.0f;
	
		var spaceState = GetWorld3D().DirectSpaceState;
		var query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
		var result = spaceState.IntersectRay(query);

		if(result.Count == 0)
		return;

		PosicionEnMundo3D = result["position"].AsVector3();
		CeldaCliqueada = ObtenerCeldaDesdePosicion(tablero.Celdas, PosicionEnMundo3D);
	
		if(CeldaCliqueada == null)
		return;

		if(CeldasDisponibles.Contains(CeldaCliqueada)){
			OcultarCeldasDisponiblesParaInvocar();	
			OcultarAbejasObjetivo();
			tropasManager.InstanciarAbeja(CeldaCliqueada);
			var bee1 = AudioManager.Instance.GameAudio.Sound5;
			AudioManager.Instance.PlaySound(bee1);
		}
		else{
			GD.Print("No se puede invocar una abeja sobre esta celda");
		}
	}

	private void OnCambioDeTurnoJugador(AbejaReina jugadorNuevo)
	{
		if (jugadorNuevo == null) return;

		Node3D visualJugador = VisualesJugadores[jugadorNuevo.Id - 1]; 

		if (GameManager.Instance.CamaraActual != null && IsInstanceValid(visualJugador))
		{
			GameManager.Instance.CamaraActual.EnfocarNodo(visualJugador, 2, 1);
		}
		
		LimpiarAbejasEliminadas();
	}

	public override void _ExitTree()
	{
		if (GameManager.Instance.TurnManager != null)
		{
			GameManager.Instance.TurnManager.OnCambioDeTurnoJugador -= OnCambioDeTurnoJugador;
		}
	}
}
