using Godot;

public partial class GameUI : UIBase {
	
	[Export] public TroopsManager tropasManager;
	[Export] public CeldasManager celdasManager; 
	[Export] public AttackManager attackManager;
	[Export] public HudSuperior hudSuperior;
	[Export] public HudInferior hudInferior;
	[Export] public PanelContainer panelFueraDeJuego;
	[Export] public PlayerManager playerManager;
	public PanelContainer containerPausa;
	public HBoxContainer uiPausa;
	public bool hayJugadorVictorioso = false;
	public bool reinicioConfirmado = false;
	public Button botonContinuar;
	public Button botonReiniciarPartida;
	public Button botonMenuPrincipal;
	public Button botonTutorial;
	public PanelContainer confirmacionSalir;
	public PanelContainer confirmacionReiniciar;
	public PanelContainer confirmacionSalirVictoria;
	public PanelContainer confirmacionReiniciarVictoria;
	public HBoxContainer MenuVictoria;
	public Button botonSalirVictoria;
	public Button botonReiniciarPartidaVictoria;
	private bool panelFueraDeJuegoMostrado = false;

	public override void _Ready(){
		ProcessMode = ProcessModeEnum.Always;
		
		ActualizarConfiguracionesGraficas();
		
		string currentScene = GetTree().CurrentScene.SceneFilePath;

		if(currentScene == "res://Scenes/escenaPrueba.tscn"){
			ConfigurarEscenaDeJuego();
		}
		else if(currentScene == "res://Scenes/pantallaVictoria.tscn"){
			MostrarPantallaDeVictoria();
			AudioManager.Instance.PlaySound(UiSound7);
		}
		else if(currentScene == "res://Scenes/pantallaEmpate.tscn"){
			MostrarPantallaDeEmpate();
			AudioManager.Instance.PlaySound(UiSound10);
		}

		GameManager.Instance.OnJugadorEliminadoPorDesconexion += EliminarVisualJugadorDesconectado;
	}

	private void OnPeerDisconnectedInGame(long id){
		GD.Print($"Un jugador se desconectó en plena partida: {id}");
		var jugador = GameManager.Instance.JugadoresEnPartida.Find(j => j.Id == id);
		if (jugador != null){
			jugador.HP = 0;
			jugador.FueraDeJuego = true;

			if (playerManager != null && playerManager.VisualesJugadores.TryGetValue(jugador, out Node3D visualJugador)){
				int indiceJugador = GameManager.Instance.JugadoresEnPartida.IndexOf(jugador);
				
				if (attackManager != null){
					attackManager.EliminarInstanciaDeJugador(visualJugador, indiceJugador);
				}
			}
		}
	}

	private void ConfigurarEscenaDeJuego(){
		var root = GetTree().CurrentScene; 
		GameManager.Instance.TableroActual = root.GetNodeOrNull<Tablero>("Tablero");
		if (GameManager.Instance.TableroActual == null)
			GD.PrintErr("No se encontró Tablero en la escena");

		GameManager.Instance.CamaraActual = root.GetNodeOrNull<CamaraController>("Camara");
		playerManager ??= root.GetNodeOrNull<PlayerManager>("PlayerManager");
		tropasManager ??= root.GetNodeOrNull<TroopsManager>("TroopsManager");
		celdasManager ??= root.GetNodeOrNull<CeldasManager>("CeldasManager");
		attackManager ??= root.GetNodeOrNull<AttackManager>("AttackManager");
		hudSuperior ??= GetNodeOrNull<HudSuperior>("HudSuperior");
		hudInferior ??= GetNodeOrNull<HudInferior>("HudInferior");

		GameManager.Instance.SetTiles(GameManager.Instance.sizeTablero);

		InicializarGameUI();
		hudSuperior?.Inicializar(playerManager);
		hudInferior?.Inicializar(this, playerManager, tropasManager, celdasManager, attackManager);
		
		SuscribirAEventos();
		hudSuperior?.MostrarDataDeJugadores();
		ActualizarUI();
	}
	
