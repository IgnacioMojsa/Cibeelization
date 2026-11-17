using Godot;
using System.Linq;

public partial class CamaraController : Node3D
{
	[Export] private Camera3D camera3D;
	[Export] private float paddingFactor = 1.25f; // Margen de seguridad alrededor del tablero
	[Export] private float anguloInclinacionDeg = -60.0f; 
	[Export] private float alturaZoomTurno = 4.5f;  
	[Export] private float offsetZTurno = 1.0f;     
	[Export] private float duracionAnimacion = 0.8f;
	[Export] private float velocidadRotacion = 0.07f;
	[Export] private float velocidadManualVertical = 0.2f;
	[Export] private float inclinacionMinDeg = 15.0f; // Más bajo = más cercano al tablero
	[Export] private float inclinacionMaxDeg = 75.0f;  
	[Export] private float inclinacionTurnoDeg = 30.0f; // Grados de elevación al enfocar un jugador
	[Export] private float distanciaTurno = 17.0f; // Distancia cámara-pivote al enfocar
	private float pitchActual = 66.0f;   
	private float distanciaActual = 15.0f;  
	private bool controlJugador = false;
	private Vector2 ultimaPosMouse;
	[Export] private float velocidadManual = 0.04f;
	private bool enModoOrbita = false;
	private Tween tweenCamara;
	private Node3D objetivoSeguimiento = null;
    private bool estaSiguiendo = false;
    [Export] private float velocidadSeguimiento = 6.0f;
	private int indiceJugadorInspeccionado = -1;

	public override void _Ready(){
		if (camera3D == null){
			camera3D = GetNodeOrNull<Camera3D>("Camera3D");
		}

		if (camera3D != null){
		    camera3D.Position = Vector3.Zero;
		    camera3D.RotationDegrees = Vector3.Zero;
		    camera3D.MakeCurrent();
		}

		// Registrar la cámara en el Singleton
		GameManager.Instance.CamaraActual = this;

		// EJECUCIÓN CLAVE: Si el tablero ya existe o fue asignado, forzar el ajuste al cargar la escena
		CallDeferred(nameof(AjustarAlInicio));
	}

	public override void _Process(double delta){
        if (estaSiguiendo && objetivoSeguimiento != null && IsInstanceValid(objetivoSeguimiento)){
    	    GlobalPosition = GlobalPosition.Lerp(
    	        objetivoSeguimiento.GlobalPosition,
    	        (float)(delta * velocidadSeguimiento));
    	}
    	if (!controlJugador && enModoOrbita){
    	    RotateY((float)delta * velocidadRotacion);
    	}
    }

