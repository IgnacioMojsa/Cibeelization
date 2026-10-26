using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

public partial class PlayerManager : Node3D
{
	[Export] public Camera3D camera;
	[Export] public Tablero tablero; 
	[Export] public TroopsManager tropasManager; 
	[Export] public CeldasManager celdasManager; 
	[Export] public AnimationManager animationManager;
	[Export] public VFXManager vfxManager;
	[Export] public AttackManager attackManager;

	public readonly Dictionary<AbejaReina, Node3D> VisualesJugadores = new();
	private readonly List<PackedScene> Assets = new();
	public Dictionary<Node3D, Celda> CeldaActualPorJugador = new();
	public List<Abeja> AbejasObjetivo = new();

	public MovimientoManager movimientoManager;

	public Node3D VisualJugadorActual;
	public Vector3 PosicionEnMundo3D;
	public Celda CeldaCliqueada;
	public Celda CeldaOrigen;
	public Type TransformacionSeleccionada;
	public AudioSetting UiSound4 => AudioManager.Instance?.GameAudio?.Sound4;
	
	public override async void _Ready()
	{
		movimientoManager = new MovimientoManager(tablero);

		InstanciarJugadores();
		CallDeferred(nameof(EstablecerSpawnsEnCeldas));

		if (GameManager.Instance.TurnManager != null)
		{
			GameManager.Instance.TurnManager.OnCambioDeTurnoJugador += OnCambioDeTurnoJugador;
		}

		GameManager.Instance.OnTurnoTerminadoPorMovimientos += ManejarTurnoTerminadoPorMovimientos;

		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		GameManager.Instance.CamaraActual.EnfocarNodo(VisualJugadorActual, 5, 5);
	}