	private void OnCambioDeTurno(AbejaReina nuevoJugador){
		if (nuevoJugador == null) return;
		
		var anterior = GameManager.Instance.jugadorEnTurno;
		GameManager.Instance.jugadorEnTurno = nuevoJugador;

		foreach (var j in GameManager.Instance.JugadoresEnPartida){
			if (j == null) continue;
			j.EsSuTurno = (j == nuevoJugador);
			j.AtaquePotenciado = false;
		}

		nuevoJugador.AccionConsumida = false;
		nuevoJugador.TiroLosDados = false;
		nuevoJugador.ModoAtaque = false;
		nuevoJugador.AtaqueConsumido = false;
		nuevoJugador.AtaqueAObjetivo = false;
		nuevoJugador.ModoInvocacion = false;
		nuevoJugador.ModoAbsorcion = false;
		nuevoJugador.ModoTransformacion = false;
		nuevoJugador.MenteColmena = false;
		nuevoJugador.MoviendoSubdito = false;
		nuevoJugador.AtacoRecien = false;
		nuevoJugador.InvocoRecien = false;
		nuevoJugador.MovimientosDisponibles = 0;
		nuevoJugador.Estado = AbejaReina.EstadoTurno.EsperandoDado;

		if (playerManager != null && playerManager.VisualesJugadores.TryGetValue(nuevoJugador, out Node3D visualJugador)){
			playerManager.VisualJugadorActual = visualJugador;
			if (GameManager.Instance.CamaraActual != null && IsInstanceValid(visualJugador)){
				GameManager.Instance.CamaraActual.ResetearCamaraAutomaticamente(visualJugador);
			}
		}

		ActualizarUI();
	}
	
	private void InicializarGameUI(){
		uiPausa = GetNode<HBoxContainer>("MenuPausa");
		confirmacionSalir = GetNode<PanelContainer>("ConfirmacionSalir");
		confirmacionReiniciar = GetNode<PanelContainer>("ConfirmacionReiniciar");
		panelFueraDeJuego = GetNode<PanelContainer>("PanelFueraDeJuego");
		var tutorial = GetNodeOrNull<TutorialUI>("GuiaTutorial");

		botonContinuar = GetNode<Button>("MenuPausa/PausaBorder/MarginContainer/VBoxContainer/Continuar/ContinuarButton");
		botonMenuPrincipal = GetNode<Button>("MenuPausa/PausaBorder/MarginContainer/VBoxContainer/MenuPrincipal/MenuPrincipalButton");
		botonReiniciarPartida = GetNode<Button>("MenuPausa/PausaBorder/MarginContainer/VBoxContainer/Reiniciar/ReiniciarButton");
		botonTutorial = GetNodeOrNull<Button>("MenuPausa/PausaBorder/MarginContainer/VBoxContainer/Tutorial/TutorialBoton");

		var contenedorReiniciarPausa = GetNodeOrNull<Control>("MenuPausa/PausaBorder/MarginContainer/VBoxContainer/Reiniciar");
		if (contenedorReiniciarPausa != null){
			contenedorReiniciarPausa.Visible = !GameManager.Instance.modoMultiplayer;
		}

		MostrarTutorial(tutorial);

		botonContinuar.Pressed += AlternarPausa;
		botonMenuPrincipal.Pressed += () => CambiarMenu(uiPausa, confirmacionSalir);
		botonReiniciarPartida.Pressed += () => CambiarMenu(uiPausa, confirmacionReiniciar);

		GetNode<Button>("ConfirmacionSalir/MarginContainer/VBoxContainer/HBoxContainer/Si/SiButton").Pressed += IrAlMenuPrincipal;
		GetNode<Button>("ConfirmacionSalir/MarginContainer/VBoxContainer/HBoxContainer/No/NoButton").Pressed += () => CambiarMenu(confirmacionSalir, uiPausa);
		GetNode<Button>("ConfirmacionReiniciar/MarginContainer/VBoxContainer/HBoxContainer/Si/SiButton").Pressed += Reiniciar;
		GetNode<Button>("ConfirmacionReiniciar/MarginContainer/VBoxContainer/HBoxContainer/No/NoButton").Pressed += () => CambiarMenu(confirmacionReiniciar, uiPausa);
	}

