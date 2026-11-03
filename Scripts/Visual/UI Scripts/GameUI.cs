using Godot;
using System.Linq;

public partial class GameUI : UIJugadores {
	[Export] public TroopsManager tropasManager;
	[Export] public CeldasManager celdasManager; 
	[Export] public AttackManager attackManager;
	[Export] private Label feedback;
	[Export] private PackedScene menuRadialPrefab;
	private MenuRadial instanciaMenuRadial;
	private Button botonPausa;
	private TextureButton botonColmena;
	private PanelContainer containerPausa;
	private HBoxContainer uiPausa;
	private Label resultadoDados;
	public AudioSetting UiSound1 => AudioManager.Instance?.GameAudio?.Sound1;
	public AudioSetting UiSound4 => AudioManager.Instance?.GameAudio?.Sound4;
	private TextureButton botonDado;
	private TextureButton botonAtacar;
	public bool hayJugadorVictorioso = false;
	public bool reinicioConfirmado = false;
	private Button botonContinuar;
	private Button botonReiniciarPartida;
	private Button botonMenuPrincipal;
	private PanelContainer confirmacionSalir;
	private PanelContainer confirmacionReiniciar;
	private PanelContainer confirmacionSalirVictoria;
	private PanelContainer confirmacionReiniciarVictoria;
	private HBoxContainer MenuVictoria;
	private Button botonSalirVictoria;
	private Button botonReiniciarPartidaVictoria;
	
	public override void _Ready(){
		ProcessMode = ProcessModeEnum.Always;
		
		string currentScene = GetTree().CurrentScene.SceneFilePath;

		if(currentScene == "res://Scenes/escenaPrueba.tscn"){
			ConfigurarEscenaDeJuego();
		}
		else if(currentScene == "res://Scenes/pantallaVictoria.tscn"){
			MostrarPantallaDeVictoria();
		}
	}

	private void ConfigurarEscenaDeJuego(){
		var root = GetTree().CurrentScene; 
		GameManager.Instance.TableroActual = root.GetNodeOrNull<Tablero>("Tablero");
		if (GameManager.Instance.TableroActual == null)
			GD.PrintErr("No se encontró Tablero en la escena");

		GameManager.Instance.CamaraActual = root.GetNode<CamaraController>("Camara");
		playerManager = root.GetNode<PlayerManager>("PlayerManager");
		GameManager.Instance.SetTiles(GameManager.Instance.sizeTablero);
		
		InicializarGameUI();
		SuscribirAEventos();
		MostrarDataDeJugadores();
		ActualizarUI();
	}
	
	private void OnCambioDeTurno(object _) => ActualizarUI();
	
	public void InicializarGameUI(){
		botonPausa = GetNode<Button>("Pausa/PausaButton");
		containerPausa = GetNode<PanelContainer>("Pausa");
		uiPausa = GetNode<HBoxContainer>("MenuPausa");
		confirmacionSalir = GetNode<PanelContainer>("ConfirmacionSalir");
		confirmacionReiniciar = GetNode<PanelContainer>("ConfirmacionReiniciar");

		resultadoDados = GetNode<Label>("Dado/NumeroDado");
		recursos = GetNode<Control>("CantRecursos");
		feedback = GetNode<Label>("Feedback");
		botonDado = GetNode<TextureButton>("Dado/DadoButton");
		botonAtacar = GetNode<TextureButton>("Atacar/AtacarButton");
		botonColmena = GetNode<TextureButton>("Colmena/ColmenaButton");

		botonContinuar = GetNode<Button>("MenuPausa/PausaBorder/MarginContainer/VBoxContainer/Continuar/ContinuarButton");
		botonMenuPrincipal = GetNode<Button>("MenuPausa/PausaBorder/MarginContainer/VBoxContainer/MenuPrincipal/MenuPrincipalButton");
		botonReiniciarPartida = GetNode<Button>("MenuPausa/PausaBorder/MarginContainer/VBoxContainer/Reiniciar/ReiniciarButton");
		botonContinuar.Pressed += PausarPartida;
		botonMenuPrincipal.Pressed += () => CambiarMenu(uiPausa, confirmacionSalir);
		botonReiniciarPartida.Pressed += () => CambiarMenu(uiPausa, confirmacionReiniciar);

		botonAtacar.Pressed += OnAtacarPressed;
		botonColmena.Pressed += OnBotonColmenaPressed;
		botonPausa.Pressed += PausarPartida;

		GetNode<Button>("ConfirmacionSalir/MarginContainer/VBoxContainer/HBoxContainer/Si/SiButton").Pressed += IrAlMenuPrincipal;
		GetNode<Button>("ConfirmacionSalir/MarginContainer/VBoxContainer/HBoxContainer/No/NoButton").Pressed += () => CambiarMenu(confirmacionSalir, uiPausa);
		GetNode<Button>("ConfirmacionReiniciar/MarginContainer/VBoxContainer/HBoxContainer/Si/SiButton").Pressed += Reiniciar;
		GetNode<Button>("ConfirmacionReiniciar/MarginContainer/VBoxContainer/HBoxContainer/No/NoButton").Pressed += () => CambiarMenu(confirmacionReiniciar, uiPausa);
	}

