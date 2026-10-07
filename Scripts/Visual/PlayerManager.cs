using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class PlayerManager : Node3D
{
	[Export] private Camera3D camera;
	[Export] public Tablero tablero; 
	[Export] public TroopsManager tropasManager; 
	[Export] public CeldasManager celdasManager; 
	[Export] public AnimationManager animationManager;
	[Export] public VFXManager vfxManager;

	public readonly List<Node3D> VisualesJugadores = new();
	private readonly List<PackedScene> Assets = new();
	public Dictionary<Node3D, Celda> CeldaActualPorJugador = new();
	public List<Abeja> AbejasObjetivo = new();

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
			Atacar();
		}

		else if(jugador.ModoAbsorcion && !jugador.AccionConsumida){
			tropasManager.AbsorberSubdito(CeldaCliqueada);
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
		
		RotarJugadorHex(VisualJugadorActual, CeldaCliqueada);

		EstablecerCeldaParaJugadorEnTurno();

		if(PuedeMoverseEntre(CeldaOrigen, CeldaCliqueada, CeldaActualPorJugador, VisualJugadorActual, VisualesJugadores))
		{
			vfxManager.OcultarAbejasObjetivo();
			MoverAbejaACelda(VisualJugadorActual, CeldaCliqueada);

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
		
		Node nodoCollider = result["collider"].As<GodotObject>() as Node;
		Abeja abejaCliqueada = ObtenerAbejaDesdeCollider(nodoCollider);

		if(abejaCliqueada != null){
			CeldaCliqueada = abejaCliqueada.CeldaActual;
			RotarJugadorHex(VisualJugadorActual, CeldaCliqueada);
			if (AbejasObjetivo.Contains(abejaCliqueada)){
				AtacarAbejaEspecifica(abejaCliqueada);
				return;
			}
		}

		CeldaCliqueada = ObtenerCeldaDesdePosicion(tablero.Celdas, PosicionEnMundo3D);
		if (CeldaCliqueada == null)
		return;

		RotarJugadorHex(VisualJugadorActual, CeldaCliqueada);

		if(OtroJugadorCerca())
		{
			AtacarJugador();
		}
		else if(AbejasObjetivo.Count > 0)
		{
			AtacarAbeja();
		}
		
		vfxManager.OcultarAbejasObjetivo();
	}

	private Abeja ObtenerAbejaDesdeCollider(Node collider)
	{
		if(collider == null || tropasManager == null || tropasManager.VisualAbejas == null)
		return null;

		Node actual = collider;

		while(actual != null && actual != GetTree().Root){
			if(actual is Node3D node3d){
				var par = tropasManager.VisualAbejas.FirstOrDefault(kvp => kvp.Value == node3d);
				if(par.Key != null){
					return par.Key;
				}
			}
			actual = actual.GetParent();
		}
		return null;
	}

	private void AtacarAbejaEspecifica(Abeja abejaObjetivo)
	{
		ataqueManager.DaniarAbeja(abejaObjetivo);
		if(tropasManager.VisualAbejas.TryGetValue(abejaObjetivo, out Node3D visual) && IsInstanceValid(visual)){
			celdasManager.DespintarCeldaDeAbeja(abejaObjetivo.CeldaActual);
			visual.QueueFree();
		}
		LimpiarAbejasEliminadas();
		GameManager.Instance.ConsumirAtaque();
	}

	public bool OtroJugadorCerca()
	{
		return VisualesJugadores.Any(v => JugadorEnTurnoAdyacenteAOtro(v));
	}

	public bool TieneObjetivosCerca(){	
		ActualizarAbejasObjetivo();
    	return AbejasObjetivo.Count > 0 || OtroJugadorCerca();
	}

	private void AtacarAbeja(){
		var abejaObjetivo = AbejasObjetivo.Find(a => a.CeldaActual == CeldaCliqueada);

		if(abejaObjetivo == null){
			GD.Print("No hay ninguna abeja objetivo en la celda seleccionada.");
			EventosUI.MostrarMensaje("No hay ninguna abeja enemiga para atacar ahí.");
			return;
		}

		ataqueManager.DaniarAbeja(abejaObjetivo);
		
		if (tropasManager.VisualAbejas[abejaObjetivo] != null && IsInstanceValid(tropasManager.VisualAbejas[abejaObjetivo]))
		{
			celdasManager.DespintarCeldaDeAbeja(abejaObjetivo.CeldaActual);
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
				animationManager.CambiarAnimacionDeJugador(reinaObjetivo, "RecibirDanio");

				GameManager.Instance.ConsumirAtaque();
			}
			else
			{
				GD.Print("La Abeja Reina rival está demasiado lejos para ser atacada.");
				EventosUI.MostrarMensaje("Esa abeja enemiga está muy lejos.");
			}
		}
		else
		{
			GD.Print("No hay ninguna Abeja Reina rival en la celda seleccionada.");
			EventosUI.MostrarMensaje("No hay ninguna abeja enemiga para atacar ahí.");
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

		animationManager.GuardarInstanciasDeJugadores(VisualesJugadores);
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

	public void BuscarAbejasObjetivo(AbejaReina jugadorRival, List<Abeja> listaDeAbejas){
		var jugadorActual = GameManager.Instance.jugadorEnTurno;
		
		foreach (var abejaActual in jugadorRival.ColmenaDeReina.AbejasDeColmena)
		{
			if (celdasManager.CeldasDisponibles.Contains(abejaActual.CeldaActual) 
			&& movimientoManager.CeldasSonAdyacentes(jugadorActual.UbicacionActual, abejaActual.CeldaActual) 
			&& !abejaActual.FueraDeJuego
			&& !jugadorActual.ColmenaDeReina.AbejasDeColmena.Contains(abejaActual))
			{
				listaDeAbejas.Add(abejaActual);
			}
		}
	}

	public void ActualizarAbejasObjetivo()
	{
    	AbejasObjetivo.Clear();
    	VisualJugadorActual = VisualesJugadores[GameManager.Instance.jugadorEnTurno.Id - 1];
    	celdasManager.CeldasDisponibles = tablero.ObtenerVecinos(CeldaActualPorJugador[VisualJugadorActual]);

    	foreach (var jugador in GameManager.Instance.JugadoresEnPartida)
    	{
    	    if (jugador == GameManager.Instance.jugadorEnTurno || jugador.FueraDeJuego) continue;
    	    BuscarAbejasObjetivo(jugador, AbejasObjetivo);
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

		Node3D visualJugador = VisualesJugadores[jugadorNuevo.Id - 1]; 

		if (GameManager.Instance.CamaraActual != null && IsInstanceValid(visualJugador))
		{
			GameManager.Instance.CamaraActual.ResetearCamaraAutomaticamente(visualJugador);
		}

		animationManager.ReestablecerVelocidadAnimacion();
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