	private void SuscribirAEventos(){
		//_ExitTree();

		var turnManager = GameManager.Instance.TurnManager;
		EventosUI.OnMensajeAMostrar -= hudInferior.ActualizarInstrucciones;
		EventosUI.OnMensajeAMostrar += hudInferior.ActualizarInstrucciones;

		if (turnManager != null){
			turnManager.OnCambioDeTurnoJugador -= OnCambioDeTurno;
			turnManager.OnCambioDeTurnoJugador += OnCambioDeTurno;
			turnManager.OnTurnoCambiado -= ActualizarUI;
			turnManager.OnTurnoCambiado += ActualizarUI;
		} else {
			GD.PrintErr($"[Turnos][Peer {Multiplayer.GetUniqueId()}] TurnManager es null en GameUI.SuscribirAEventos().");
		}

		hudSuperior.OnPausaAlternada -= AlternarPausa;
		hudSuperior.OnPausaAlternada += AlternarPausa;
		hudInferior.OnUIActualizacionRequerida -= ActualizarUI;
		hudInferior.OnUIActualizacionRequerida += ActualizarUI;

		GameManager.Instance.OnEstadoAccionesCambiado -= hudInferior.AlternarEstadoDeAtaque;
		GameManager.Instance.OnEstadoAccionesCambiado -= hudInferior.AlternarEstadoDeAbsorcion;
		GameManager.Instance.OnEstadoAccionesCambiado -= hudInferior.AlternarEstadoDeTransformacion;
		GameManager.Instance.OnEstadoAccionesCambiado -= ActualizarUI;

		GameManager.Instance.OnEstadoAccionesCambiado += hudInferior.AlternarEstadoDeAtaque;
		GameManager.Instance.OnEstadoAccionesCambiado += hudInferior.AlternarEstadoDeAbsorcion;
		GameManager.Instance.OnEstadoAccionesCambiado += hudInferior.AlternarEstadoDeTransformacion;
		GameManager.Instance.OnEstadoAccionesCambiado += ActualizarUI;
	}

	public void ActualizarUI(){
		if (hudSuperior != null){
				hudSuperior.MostrarJugadorEnTurno();
				hudSuperior.MostrarHPDeJugaores();
				hudSuperior.MostrarRecursosDeJugadorEnTurno();
		}

		if (hudInferior != null){
				hudInferior.DeshabilitarDado();
				hudInferior.AlternarEstadoDeAtaque();
				hudInferior.AlternarEstadoDeAbsorcion();
				hudInferior.AlternarEstadoDeTransformacion();
				hudInferior.MostrarMovimientosDisponibles();
				hudInferior.ValidarTurnoLocalUI();
				hudInferior.ActualizarUIAtaques();
				//AlternarComandoDeAtaque();
		}
		
		VerificarJugadorFueraDeJuego();

		var instanciaMenuRadial = hudInferior?.instanciaMenuRadial;
		if (instanciaMenuRadial != null && instanciaMenuRadial.ContenedorDeTransformaciones != null){
			if(instanciaMenuRadial.ContenedorDeTransformaciones.Visible){
				instanciaMenuRadial.OcultarTransformaciones();
			} else {
				instanciaMenuRadial.OcultarMenu();
			}
		}
	}

	public void AlternarPausa(){
		bool mostrarMenu = !uiPausa.Visible;
		uiPausa.Visible = mostrarMenu;

		if(!GameManager.Instance.modoMultiplayer){
			GetTree().Paused = mostrarMenu;
		}

		if(mostrarMenu){
			GD.Print("El juego se pausó en modo Online");
			MostrarUIPanel(hudSuperior.containerPausa, false);
		} else {
			GD.Print("El juego se reanudó en modo Online");
			MostrarUIPanel(hudSuperior.containerPausa, true);
		}
	}

	private void OcultarConfirmacionSalir() => CambiarMenu(confirmacionSalir, uiPausa);

	private void Reiniciar(){
		GetTree().Paused = false;

		GameManager.Instance.ReiniciarPartida();

		reinicioConfirmado = true;
		hayJugadorVictorioso = false;

		if (confirmacionReiniciar != null) confirmacionReiniciar.Visible = false;
		if (confirmacionReiniciarVictoria != null) confirmacionReiniciarVictoria.Visible = false;
		if (hudSuperior.containerPausa != null) hudSuperior.containerPausa.Visible = true;

		GD.Print("Comienza nueva partida");
		GetTree().ChangeSceneToFile("res://Scenes/escenaPrueba.tscn");
	}