	private void SuscribirAEventos(){
		_ExitTree();

		var turnManager = GameManager.Instance.TurnManager;
		EventosUI.OnMensajeAMostrar += ActualizarInstrucciones;
		turnManager.OnCambioDeTurnoJugador += OnCambioDeTurno;
		turnManager.OnTurnoCambiado += ActualizarUI;

		GameManager.Instance.OnEstadoAccionesCambiado += AlternarEstadoDeAtaque;
		GameManager.Instance.OnEstadoAccionesCambiado += AlternarEstadoDeAbsorcion;
		GameManager.Instance.OnEstadoAccionesCambiado += ActualizarUI;
	}

	public void CambiarMenu(Control menuAOcultar, Control menuAMostrar){
		menuAOcultar.Visible = false;
		menuAMostrar.Visible = true;
		AudioManager.Instance.PlaySound(UiSound1);
	}
	
	private void ActualizarUI(){
		MostrarJugadorEnTurno();
		MostrarHPDeJugaores();
		MostrarRecursosDeJugadorEnTurno();
		DeshabilitarDado();
		AlternarEstadoDeAtaque();
		AlternarEstadoDeAbsorcion();
		MostrarMovimientosDisponibles();
		ValidarTurnoLocalUI();
	}

	private void OnBotonColmenaPressed(){
		if(!EsTurnoLocal()) return;
		if (instanciaMenuRadial == null){
			instanciaMenuRadial = menuRadialPrefab.Instantiate<MenuRadial>();
			AddChild(instanciaMenuRadial);
			instanciaMenuRadial.EstablecerHUD(this);
		}
		
		instanciaMenuRadial.DesplegarEnPosicion(GetViewportRect().Size / 2);

		AlternarEstadoDeInvocacion();
		AlternarEstadoDeAbsorcion();
		AlternarEstadoDeControl();
		AlternarEstadoDeTransformacion();
	}

	public void OnControlarPressed(){
		if(playerManager == null) return;

		if(!GameManager.Instance.jugadorEnTurno.MenteColmena){		
			GameManager.Instance.jugadorEnTurno.MenteColmena = true;
			tropasManager.MostrarAbejasCercanas();
		}
		else if(GameManager.Instance.jugadorEnTurno.MenteColmena){
			GameManager.Instance.jugadorEnTurno.MenteColmena = false;
			GameManager.Instance.jugadorEnTurno.MoviendoSubdito = false;
			tropasManager.OcultarAbejasCercanas();
			GameManager.Instance.CamaraActual.EnfocarNodo(playerManager.VisualJugadorActual, 2, 1);
		}
	}

	public void OnCurarPressed(){
		if(playerManager == null) return;

		if(!GameManager.Instance.jugadorEnTurno.ModoAbsorcion){		
			GameManager.Instance.jugadorEnTurno.ModoAbsorcion = true;

			if(tropasManager.PuedeAbsorberSubdito())
			{
				tropasManager.MostrarAbejasCercanas();
				GD.Print("Modo de absorcion = " + GameManager.Instance.jugadorEnTurno.ModoAbsorcion);
			}
		}
		else if(GameManager.Instance.jugadorEnTurno.ModoAbsorcion){
			GameManager.Instance.jugadorEnTurno.ModoAbsorcion = false;
			tropasManager.OcultarAbejasCercanas();
			GD.Print("Modo de absorcion = " + GameManager.Instance.jugadorEnTurno.ModoAbsorcion);
		}
	}