	public override void _Input(InputEvent @event){
	    if (@event is InputEventMouseButton mouseEvent)
	    {
	        if (mouseEvent.ButtonIndex == MouseButton.Right)
	        {
	            if (mouseEvent.Pressed)
	            {
	                controlJugador = true;
	                enModoOrbita = false;
	                ultimaPosMouse = mouseEvent.Position; 
	            }
	            else
	            {
	                controlJugador = false; 
	            }
	        }
	    }
		else if (@event is InputEventMouseMotion motionEvent && controlJugador){
	        float deltaX = motionEvent.Position.X - ultimaPosMouse.X;
			float deltaY = motionEvent.Position.Y - ultimaPosMouse.Y;
	        ultimaPosMouse = motionEvent.Position;

	        RotateY(deltaX * velocidadManual * - 1f);
			RotarVertical(deltaY);

		} 
		else if (GameManager.Instance.modoMultiplayer && !EsTurnoLocal()){
	        //hay que agregar botones también, así no depende del input del teclado solamente 
			if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo){
	            if (keyEvent.Keycode == Key.Right || keyEvent.Keycode == Key.D){
	                EnfocarJugadorSiguiente();
	                GetViewport().SetInputAsHandled();
	            } 
				else if (keyEvent.Keycode == Key.Left || keyEvent.Keycode == Key.A){
	                EnfocarJugadorPrevio();
	                GetViewport().SetInputAsHandled();
	            }
	        }
	    }
	}

	private bool EsTurnoLocal(){
		if (!GameManager.Instance.modoMultiplayer) return true;
		var jugador = GameManager.Instance.jugadorEnTurno;
		if (jugador == null) return true;
		return Multiplayer.GetUniqueId() == jugador.Id;
	}

	private void EnfocarJugadorLocal(){
		if (!GameManager.Instance.modoMultiplayer) return;
		long localId = Multiplayer.GetUniqueId();
		var jugadorLocal = GameManager.Instance.JugadoresEnPartida.Find(j => j.Id == localId);
		var playerManager = GetTree().CurrentScene.GetNodeOrNull<PlayerManager>("PlayerManager");
		
		if (jugadorLocal != null && playerManager != null && playerManager.VisualesJugadores.TryGetValue(jugadorLocal, out Node3D visualLocal)){
			indiceJugadorInspeccionado = GameManager.Instance.JugadoresEnPartida.IndexOf(jugadorLocal);
			EnfocarNodo(visualLocal, 1.0f, 1.0f);
		}
	}

	private void EnfocarJugadorSiguiente(){
		var jugadoresVivos = GameManager.Instance.JugadoresEnPartida.Where(j => !j.FueraDeJuego).ToList();
		if (jugadoresVivos.Count <= 1) return;

		var jugadorActualInspeccionado = GameManager.Instance.JugadoresEnPartida.ElementAtOrDefault(indiceJugadorInspeccionado);
		int idxActualVivos = jugadoresVivos.IndexOf(jugadorActualInspeccionado);

		if (idxActualVivos == -1) idxActualVivos = 0;

		idxActualVivos = (idxActualVivos + 1) % jugadoresVivos.Count;
		var siguienteJugador = jugadoresVivos[idxActualVivos];
		indiceJugadorInspeccionado = GameManager.Instance.JugadoresEnPartida.IndexOf(siguienteJugador);

		var playerManager = GetTree().CurrentScene.GetNodeOrNull<PlayerManager>("PlayerManager");
		if (playerManager != null && playerManager.VisualesJugadores.TryGetValue(siguienteJugador, out Node3D visual)){
			EnfocarNodo(visual, 1.0f, 1.0f);
		}
	}

	private void EnfocarJugadorPrevio(){
		var jugadoresVivos = GameManager.Instance.JugadoresEnPartida.Where(j => !j.FueraDeJuego).ToList();
		if (jugadoresVivos.Count <= 1) return;

		var jugadorActualInspeccionado = GameManager.Instance.JugadoresEnPartida.ElementAtOrDefault(indiceJugadorInspeccionado);
		int idxActualVivos = jugadoresVivos.IndexOf(jugadorActualInspeccionado);

		if (idxActualVivos == -1) idxActualVivos = 0;

		idxActualVivos = (idxActualVivos - 1 + jugadoresVivos.Count) % jugadoresVivos.Count;
		var anteriorJugador = jugadoresVivos[idxActualVivos];
		indiceJugadorInspeccionado = GameManager.Instance.JugadoresEnPartida.IndexOf(anteriorJugador);

		var playerManager = GetTree().CurrentScene.GetNodeOrNull<PlayerManager>("PlayerManager");
		if (playerManager != null && playerManager.VisualesJugadores.TryGetValue(anteriorJugador, out Node3D visual)){
			EnfocarNodo(visual, 1.0f, 1.0f);
		}
	}

	private void AjustarAlInicio(){
		if (GameManager.Instance.TableroActual != null){
			AjustarATablero(
				GameManager.Instance.DimensionActual, 
				GameManager.Instance.DimensionActual, 
				GameManager.Instance.TableroActual.TileSize
			);
		}
	}

	public void AjustarATablero(int filas, int columnas, float tileSize) {
		if (camera3D == null) return;

		enModoOrbita = false;
		estaSiguiendo = false;
	
		float anchoTablero = columnas * (tileSize * 1.732f); 
		float profundidadTablero = filas * (tileSize * 1.5f);
		float valorFloat;

		if(filas == 15) {
			valorFloat = 40.0f;
		}
		else if(filas == 25) {
			valorFloat = 2.5f; 
		}
		else if(filas == 30) {
			valorFloat = 2.0f;
		} else {
			valorFloat = 40.0f;
		}

		Vector3 centroTablero = new Vector3(anchoTablero / 2.0f, 0f, profundidadTablero / valorFloat);

		float dimensionMaxima = Mathf.Max(anchoTablero, profundidadTablero);
	
		// 4. Calcular la distancia (altura) en base al FOV de la cámara
		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		float aspectRatio = viewportSize.X / viewportSize.Y;
	
		float fovRad = Mathf.DegToRad(camera3D.Fov);
		// Ajuste trigonométrico de distancia
		float distanciaRequerida = (dimensionMaxima * paddingFactor / 2.0f) / Mathf.Tan(fovRad / 2.0f);
	
		// Ajustar si la pantalla es más alta que ancha
		if (aspectRatio < 1.0f){
			distanciaRequerida /= aspectRatio;
		}
	
		// 5. Posicionar el pivote en el centro exacto del tablero
		Position = centroTablero;
	
		pitchActual = Mathf.Abs(anguloInclinacionDeg);
		distanciaActual = distanciaRequerida;
		AplicarCamara();
	}

	public void EnfocarNodo(Node3D objetivo, float multiplicadorPivote, float multiplicadorCamara){
		if (objetivo == null || camera3D == null) return;

		estaSiguiendo = false;
        enModoOrbita = false;

		if (tweenCamara != null && tweenCamara.IsValid()){
			tweenCamara.Kill();
		}

		tweenCamara = CreateTween().SetParallel(true).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);

		Node3D pivoteObjetivo = objetivo.GetNodeOrNull<Node3D>("Pivote") ?? objetivo;
		Vector3 posicionObjetivoPivote = pivoteObjetivo.GlobalPosition;

		tweenCamara.TweenMethod(Callable.From<float>(v => { pitchActual = v; AplicarCamara(); }),
    	pitchActual, inclinacionTurnoDeg, duracionAnimacion * multiplicadorCamara);

		tweenCamara.TweenMethod(Callable.From<float>(v => { distanciaActual = v; AplicarCamara(); }),
    	distanciaActual, distanciaTurno, duracionAnimacion * multiplicadorCamara);

		tweenCamara.TweenProperty(this, "global_position", posicionObjetivoPivote, duracionAnimacion*multiplicadorPivote);

		tweenCamara.Chain().TweenCallback(Callable.From(() => 
		{
			objetivoSeguimiento = pivoteObjetivo;
            estaSiguiendo = true;
            enModoOrbita = true;
		}));

		GD.Print($"EnfocarNodo -> {objetivo?.Name} pos={objetivo?.GlobalPosition}");
	}

	public void ResetearCamaraAutomaticamente(Node3D jugadorActual){
	    controlJugador = false;
        enModoOrbita = false;
        estaSiguiendo = false;

	    EnfocarNodo(jugadorActual, 1.0f, 1.0f); // vuelve a la lógica automática
	}

	private void AplicarCamara(){
		if (camera3D == null) return;
		float rad = Mathf.DegToRad(pitchActual);
		camera3D.Position = new Vector3(0f, distanciaActual * Mathf.Sin(rad), distanciaActual * Mathf.Cos(rad));
		camera3D.RotationDegrees = new Vector3(-pitchActual, 0f, 0f);
	}

	private void RotarVertical(float deltaY){
    	if (camera3D == null) return;
    	if (tweenCamara != null && tweenCamara.IsRunning()) return;

    	pitchActual = Mathf.Clamp(pitchActual + deltaY * velocidadManualVertical, inclinacionMinDeg, inclinacionMaxDeg);
    	AplicarCamara();
	}
}