	private void IrAlMenuPrincipal(){
		GetTree().Paused = false;
		ForzarDesconexionYLimpiezaTotal();
		if (confirmacionSalir != null) confirmacionSalir.Visible = false;
		if (confirmacionSalirVictoria != null) confirmacionSalirVictoria.Visible = false;
		if (hudSuperior.containerPausa != null) hudSuperior.containerPausa.Visible = true;
		
		GD.Print("Regresando al menú principal");

		reinicioConfirmado = false;
		hayJugadorVictorioso = false;
	}

	public void ForzarDesconexionYLimpiezaTotal(){
		if (Multiplayer.MultiplayerPeer != null){
			Multiplayer.MultiplayerPeer.Close();
			Multiplayer.MultiplayerPeer = null;
		}
		GameManager.Instance.ResetearEstadoPartida();
		if (GetTree().CurrentScene.SceneFilePath != "res://Scenes/SplashScreen.tscn"){
			GetTree().ChangeSceneToFile("res://Scenes/SplashScreen.tscn");
		}
	}

	private void MostrarTutorial(TutorialUI tutorial){
		if(tutorial != null){
			if(tutorial.SeMuestraAlComenzarPartida()){
				tutorial.AbrirTutorial(TutorialUI.ModoTutorial.InicioPartida);
			} else {
				tutorial.Visible = false;
			}

			if(botonTutorial != null){
				bool abiertoDesdePausa = false;

				botonTutorial.Pressed += () =>
				{
					abiertoDesdePausa = true;
					uiPausa.Visible = false;
					tutorial.AbrirTutorial(TutorialUI.ModoTutorial.MenuPausa);
				};

				tutorial.OnTutorialCerrado += () =>
				{
					if(abiertoDesdePausa)
					{
						abiertoDesdePausa = false;
						uiPausa.Visible = true;
					}
				};
			}
		}
	}

	private void Ajustes(){
		PanelContainer UIAjustes = GetNode<PanelContainer>("Ajustes");
		MostrarUIPanel(UIAjustes, true);
	}

	private void AceptarConfigAudio(){
		PanelContainer UIAjustesMenuPrincipal = GetNode<PanelContainer>("Ajustes");
		MostrarUIPanel(UIAjustesMenuPrincipal, false);
	}

	private void MostrarPantallaDeVictoria(){
		hayJugadorVictorioso = true;
		MostrarMensajeVictoria();
		UIVictoria();
	}

	private void MostrarMensajeVictoria(){
		var mensajeVictoria = GetNode<Label>("Titulo");
		mensajeVictoria.Text = "EL  JUGADOR   " + GameManager.Instance.JugadorGanador.Id + "   ES  EL  GANADOR";
	}

	private void MostrarPantallaDeEmpate(){
		var mensajeEmpate = GetNode<Label>("Titulo");
		mensajeEmpate.Text = "Empate";
		UIVictoria();
	}

	public bool EsTurnoLocal(){
		if (!GameManager.Instance.modoMultiplayer) return true;
		var jugador = GameManager.Instance.jugadorEnTurno;
		if (jugador == null) return false;
		return Multiplayer.GetUniqueId() == jugador.Id;
	}	
	