	private void OnAtacarPressed(){
		if(playerManager == null) return;
		
		bool subditoAdyacenteAJugador = GameManager.Instance.JugadoresEnPartida.Any(j => tropasManager.SubditoTieneAAlguienCerca(j));

		if (!GameManager.Instance.jugadorEnTurno.ModoAtaque){
			if(GameManager.Instance.PuedeAtacar() && (attackManager.TieneObjetivosCerca() || subditoAdyacenteAJugador)){
				GameManager.Instance.jugadorEnTurno.ModoAtaque = true;
				playerManager.vfxManager.MostrarJugadoresObjetivo();
				playerManager.vfxManager.MostrarAbejasObjetivo();
				GameManager.Instance.PotenciarAtaqueDeJugadorEnTurno();

				if(GameManager.Instance.jugadorEnTurno.AtaquePotenciado){
					playerManager.celdasManager.PintarCeldaDeAbejaBuffeada();
				}
			}
		}else if(GameManager.Instance.jugadorEnTurno.ModoAtaque){
			GameManager.Instance.jugadorEnTurno.ModoAtaque = false;
			playerManager.vfxManager.EsconderOutlineDeJugadores();
			playerManager.vfxManager.OcultarAbejasObjetivo();
			playerManager.celdasManager.DespintarCeldaDeAbejaBuffeada();
		}
		AudioManager.Instance.PlaySound(UiSound1);
	}

	private void AlternarEstadoDeAtaque(){
		if (playerManager == null) return;

		bool subditoAdyacenteAJugador = GameManager.Instance.JugadoresEnPartida.Any(j => tropasManager.SubditoTieneAAlguienCerca(j));
		bool modoAtaqueDisponible = !GameManager.Instance.jugadorEnTurno.AccionConsumida && !GameManager.Instance.jugadorEnTurno.ModoInvocacion && !GameManager.Instance.jugadorEnTurno.ModoAbsorcion;
		bool puedeAtacar = GameManager.Instance.PuedeAtacar() && (attackManager.TieneObjetivosCerca() || subditoAdyacenteAJugador) && modoAtaqueDisponible;

		botonAtacar.Disabled = !puedeAtacar;
	}

	private void AlternarEstadoDeInvocacion(){
		bool puedeInvocar = !GameManager.Instance.jugadorEnTurno.AccionConsumida && !GameManager.Instance.jugadorEnTurno.ModoAtaque && !GameManager.Instance.jugadorEnTurno.ModoAbsorcion;;
		
		if (instanciaMenuRadial != null){
			instanciaMenuRadial.ActualizarEstadoBoton(puedeInvocar, "InvocarButton");
		}
	}

	private void AlternarEstadoDeControl(){
		bool puedeControlar = !GameManager.Instance.jugadorEnTurno.AccionConsumida && !GameManager.Instance.jugadorEnTurno.ModoAtaque && !GameManager.Instance.jugadorEnTurno.ModoAbsorcion && !GameManager.Instance.jugadorEnTurno.ModoInvocacion;
		
		if (instanciaMenuRadial != null){
			instanciaMenuRadial.ActualizarEstadoBoton(puedeControlar, "ControlarButton");
		}
	}

	private void AlternarEstadoDeAbsorcion(){
		if (playerManager == null) return;
		
		bool puedeAbsorber = tropasManager.PuedeAbsorberSubdito() && !GameManager.Instance.jugadorEnTurno.AccionConsumida && !GameManager.Instance.jugadorEnTurno.ModoInvocacion && !GameManager.Instance.jugadorEnTurno.ModoAtaque;

		if (instanciaMenuRadial != null){
			instanciaMenuRadial.ActualizarEstadoBoton(puedeAbsorber, "CurarButton");
		}
	}

	private void AlternarEstadoDeTransformacion(){
		if (playerManager == null) return;

		bool noEstaEnOtroModo = !GameManager.Instance.jugadorEnTurno.ModoInvocacion && !GameManager.Instance.jugadorEnTurno.ModoAtaque && !GameManager.Instance.jugadorEnTurno.ModoAbsorcion;
		bool puedeTransformar = tropasManager.ExistenSubditosTransformables() && !GameManager.Instance.jugadorEnTurno.AccionConsumida && noEstaEnOtroModo;

		if (instanciaMenuRadial != null){
			instanciaMenuRadial.ActualizarEstadoBoton(puedeTransformar, "TransformarButton");
		}
	}

	private void OnTirarDadoPressed(){	
		int resultado = GameManager.Instance.TirarDado();
		resultadoDados.Text = resultado.ToString();
	}

