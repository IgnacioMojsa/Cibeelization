using Godot;
using System.Collections.Generic;

public partial class GameUI : Control {
	[Export] private PlayerManager playerManager;
	[Export] private TroopsManager tropasManager;
	[Export] public Label feedback;
	private Button botonPausa;
	private PanelContainer containerPausa;
	private HBoxContainer uiPausa;
	private Label resultadoDados;
	public AudioSetting UiSound1 => AudioManager.Instance?.GameAudio?.Sound1;
	private Button botonDado;
	private Button botonAtacar;
	private Button botonCurar;
	public bool hayJugadorVictorioso = false;
	public bool reinicioConfirmado = false;
	private Button botonContinuar;
	private Button botonReiniciarPartida;
	private Button botonMenuPrincipal;
	private PanelContainer confirmacionSalir;
	private PanelContainer confirmacionReiniciar;
	public PanelContainer confirmacionSalirVictoria;
	public PanelContainer confirmacionReiniciarVictoria;
	private HBoxContainer MenuVictoria;
	private Button botonSalirVictoria;
	private Button botonReiniciarPartidaVictoria;
	
	public override void _Ready(){
		
		if(GetTree().CurrentScene.SceneFilePath == "res://Scenes/escenaPrueba.tscn"){

			var root = GetTree().CurrentScene; 

			GameManager.Instance.TableroActual = root.GetNodeOrNull<Tablero>("Tablero");
				if (GameManager.Instance.TableroActual == null)
					GD.PrintErr("No se encontró Tablero en la escena");

			GameManager.Instance.CamaraActual = root.GetNode<CamaraController>("Camara");
			playerManager = root.GetNode<PlayerManager>("PlayerManager");

			//Tablero válido:
			GameManager.Instance.SetTiles(GameManager.Instance.sizeTablero);
			
			InicializarGameUI();
			SuscribirAEventos();
			MostrarDataDeJugadores();
			ActualizarUI();
			ActualizarInstrucciones("Tirá el dado para comenzar.");
			}
		else if(GetTree().CurrentScene.SceneFilePath == "res://Scenes/pantallaVictoria.tscn")
			{
				MostrarPantallaDeVictoria();
			}
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

	private void OnCambioDeTurno(object _) => ActualizarUI();

	public void InicializarGameUI(){
		botonPausa = GetNode<Button>("Pausa/PausaButton");
		containerPausa = GetNode<PanelContainer>("Pausa");
		uiPausa = GetNode<HBoxContainer>("MenuPausa");
		confirmacionSalir = GetNode<PanelContainer>("ConfirmacionSalir");
		confirmacionReiniciar = GetNode<PanelContainer>("ConfirmacionReiniciar");
		botonContinuar = GetNode<Button>("MenuPausa/PausaBorder/MarginContainer/VBoxContainer/Continuar/ContinuarButton");
		botonMenuPrincipal = GetNode<Button>("MenuPausa/PausaBorder/MarginContainer/VBoxContainer/MenuPrincipal/MenuPrincipalButton");
		botonReiniciarPartida = GetNode<Button>("MenuPausa/PausaBorder/MarginContainer/VBoxContainer/Reiniciar/ReiniciarButton");

		botonPausa = GetNode<Button>("Pausa/PausaButton");
		containerPausa = GetNode<PanelContainer>("Pausa");
		uiPausa = GetNode<HBoxContainer>("MenuPausa");
		resultadoDados = GetNode<Label>("HBoxContainer/NumeroDado/MarginContainer/Label");
		feedback = GetNode<Label>("Feedback");
		botonDado = GetNode<Button>("HBoxContainer/TirarDado/TirarDadoButton");
		botonAtacar = GetNode<Button>("Atacar/AtacarButton");
		botonCurar = GetNode<Button>("Curar/CurarButton");

		//Suscripciones de godot
		botonAtacar.Pressed += OnAtacarPressed;
		botonCurar.Pressed += OnCurarPressed;
		botonPausa.Pressed += PausarPartida;
		botonContinuar.Pressed += PausarPartida; // Reanuda al presionar Continuar
		botonMenuPrincipal.Pressed += MostrarConfirmacionSalir;
		botonReiniciarPartida.Pressed += MostrarConfirmacionReiniciar;

		// Botones del cuadro de confirmación para regresar al menu principal
		GetNode<Button>("ConfirmacionSalir/MarginContainer/VBoxContainer/HBoxContainer/Si/SiButton").Pressed += IrAlMenuPrincipal;
		GetNode<Button>("ConfirmacionSalir/MarginContainer/VBoxContainer/HBoxContainer/No/NoButton").Pressed += OcultarConfirmacionSalir;

		// Botones del cuadro de confirmación para reiniciar la partida
		GetNode<Button>("ConfirmacionReiniciar/MarginContainer/VBoxContainer/HBoxContainer/Si/SiButton").Pressed += Reiniciar;
		GetNode<Button>("ConfirmacionReiniciar/MarginContainer/VBoxContainer/HBoxContainer/No/NoButton").Pressed += OcultarConfirmacionReiniciar;
	}

	private void SuscribirAEventos(){
		// Primero limpiar suscripciones viejas
		_ExitTree();

		var turnManager = GameManager.Instance.TurnManager;
		EventosUI.OnMensajeAMostrar += ActualizarInstrucciones;
		turnManager.OnCambioDeTurnoJugador += OnCambioDeTurno;
		turnManager.OnTurnoCambiado += ActualizarUI;

		//GameManager.Instance.OnEstadoAccionesCambiado -= AlternarEstadoDeAtaque;
		GameManager.Instance.OnEstadoAccionesCambiado += AlternarEstadoDeAtaque;
		GameManager.Instance.OnEstadoAccionesCambiado += AlternarEstadoDeAbsorcion;
	}

	private void ActualizarUI(){
		MostrarJugadorEnTurno();
		MostrarHPDeJugaores();
		DeshabilitarDado();
		AlternarEstadoDeAtaque();
		AlternarEstadoDeAbsorcion();
	}

	public void MostrarDataDeJugadores(){
		var jugador1 = GetNode<PanelContainer>("VBoxContainer/Jugador1");
		var jugador2 = GetNode<PanelContainer>("VBoxContainer/Jugador2");
		var jugador3 = GetNode<PanelContainer>("VBoxContainer/Jugador3");
		var jugador4 = GetNode<PanelContainer>("VBoxContainer/Jugador4");

		jugador1.Visible = true;
		jugador2.Visible = true;

		if(GameManager.Instance.cantidadJugadores == 3){
			jugador3.Visible = true;
		}
		else if(GameManager.Instance.cantidadJugadores == 4){
			jugador3.Visible = true;
			jugador4.Visible = true;
		}
	}

	public void MostrarHPDeJugaores(){
		var hpJ1 = GetNode<Label>("VBoxContainer/Jugador1/HBoxContainer/MarginContainer2/Label");
		var hpJ2 = GetNode<Label>("VBoxContainer/Jugador2/HBoxContainer/MarginContainer2/Label");
		var hpJ3 = GetNode<Label>("VBoxContainer/Jugador3/HBoxContainer/MarginContainer2/Label");
		var hpJ4 = GetNode<Label>("VBoxContainer/Jugador4/HBoxContainer/MarginContainer2/Label");

		List<Label> HPJugadores = new List<Label>{ hpJ1, hpJ2, hpJ3, hpJ4};

		for (int j = 0; j < GameManager.Instance.cantidadJugadores; j++){
			HPJugadores[j].Text = GameManager.Instance.JugadoresEnPartida[j].HP + " HP";
		}
	}

	private void OnTirarDadoPressed(){	
		int resultado = GameManager.Instance.TirarDado();
		resultadoDados.Text = resultado.ToString();
	}

	private void OnCurarPressed(){
		if(playerManager == null) return;

		if(!GameManager.Instance.jugadorEnTurno.ModoAbsorcion){		
			GameManager.Instance.jugadorEnTurno.ModoAbsorcion = true;
			tropasManager.MostrarAbejasAAbsorber();
		}else{
			GameManager.Instance.jugadorEnTurno.ModoAbsorcion = false;
			tropasManager.OcultarAbejasAAbsorber();
		}
	}

	private void OnAtacarPressed(){
		if(playerManager == null) return;

		if (!GameManager.Instance.jugadorEnTurno.ModoAtaque){
			GameManager.Instance.jugadorEnTurno.ModoAtaque = true;
		
			if(GameManager.Instance.PuedeAtacar() && playerManager.TieneObjetivosCerca()){
				playerManager.MostrarJugadoresObjetivo();
				playerManager.MostrarAbejasObjetivo();
				GameManager.Instance.PotenciarAtaqueDeJugadorEnTurno();

				if(GameManager.Instance.jugadorEnTurno.AtaquePotenciado){
					playerManager.PintarCeldaDeAbejaBuffeada();
				}
			}
		}else if(GameManager.Instance.jugadorEnTurno.ModoAtaque){
			GameManager.Instance.jugadorEnTurno.ModoAtaque = false;

			playerManager.EsconderOutlineDeJugadores();
			playerManager.OcultarAbejasObjetivo();
			playerManager.DespintarCeldaDeAbejaBuffeada();
		}
		AudioManager.Instance.PlaySound(UiSound1);
	}

	private void DeshabilitarDado(){
		var jugador = GameManager.Instance.jugadorEnTurno;
		if (jugador == null) return;

		bool yaTiro = jugador.TiroLosDados || jugador.Estado == AbejaReina.EstadoTurno.EsperandoAccion;
		botonDado.Disabled = !jugador.EsSuTurno || yaTiro;
	}

	private void AlternarEstadoDeAtaque(){
		if (playerManager == null) return;
		
		bool puedeAtacar = GameManager.Instance.PuedeAtacar() && playerManager.TieneObjetivosCerca() && !GameManager.Instance.jugadorEnTurno.ModoInvocacion;

		botonAtacar.Disabled = !puedeAtacar;
	}

	private void AlternarEstadoDeAbsorcion(){
		if (playerManager == null) return;
		
		bool puedeAbsorber = tropasManager.PuedeAbsorberSubdito() && !GameManager.Instance.jugadorEnTurno.ModoInvocacion && !GameManager.Instance.jugadorEnTurno.ModoAtaque;

		botonCurar.Disabled = !puedeAbsorber;
	}

	public void MostrarResultadoDado(){
		if (botonDado.Disabled) return;

		int resultado = GameManager.Instance.TirarDado();

		if (resultado != -1){
			resultadoDados.Text = resultado.ToString();
			ActualizarInstrucciones("Debes moverte antes de atacar o invocar.");
			botonDado.Disabled = true;

			if(AudioManager.Instance?.GameAudio?.Sound4 != null){
				var dado = AudioManager.Instance.GameAudio.Sound4;
				AudioManager.Instance.PlaySound(dado);
			}
		}
	}

//hay q parametrizar esto
	private void MostrarJugadorEnTurno(){ 
		AbejaReina jugadorEnTurno = GameManager.Instance.jugadorEnTurno;

		var jugador1 = GetNode<PanelContainer>("VBoxContainer/Jugador1");
		var jugador2 = GetNode<PanelContainer>("VBoxContainer/Jugador2");
		var jugador3 = GetNode<PanelContainer>("VBoxContainer/Jugador3");
		var jugador4 = GetNode<PanelContainer>("VBoxContainer/Jugador4");

		var colorJ1 = Color.FromHtml("#6a268f");
		var colorJ2 = Color.FromHtml("#4c1dae");
		var colorJ3 = Color.FromHtml("#b33671");
		var colorJ4 = Color.FromHtml("#8260e5");

		List<PanelContainer> UIJugadores = new List<PanelContainer>{ jugador1, jugador2, jugador3, jugador4};
		List<Color> ColorJugadores = new List<Color>{ colorJ1, colorJ2, colorJ3, colorJ4};

		for (int j = 0; j < GameManager.Instance.JugadoresEnPartida.Count; j++){
			if( GameManager.Instance.JugadoresEnPartida[j] == jugadorEnTurno && !(GameManager.Instance.JugadoresEnPartida[j].FueraDeJuego)){
				UIJugadores[j].Modulate = ColorJugadores[j];
			}else if(GameManager.Instance.JugadoresEnPartida[j].FueraDeJuego){
				UIJugadores[j].Visible = false;
			}else{
				UIJugadores[j].Modulate = Color.FromHtml("#ffffff");
			}
		}

		if (playerManager.VisualesJugadores.Count >= jugadorEnTurno.Id){
			playerManager.VisualJugadorActual = playerManager.VisualesJugadores[jugadorEnTurno.Id - 1];
			playerManager.OutlineJugadorEnTurno(playerManager.VisualJugadorActual);
		}
	}

	private void InvocarSubdito(){
		if(GameManager.Instance.jugadorEnTurno.TiroLosDados && !GameManager.Instance.jugadorEnTurno.ModoInvocacion && !GameManager.Instance.jugadorEnTurno.ModoAtaque){	
			GameManager.Instance.jugadorEnTurno.ModoInvocacion = true;
			AlternarEstadoDeAtaque();
			AlternarEstadoDeAbsorcion();
			playerManager.MostrarCeldasDisponiblesParaInvocar();
		}else if(GameManager.Instance.jugadorEnTurno.TiroLosDados && GameManager.Instance.jugadorEnTurno.ModoInvocacion){
			GameManager.Instance.jugadorEnTurno.ModoInvocacion = false;
			AlternarEstadoDeAtaque();
			AlternarEstadoDeAbsorcion();
			playerManager.OcultarCeldasDisponiblesParaInvocar();
		}

		AudioManager.Instance.PlaySound(UiSound1);
	}

	public void ActualizarInstrucciones(string texto){
		if(feedback != null){
		feedback.Text = texto;
		}
	}

	private void PausarPartida(){
		// Alterna el estado de pausa
		GetTree().Paused = !GetTree().Paused;
	
		// Muestra u oculta el menú principal de pausa
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

	private void MostrarConfirmacionReiniciar(){
		uiPausa.Visible = false;
		confirmacionReiniciar.Visible = true;
	}

	private void OcultarConfirmacionReiniciar(){
		confirmacionReiniciar.Visible = false;
		uiPausa.Visible = true;
	}

	private void MostrarConfirmacionSalir(){
		uiPausa.Visible = false;
		confirmacionSalir.Visible = true;
		AudioManager.Instance.PlaySound(UiSound1);
	}

	private void OcultarConfirmacionSalir(){
		confirmacionSalir.Visible = false;
		uiPausa.Visible = true;
		AudioManager.Instance.PlaySound(UiSound1);
	}

	public void Reiniciar(){
		GetTree().Paused = false;

		// Reinicia la partida con los mismos parámetros guardados en PartidaActual
		GameManager.Instance.ReiniciarPartida();

		reinicioConfirmado = true;
		hayJugadorVictorioso = false;

		// Ocultar confirmaciones si existen
		if (confirmacionReiniciar != null) confirmacionReiniciar.Visible = false;
		if (confirmacionReiniciarVictoria != null) confirmacionReiniciarVictoria.Visible = false;
		if (containerPausa != null) containerPausa.Visible = true;

		GD.Print("Comienza nueva partida");
		GetTree().ChangeSceneToFile("res://Scenes/escenaPrueba.tscn");
	}

	public void IrAlMenuPrincipal(){
		GetTree().Paused = false;
		// Limpia todo para volver al menú principal

		GameManager.Instance.ResetearEstadoPartida();
	
		reinicioConfirmado = false;
		hayJugadorVictorioso = false;
	
		// Ocultar confirmaciones si existen
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

	private void MostrarPantallaDeVictoria(){
		hayJugadorVictorioso = true;
		MostrarMensajeVictoria();
		UIVictoria();
	}

	public void UIVictoria(){
		MenuVictoria = GetNode<HBoxContainer>("MenuVictoria");
		botonSalirVictoria = GetNode<Button>("MenuVictoria/VictoriaBorder/MarginContainer/VBoxContainer/MenuPrincipal/MenuPrincipalButton");
		botonReiniciarPartidaVictoria = GetNode<Button>("MenuVictoria/VictoriaBorder/MarginContainer/VBoxContainer/Reiniciar/ReiniciarButton");
		confirmacionReiniciarVictoria = GetNode<PanelContainer>("ConfirmacionReiniciarVictoria");
		confirmacionSalirVictoria = GetNode<PanelContainer>("ConfirmacionSalirVictoria");

		botonSalirVictoria.Pressed += MostrarConfirmacionSalirVictoria;
		botonReiniciarPartidaVictoria.Pressed += MostrarConfirmacionReiniciarVictoria;

		// Botones del cuadro de confirmación para regresar al menu principal
		GetNode<Button>("ConfirmacionSalirVictoria/MarginContainer/VBoxContainer/HBoxContainer/Si/SiButton").Pressed += IrAlMenuPrincipal;
		GetNode<Button>("ConfirmacionSalirVictoria/MarginContainer/VBoxContainer/HBoxContainer/No/NoButton").Pressed += OcultarConfirmacionSalirVictoria;

		// Botones del cuadro de confirmación para reiniciar la partida
		GetNode<Button>("ConfirmacionReiniciarVictoria/MarginContainer/VBoxContainer/HBoxContainer/Si/SiButton").Pressed += Reiniciar;
		GetNode<Button>("ConfirmacionReiniciarVictoria/MarginContainer/VBoxContainer/HBoxContainer/No/NoButton").Pressed += OcultarConfirmacionReiniciarVictoria;
	}

	private void MostrarConfirmacionSalirVictoria(){
		MenuVictoria.Visible = false;
		confirmacionSalirVictoria.Visible = true;
		AudioManager.Instance.PlaySound(UiSound1);
	}

	private void OcultarConfirmacionSalirVictoria(){
		confirmacionSalirVictoria.Visible = false;
		MenuVictoria.Visible = true;
		AudioManager.Instance.PlaySound(UiSound1);
	}

	private void MostrarConfirmacionReiniciarVictoria(){
		MenuVictoria.Visible = false;
		confirmacionReiniciarVictoria.Visible = true;
		AudioManager.Instance.PlaySound(UiSound1);
	}

	private void OcultarConfirmacionReiniciarVictoria(){
		confirmacionReiniciarVictoria.Visible = false;
		MenuVictoria.Visible = true;
		AudioManager.Instance.PlaySound(UiSound1);
	}

	public void MostrarMensajeVictoria(){
		var mensajeVictoria = GetNode<Label>("Titulo");
		
		mensajeVictoria.Text = "EL  JUGADOR   " + GameManager.Instance.JugadorGanador.Id + "   ES  EL  GANADOR";
	}
};