	private void UIVictoria(){
		MenuVictoria = GetNode<HBoxContainer>("MenuVictoria");
		botonSalirVictoria = GetNode<Button>("MenuVictoria/VictoriaBorder/MarginContainer/VBoxContainer/MenuPrincipal/MenuPrincipalButton");
		botonReiniciarPartidaVictoria = GetNode<Button>("MenuVictoria/VictoriaBorder/MarginContainer/VBoxContainer/Reiniciar/ReiniciarButton");
		confirmacionReiniciarVictoria = GetNode<PanelContainer>("ConfirmacionReiniciarVictoria");
		confirmacionSalirVictoria = GetNode<PanelContainer>("ConfirmacionSalirVictoria");

		botonSalirVictoria.Pressed += () => CambiarMenu(MenuVictoria, confirmacionSalirVictoria);
		botonReiniciarPartidaVictoria.Pressed += () => CambiarMenu(MenuVictoria, confirmacionReiniciarVictoria);

		var contenedorReiniciarVictoria = GetNodeOrNull<Control>("MenuVictoria/VictoriaBorder/MarginContainer/VBoxContainer/Reiniciar");
		if (contenedorReiniciarVictoria != null){
			contenedorReiniciarVictoria.Visible = !GameManager.Instance.modoMultiplayer;
		}

		GetNode<Button>("ConfirmacionSalirVictoria/MarginContainer/VBoxContainer/HBoxContainer/Si/SiButton").Pressed += IrAlMenuPrincipal;
		GetNode<Button>("ConfirmacionSalirVictoria/MarginContainer/VBoxContainer/HBoxContainer/No/NoButton").Pressed += () => CambiarMenu(confirmacionSalirVictoria, MenuVictoria);
		GetNode<Button>("ConfirmacionReiniciarVictoria/MarginContainer/VBoxContainer/HBoxContainer/Si/SiButton").Pressed += Reiniciar;
		GetNode<Button>("ConfirmacionReiniciarVictoria/MarginContainer/VBoxContainer/HBoxContainer/No/NoButton").Pressed += () => CambiarMenu(confirmacionReiniciarVictoria, MenuVictoria);
	}

	private void VerificarJugadorFueraDeJuego(){
		if (!GameManager.Instance.modoMultiplayer) return;
		long idLocal = Multiplayer.GetUniqueId();
		var jugadorLocal = GameManager.Instance.JugadoresEnPartida.Find(j => j.Id == idLocal);

		if (jugadorLocal != null && jugadorLocal.FueraDeJuego && !panelFueraDeJuegoMostrado){
			panelFueraDeJuegoMostrado = true;
			MostrarPanelFueraDeJuego();
		}
	}

	private void MostrarPanelFueraDeJuego(){
		MostrarUIPanel(panelFueraDeJuego, true);
		AudioManager.Instance.PlaySound(UiSound1);
	}

	private void OcultarPanelFueraDeJuego(){
		MostrarUIPanel(panelFueraDeJuego, false);
		AudioManager.Instance.PlaySound(UiSound1);
	}

	private void EliminarVisualJugadorDesconectado(AbejaReina jugador){
		GD.Print("Se elinimó el visual del jugador porque ya pasó el tiempo de reconexión");
		if(playerManager != null && playerManager.VisualesJugadores.TryGetValue(jugador, out Node3D visualJugador)){
			int indiceJugador = GameManager.Instance.JugadoresEnPartida.IndexOf(jugador);
			if(attackManager != null){
				attackManager.EliminarInstanciaDeJugador(visualJugador, indiceJugador);
			}
		}
		ActualizarUI();
	}

	public override void _ExitTree(){
		if(hudSuperior != null) hudSuperior.OnPausaAlternada -= AlternarPausa;

		if(hudInferior != null){
			EventosUI.OnMensajeAMostrar -= hudInferior.ActualizarInstrucciones;
			hudInferior.OnUIActualizacionRequerida -= ActualizarUI;
		}

		if(GameManager.Instance != null){
			GameManager.Instance.OnJugadorEliminadoPorDesconexion -= EliminarVisualJugadorDesconectado;

			if(hudInferior != null){
				GameManager.Instance.OnEstadoAccionesCambiado -= hudInferior.AlternarEstadoDeAtaque;
				GameManager.Instance.OnEstadoAccionesCambiado -= hudInferior.AlternarEstadoDeAbsorcion;
				GameManager.Instance.OnEstadoAccionesCambiado -= hudInferior.AlternarEstadoDeTransformacion;
			}

			GameManager.Instance.OnEstadoAccionesCambiado -= ActualizarUI;

			if(GameManager.Instance.TurnManager != null){
				var turnManager = GameManager.Instance.TurnManager;
				turnManager.OnCambioDeTurnoJugador -= OnCambioDeTurno;
				turnManager.OnTurnoCambiado -= ActualizarUI;
			}
		}
		base._ExitTree();
	}
};