	private void DeshabilitarDado(){
		var jugador = GameManager.Instance.jugadorEnTurno;
		if (jugador == null) return;

		bool yaTiro = jugador.TiroLosDados || jugador.Estado == AbejaReina.EstadoTurno.EsperandoAccion;
		bool esSuTurnoAhora = jugador.EsSuTurno && !yaTiro;

		if(GameManager.Instance.modoMultiplayer){
			long idLocal = Multiplayer.GetUniqueId();
			botonDado.Disabled = !esSuTurnoAhora || (idLocal != jugador.Id);
		} else {
			botonDado.Disabled = !esSuTurnoAhora;
		}
	}

	private void MostrarMovimientosDisponibles(){
		var jugador = GameManager.Instance.jugadorEnTurno;
		if(jugador != null){
			resultadoDados.Text = GameManager.Instance.jugadorEnTurno.MovimientosDisponibles.ToString();
			}
	}

	public void MostrarResultadoDado(){
		if (botonDado.Disabled) return;
		bool esTurnoLocal = !GameManager.Instance.modoMultiplayer || Multiplayer.GetUniqueId() == GameManager.Instance.jugadorEnTurno.Id;
		if (!esTurnoLocal) return;
		int resultado = GameManager.Instance.TirarDado();

		if (resultado != -1){
			if (GameManager.Instance.modoMultiplayer){
				playerManager.Rpc(nameof(PlayerManager.SincronizarDadoRpc), resultado);
			}

			resultadoDados.Text = resultado.ToString();
			ActualizarInstrucciones("Debes moverte antes de atacar o invocar.");
			botonDado.Disabled = true;
			AudioManager.Instance.PlaySound(UiSound4);
		}
	}

	public void InvocarSubdito(){
		if(GameManager.Instance.jugadorEnTurno.TiroLosDados && !GameManager.Instance.jugadorEnTurno.AccionConsumida && !GameManager.Instance.jugadorEnTurno.ModoInvocacion && !GameManager.Instance.jugadorEnTurno.ModoAtaque){	
			GameManager.Instance.jugadorEnTurno.ModoInvocacion = true;

			AlternarEstadoDeAtaque();
			AlternarEstadoDeAbsorcion();
			celdasManager.MostrarCeldasDisponiblesParaInvocar();
		}else if(GameManager.Instance.jugadorEnTurno.TiroLosDados && GameManager.Instance.jugadorEnTurno.ModoInvocacion){
			GameManager.Instance.jugadorEnTurno.ModoInvocacion = false;

			AlternarEstadoDeAtaque();
			AlternarEstadoDeAbsorcion();
			celdasManager.OcultarCeldasDisponiblesParaInvocar();
		}

		AudioManager.Instance.PlaySound(UiSound1);
	}

	public void ActualizarInstrucciones(string texto){
		if(feedback != null){
		feedback.Text = texto;
		}
	}

	private void PausarPartida(){
		GetTree().Paused = !GetTree().Paused;
	
		uiPausa.Visible = GetTree().Paused;

		AudioManager.Instance.PlaySound(UiSound1);

		if (GetTree().Paused){
			GD.Print("Pausa activada");
			containerPausa.Visible = false;
		}else{
			GD.Print("Juego Reanudado");
			containerPausa.Visible = true;
		}
	}

	private void OcultarConfirmacionSalir(){
		confirmacionSalir.Visible = false;
		uiPausa.Visible = true;
		AudioManager.Instance.PlaySound(UiSound1);
	}

	public void Reiniciar(){
		GetTree().Paused = false;

		GameManager.Instance.ReiniciarPartida();

		reinicioConfirmado = true;
		hayJugadorVictorioso = false;

		if (confirmacionReiniciar != null) confirmacionReiniciar.Visible = false;
		if (confirmacionReiniciarVictoria != null) confirmacionReiniciarVictoria.Visible = false;
		if (containerPausa != null) containerPausa.Visible = true;

		GD.Print("Comienza nueva partida");
		GetTree().ChangeSceneToFile("res://Scenes/escenaPrueba.tscn");
	}

	public void IrAlMenuPrincipal(){
		GetTree().Paused = false;

		GameManager.Instance.ResetearEstadoPartida();
		reinicioConfirmado = false;
		hayJugadorVictorioso = false;

		if (confirmacionSalir != null) confirmacionSalir.Visible = false;
		if (confirmacionSalirVictoria != null) confirmacionSalirVictoria.Visible = false;
		if (containerPausa != null) containerPausa.Visible = true;
	
		GD.Print("Regresando al menú principal");
		GetTree().ChangeSceneToFile("res://Scenes/pantallaInicial.tscn");
	}