	public override void _UnhandledInput(InputEvent @event)
	{	
		var jugador = GameManager.Instance.jugadorEnTurno;
		
		if (!@event.IsActionPressed("move"))
			return; 

		if (GetViewport().GuiGetHoveredControl() != null)
			return;

		camera ??= GetViewport().GetCamera3D();
		
		if(jugador.ModoInvocacion && !jugador.AccionConsumida){
			InvocarAbejaNueva();
		}
		
		else if(jugador.ModoAtaque && !jugador.AccionConsumida){
			attackManager.Atacar();
		}

		else if(jugador.ModoTransformacion && !jugador.AccionConsumida){
			CeldaCliqueada = ObtenerCeldaAlClickear();
			tropasManager.TransformarAbeja(CeldaCliqueada);
		}

		else if(jugador.ModoAbsorcion && !jugador.AccionConsumida){
			CeldaCliqueada = ObtenerCeldaAlClickear();
			tropasManager.AbsorberSubdito(CeldaCliqueada);
		}

		else if(jugador.MenteColmena && !jugador.AccionConsumida && !jugador.MoviendoSubdito){
			CeldaCliqueada = ObtenerCeldaAlClickear();
			tropasManager.ControlarAbeja(CeldaCliqueada);
		}

		else if(jugador.MenteColmena && jugador.MoviendoSubdito)
		{
			CeldaCliqueada = ObtenerCeldaAlClickear();
			tropasManager.IntentarMoverSubdito(CeldaCliqueada);
		}

		else{
			IntentarMoverJugador();
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

		if(GameManager.Instance.jugadorEnTurno.ModoTransformacion)
		return false;

		//List<Celda> vecinos = tablero.ObtenerVecinos(origen);
		//return vecinos.Contains(destino);

		return true;
	}

	private Celda ObtenerCeldaAlClickear()
	{
		Vector2 mousePosition = GetViewport().GetMousePosition();
		Vector3 rayOrigin = camera.ProjectRayOrigin(mousePosition);
		Vector3 rayEnd = rayOrigin + camera.ProjectRayNormal(mousePosition) * 1000.0f;
	
		var spaceState = GetWorld3D().DirectSpaceState;
		var query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
		var result = spaceState.IntersectRay(query);

		if(result.Count == 0) return null;

		PosicionEnMundo3D = result["position"].AsVector3();
		CeldaCliqueada = ObtenerCeldaDesdePosicion(tablero.Celdas, PosicionEnMundo3D);
	
		return CeldaCliqueada;
	}

	public Celda ObtenerCeldaDesdePosicion(List<Celda> Celdas, Vector3 posicion) 
	{
		if (tablero == null || tablero.Celdas == null || tablero.Celdas.Count == 0) return null;

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

	private void IntentarMoverJugador(){      
	if (GameManager.Instance.jugadorEnTurno.MovimientosDisponibles <= 0) return;

	VisualJugadorActual = VisualesJugadores[GameManager.Instance.jugadorEnTurno];

	CeldaCliqueada = ObtenerCeldaAlClickear();
	
	RotarJugadorHex(VisualJugadorActual, CeldaCliqueada);

	EstablecerCeldaParaJugadorEnTurno();

	if (PuedeMoverseEntre(CeldaOrigen, CeldaCliqueada, CeldaActualPorJugador, VisualJugadorActual, VisualesJugadores.Values.ToList()))
	{
		vfxManager.OcultarAbejasObjetivo();

		int indexJugador = VisualesJugadores.Values.ToList().IndexOf(VisualJugadorActual);
		int indexCelda = tablero.Celdas.IndexOf(CeldaCliqueada);
		Vector3 targetPos = CeldaCliqueada.Tile.GlobalPosition;
		targetPos.Y = VisualJugadorActual.GlobalPosition.Y;

		if (GameManager.Instance.modoMultiplayer)
		{
			Rpc(nameof(MoverAbejaACeldaRpc), indexJugador, targetPos, indexCelda);
		}
		else
		{
			MoverAbejaACelda(VisualJugadorActual, CeldaCliqueada);
		}
	}
	else
	{
		GD.Print("Solo puedes moverte a una celda contigua o vecina vacía.");
		EventosUI.MostrarMensaje("Solo podés moverte a una celda vecina vacía.");
	}
}

	public void RotarJugadorHex(Node3D jugadorVisual, Celda celdaDestino)
	{
		Vector3 origen = jugadorVisual.GlobalPosition;
		Vector3 destino = celdaDestino.Tile.GlobalPosition;

		Vector3 dir = (destino - origen).Normalized();
		dir.Y = 0; // solo plano XZ

		// Ángulo en radianes
		float anguloMouse = Mathf.Atan2(dir.X, dir.Z);

		// Convertir a grados
		float grados = Mathf.RadToDeg(anguloMouse);

		// Normalizar entre 0–360
		if (grados < 0) grados += 360f;

		// Dividir en 6 sectores de 60°
		int sector = (int)System.Math.Round(grados / 60.0);

		// Calcular ángulo fijo en grados
		float anguloFinal = (sector * 60.0f) + 180f; // Ajuste de 180° para que mire hacia el destino

		// Pasar a radianes
		float anguloFinalRad = Mathf.DegToRad(anguloFinal);

		// Aplicar rotación en Y
		jugadorVisual.Rotation = new Vector3(0, anguloFinalRad, 0);
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

	public bool JugadorEnTurnoAdyacenteAOtro(Node3D otroJugador){
		List<Celda> VecinosAdyacentes = tablero.ObtenerVecinos(CeldaActualPorJugador[VisualJugadorActual]);
		Celda CeldaOtroJugador = CeldaActualPorJugador[otroJugador];
		return VecinosAdyacentes.Contains(CeldaOtroJugador);
	}

	private void MoverAbejaACelda(Node3D jugador, Celda celdaDestino)
	{
		if(GameManager.Instance.jugadorEnTurno.AtaquePotenciado){
			celdasManager.DespintarCeldaDeAbejaBuffeada();
		}

		Vector3 targetPos = celdaDestino.Tile.GlobalPosition;
		targetPos.Y = jugador.GlobalPosition.Y; 

		jugador.GlobalPosition = targetPos;
		CeldaActualPorJugador[jugador] = celdaDestino;

		int index = VisualesJugadores.Values.ToList().IndexOf(jugador);
		if (index != -1)
		{
			GameManager.Instance.JugadoresEnPartida[index].CeldaActual = celdaDestino;
			GameManager.Instance.CamaraActual.EnfocarNodo(VisualJugadorActual, 2, 1);
		}

		_ = GameManager.Instance.ConsumirMovimiento();
		GameManager.Instance.NotificarCambioDeEstado();
		
		var sound = AudioManager.Instance.GameAudio.Sound2;
		AudioManager.Instance.PlaySound(sound);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void MoverAbejaACeldaRpc(int indexJugador, Vector3 nuevaPosicion, int indiceCeldaDestino)
	{
		var reinaJugador = GameManager.Instance.JugadoresEnPartida[indexJugador];
		var reinaVisual = VisualesJugadores[reinaJugador];
		var celdaDestino = tablero.Celdas[indiceCeldaDestino];

		reinaVisual.GlobalPosition = nuevaPosicion;
		CeldaActualPorJugador[reinaVisual] = celdaDestino;
		reinaJugador.CeldaActual = celdaDestino;

		GameManager.Instance.CamaraActual?.EnfocarNodo(reinaVisual, 2, 1);

		_ = GameManager.Instance.ConsumirMovimiento();
		GameManager.Instance.NotificarCambioDeEstado(); 

		var sound = AudioManager.Instance.GameAudio.Sound2;
		AudioManager.Instance.PlaySound(sound);
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

//mejorar codigo aca
	public void InstanciarJugadores()
	{
		CargarAssets();
		if (!GameManager.Instance.modoMultiplayer)
		{
			for (int j = 0; j < GameManager.Instance.cantidadJugadores; j++)
			{
				var Reina = GameManager.Instance.JugadoresEnPartida[j];
				var InstanciaNueva = Assets[j].Instantiate<Node3D>();
				AddChild(InstanciaNueva);
				VisualesJugadores.Add(Reina, InstanciaNueva);
			}
			animationManager.GuardarInstanciasDeJugadores(VisualesJugadores);
		}
		else
		{
			for (int j = 0; j < GameManager.Instance.JugadoresEnPartida.Count; j++)
			{
				var Reina = GameManager.Instance.JugadoresEnPartida[j];
				if(j < Assets.Count)
				{
					var InstanciaNueva = Assets[j].Instantiate<Node3D>();
					if(Reina.Id > 0)
					{
						InstanciaNueva.SetMultiplayerAuthority((int)Reina.Id);
					}
					AddChild(InstanciaNueva);
					VisualesJugadores.Add(Reina, InstanciaNueva);
					GD.Print($"Jugador instanciado en red con ID: {Reina.Id}");
				}
			}
			animationManager.GuardarInstanciasDeJugadores(VisualesJugadores);
		}
	}
		

	public void EstablecerSpawnsEnCeldas()
	{
		if (tablero == null || tablero.Celdas == null || tablero.Celdas.Count == 0) return;

		int totalCeldas = tablero.Celdas.Count;

		for (int i = 0; i < VisualesJugadores.Count; i++)
		{
			int indiceCelda = i * totalCeldas / VisualesJugadores.Count % totalCeldas;
			
			Celda celdaInicio = tablero.Celdas[indiceCelda]; 
			
			AbejaReina reinaJugador = GameManager.Instance.JugadoresEnPartida[i];
			Node3D reinaVisual = VisualesJugadores[reinaJugador];
			//Node3D reina = VisualesJugadores[GameManager.Instance.jugadorEnTurno];

			Vector3 targetPos = celdaInicio.Tile.GlobalPosition;
			targetPos.Y = reinaVisual.GlobalPosition.Y;

			reinaVisual.GlobalPosition = targetPos;
			CeldaActualPorJugador[reinaVisual] = celdaInicio;
			reinaJugador.CeldaActual = celdaInicio;
			//GameManager.Instance.JugadoresEnPartida[i].CeldaActual = celdaInicio;

			GD.Print(
				"Jugador " + i + " ubicado en " 
				+ GameManager.Instance.JugadoresEnPartida[i].CeldaActual 
				+ (GameManager.Instance.JugadoresEnPartida[i].CeldaActual.Q,
				GameManager.Instance.JugadoresEnPartida[i].CeldaActual.R)
			);
		}
	}

	public void InvocarAbejaNueva(){
		CeldaCliqueada = ObtenerCeldaAlClickear();

		if(celdasManager.CeldasDisponibles.Contains(CeldaCliqueada)){
			celdasManager.OcultarCeldasDisponiblesParaInvocar();	
			vfxManager.OcultarAbejasObjetivo();
			tropasManager.InstanciarAbeja(CeldaCliqueada);
			var bee1 = AudioManager.Instance.GameAudio.Sound5;
			AudioManager.Instance.PlaySound(bee1);
		}
		else{
			GD.Print("No se puede invocar una abeja sobre esta celda");
			EventosUI.MostrarMensaje("No se puede invocar una abeja sobre esta celda");
		}
	}

	private void OnCambioDeTurnoJugador(AbejaReina jugadorNuevo)
	{
		if (jugadorNuevo == null) return;

		if(GameManager.Instance.jugadorEnTurno.AtaquePotenciado){
			celdasManager.DespintarCeldaDeAbejaBuffeada();
		}

		if(VisualesJugadores.TryGetValue(jugadorNuevo, out Node3D visualJugador))
		{
			VisualJugadorActual = visualJugador;
			if (GameManager.Instance.CamaraActual != null && IsInstanceValid(visualJugador))
			{
				GameManager.Instance.CamaraActual.ResetearCamaraAutomaticamente(visualJugador);
			}
		}

		tropasManager.OcultarAbejasCercanas();
		animationManager.ReestablecerVelocidadAnimacion();
		attackManager.LimpiarAbejasEliminadas();
	}

	private void ManejarTurnoTerminadoPorMovimientos()
	{
		// Verificamos si el peer local es el dueño del turno actual o es el host
		if (Multiplayer.GetUniqueId() == GameManager.Instance.jugadorEnTurno.Id)
		{
			Rpc(nameof(SiguienteTurnoRpc));
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void SiguienteTurnoRpc()
	{
		_ = GameManager.Instance.TurnManager.TerminarTurno();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void SincronizarDadoRpc(int resultado)
	{
		GameManager.Instance.OtorgarMovimientos(resultado);
		GameManager.Instance.NotificarCambioDeEstado();
	}

	public void TerminarControlDeSubdito()
	{
		if(GameManager.Instance.jugadorEnTurno.MoviendoSubdito)
		{
			vfxManager.EstablecerUnOutlineAAbeja("res://outlineBase.tres", tropasManager.VisualAbejas[tropasManager.SubditoActual]);
			GameManager.Instance.CamaraActual.ResetearCamaraAutomaticamente(VisualJugadorActual);
		}
	}

	public override void _ExitTree()
	{
		if (GameManager.Instance.TurnManager != null)
		{
			GameManager.Instance.TurnManager.OnCambioDeTurnoJugador -= OnCambioDeTurnoJugador;
		}
		GameManager.Instance.OnTurnoTerminadoPorMovimientos -= ManejarTurnoTerminadoPorMovimientos;
	}
}