	private void Ajustes(){
		PanelContainer UIAjustes = GetNode<PanelContainer>("Ajustes");
		AudioManager.Instance.PlaySound(UiSound1);
		UIAjustes.Visible = true;
	}

	private void AceptarConfigAudio(){
		PanelContainer UIAjustesMenuPrincipal = GetNode<PanelContainer>("Ajustes");
		AudioManager.Instance.PlaySound(UiSound1);
		UIAjustesMenuPrincipal.Visible = false;
	}

	private bool EsTurnoLocal(){
		if (!GameManager.Instance.modoMultiplayer) return true;
		var jugador = GameManager.Instance.jugadorEnTurno;
		if (jugador == null) return false;
		return Multiplayer.GetUniqueId() == jugador.Id;
	}

	private void ValidarTurnoLocalUI(){
		bool esNuestroTurno = EsTurnoLocal();
		if (botonAtacar != null) botonAtacar.Disabled = !esNuestroTurno || botonAtacar.Disabled;
		if (botonColmena != null) botonColmena.Disabled = !esNuestroTurno;
		if (!esNuestroTurno && instanciaMenuRadial != null && instanciaMenuRadial.Activo){
			instanciaMenuRadial.OcultarMenu();
			if (instanciaMenuRadial.ContenedorDeTransformaciones != null)
				instanciaMenuRadial.OcultarTransformaciones();
		}
	}

	private void MostrarPantallaDeVictoria(){
		hayJugadorVictorioso = true;
		MostrarMensajeVictoria();
		UIVictoria();
	}

	public void MostrarMensajeVictoria(){
		var mensajeVictoria = GetNode<Label>("Titulo");
		mensajeVictoria.Text = "EL  JUGADOR   " + GameManager.Instance.JugadorGanador.Id + "   ES  EL  GANADOR";
	}
	
	public void UIVictoria(){
		MenuVictoria = GetNode<HBoxContainer>("MenuVictoria");
		botonSalirVictoria = GetNode<Button>("MenuVictoria/VictoriaBorder/MarginContainer/VBoxContainer/MenuPrincipal/MenuPrincipalButton");
		botonReiniciarPartidaVictoria = GetNode<Button>("MenuVictoria/VictoriaBorder/MarginContainer/VBoxContainer/Reiniciar/ReiniciarButton");
		confirmacionReiniciarVictoria = GetNode<PanelContainer>("ConfirmacionReiniciarVictoria");
		confirmacionSalirVictoria = GetNode<PanelContainer>("ConfirmacionSalirVictoria");

		botonSalirVictoria.Pressed += () => CambiarMenu(MenuVictoria, confirmacionSalirVictoria);
		botonReiniciarPartidaVictoria.Pressed += () => CambiarMenu(MenuVictoria, confirmacionReiniciarVictoria);

		GetNode<Button>("ConfirmacionSalirVictoria/MarginContainer/VBoxContainer/HBoxContainer/Si/SiButton").Pressed += IrAlMenuPrincipal;
		GetNode<Button>("ConfirmacionSalirVictoria/MarginContainer/VBoxContainer/HBoxContainer/No/NoButton").Pressed += () => CambiarMenu(confirmacionSalirVictoria, MenuVictoria);
		GetNode<Button>("ConfirmacionReiniciarVictoria/MarginContainer/VBoxContainer/HBoxContainer/Si/SiButton").Pressed += Reiniciar;
		GetNode<Button>("ConfirmacionReiniciarVictoria/MarginContainer/VBoxContainer/HBoxContainer/No/NoButton").Pressed += () => CambiarMenu(confirmacionReiniciarVictoria, MenuVictoria);
	}

	public override void _ExitTree(){
		if (GameManager.Instance != null){
			GameManager.Instance.OnEstadoAccionesCambiado -= AlternarEstadoDeAtaque;

			if (GameManager.Instance.TurnManager != null){
				var turnManager = GameManager.Instance.TurnManager;
				EventosUI.OnMensajeAMostrar -= ActualizarInstrucciones;
				turnManager.OnCambioDeTurnoJugador -= OnCambioDeTurno;
				turnManager.OnTurnoCambiado -= ActualizarUI;
			}
		}
	}
};
