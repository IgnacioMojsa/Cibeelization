using Godot;
using System.Collections.Generic;

public partial class GameUI : Control
{
	[Export] private PlayerManager playerManager;
	[Export] private TroopsManager tropasManager;
	private Label resultadoDados;
	public Label feedback;
	private AudioSetting UiSound1 => AudioManager.Instance?.GameAudio?.Sound1;
	private Button botonDado;
	private Button botonAtacar;

	private Button botonPausa;

	private PanelContainer containerPausa;
	private Button botonContinuar;
	private Button botonMenuPrincipal;
	private HBoxContainer uiPausa;
	private PanelContainer confirmacionSalir;
	
	public override void _Ready(){
		if(GetTree().CurrentScene.SceneFilePath == "res://Scenes/escenaPrueba.tscn"){
			
			InicializarUI();
			SuscribirAEventos();
			MostrarDataDeJugadores();
			ActualizarUI();
			MostrarTextoInstrucciones("Tirá el dado para comenzar.");
			}
		else if(GetTree().CurrentScene.SceneFilePath == "res://Scenes/pantallaVictoria.tscn")
			{
			MostrarMensajeVictoria();
			}
	}		

	public override void _ExitTree()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnEstadoAccionesCambiado -= AlternarEstadoDeAtaque;

			if (GameManager.Instance.TurnManager != null)
			{
				var turnManager = GameManager.Instance.TurnManager;
				turnManager.OnTextoInstrucciones -= MostrarTextoInstrucciones;
				turnManager.OnCambioDeTurnoJugador -= OnCambioDeTurno;
				turnManager.OnTurnoCambiado -= ActualizarUI;
			}
		}
	}

	private void OnCambioDeTurno(object _) => ActualizarUI();

	public void InicializarUI()
	{
		resultadoDados = GetNode<Label>("HBoxContainer/NumeroDado/MarginContainer/Label");
		feedback = GetNode<Label>("Feedback");
		botonDado = GetNode<Button>("HBoxContainer/TirarDado/TirarDadoButton");
		botonAtacar = GetNode<Button>("Atacar/AtacarButton");
		botonPausa = GetNode<Button>("Pausa/PausaButton");
		containerPausa = GetNode<PanelContainer>("Pausa");
		uiPausa = GetNode<HBoxContainer>("MenuPausa");
		confirmacionSalir = GetNode<PanelContainer>("ConfirmacionSalir");
		botonContinuar = GetNode<Button>("MenuPausa/PausaBorder/MarginContainer/VBoxContainer/Continuar/ContinuarButton");
		botonMenuPrincipal = GetNode<Button>("MenuPausa/PausaBorder/MarginContainer/VBoxContainer/MenuPrincipal/MenuPrincipalButton");

		//Suscripciones de godot
		botonAtacar.Pressed += OnAtacarPressed;
		botonPausa.Pressed += PausarPartida;
		botonContinuar.Pressed += PausarPartida; // Reanuda al presionar Continuar
		botonMenuPrincipal.Pressed += MostrarConfirmacionSalir;

		// Botones del cuadro de confirmación
		GetNode<Button>("ConfirmacionSalir/MarginContainer/VBoxContainer/HBoxContainer/Si/SiButton").Pressed += IrAlMenuPrincipal;
		GetNode<Button>("ConfirmacionSalir/MarginContainer/VBoxContainer/HBoxContainer/No/NoButton").Pressed += OcultarConfirmacionSalir;
	}

	private void SuscribirAEventos()
	{
		var turnManager = GameManager.Instance.TurnManager;
		turnManager.OnTextoInstrucciones += MostrarTextoInstrucciones;
		turnManager.OnCambioDeTurnoJugador += OnCambioDeTurno;
		turnManager.OnTurnoCambiado += ActualizarUI;
		
		GameManager.Instance.OnEstadoAccionesCambiado += AlternarEstadoDeAtaque;
	}

	private void ActualizarUI()
	{
		MostrarJugadorEnTurno();
		MostrarHPDeJugaores();
		DeshabilitarDado();
		AlternarEstadoDeAtaque();
	}
	
	private void Jugar(){
		PanelContainer UIComienzo = GetNode<PanelContainer>("MenuComienzo");
		AudioManager.Instance.PlaySound(UiSound1);

		GD.Print("El botón play ha sido presionado");

		UIComienzo.Visible = true;
	}

	private void ComenzarPartida()
	{
		// Aseguramos la lectura correcta de la UI
		GameManager.Instance.cantidadJugadores = ObtenerCantJugadores();
		GameManager.Instance.sizeTablero = ObtenerSizeTablero();

		GameManager.Instance.CargarJugadores(GameManager.Instance.cantidadJugadores);
		GameManager.Instance.CargarTipoDeAbejas();

		GD.Print("La partida se desarrollará con " + GameManager.Instance.cantidadJugadores + " jugadores");
		GD.Print("Opción de tamaño seleccionada: " + GameManager.Instance.sizeTablero);

		AudioManager.Instance.PlaySound(UiSound1);

		GetTree().ChangeSceneToFile("res://Scenes/escenaPrueba.tscn");
		
	}

	private void PausarPartida()
	{
		// Alterna el estado de pausa
		GetTree().Paused = !GetTree().Paused;
	
		// Muestra u oculta el menú principal de pausa
		uiPausa.Visible = GetTree().Paused;

		AudioManager.Instance.PlaySound(UiSound1);

		if (GetTree().Paused)
		{
			GD.Print("Pausa activada");
			containerPausa.Visible = false;
		}
		else
		{
			GD.Print("Juego Reanudado");
			containerPausa.Visible = true;
		}
	}

	private void MostrarConfirmacionSalir()
	{
		uiPausa.Visible = false;
		confirmacionSalir.Visible = true;
		AudioManager.Instance.PlaySound(UiSound1);
	}

	private void OcultarConfirmacionSalir()
	{
		confirmacionSalir.Visible = false;
		uiPausa.Visible = true;
		AudioManager.Instance.PlaySound(UiSound1);
	}

	private void IrAlMenuPrincipal()
	{
		GD.Print("Regresando al menu principal");
		GetTree().Paused = false; // ¡Importante! Despausar antes de cambiar de escena
		GameManager.Instance.ResetearEstadoPartida();
		
		AudioManager.Instance.PlaySound(UiSound1);

		GetTree().ChangeSceneToFile("res://Scenes/pantallaInicial.tscn"); // Ajusta a la ruta de tu menú 
	}

	private void MostrarMensajeVictoria(){
		var mensajeVictoria = GetNode<Label>("Titulo");
		
		mensajeVictoria.Text = "EL  JUGADOR   " + GameManager.Instance.JugadorGanador.Id + "   ES  EL  GANADOR";
	}

	private void SeleccionarCantidadDeJugadores(bool estaPresionado)
	{
		// Solo actuamos cuando el botón pasa a estado presionado (true)
		if (!estaPresionado) return;

		GameManager.Instance.cantidadJugadores = ObtenerCantJugadores();
		GD.Print($"Cantidad de jugadores seleccionada: {GameManager.Instance.cantidadJugadores}");
	}

	private void SeleccionarSizeTablero(bool estaPresionado)
	{
		// Solo actuamos cuando el botón pasa a estado presionado (true)
		if (!estaPresionado) return;

		GameManager.Instance.sizeTablero = ObtenerSizeTablero();
		GD.Print($"Size del tablero seleccionado: {GameManager.Instance.sizeTablero}");
	}

	private int ObtenerCantJugadores(){
		var check2 = GetNode<CheckBox>("MenuComienzo/MarginContainer/VBoxContainer/VBoxContainer/2Players/CheckBox");
		var check3 = GetNode<CheckBox>("MenuComienzo/MarginContainer/VBoxContainer/VBoxContainer/3Players/CheckBox");
		var check4 = GetNode<CheckBox>("MenuComienzo/MarginContainer/VBoxContainer/VBoxContainer/4Players/CheckBox");
		
		if(check2 != null && check2.ButtonPressed){
			return 2;
		}
		else if(check3 != null && check3.ButtonPressed){
			return 3;
		}
		else if(check4 != null && check4.ButtonPressed){
			return 4;
		}
		else{
			return 2;
		}
	}

	private int ObtenerSizeTablero(){
		var check2 = GetNode<CheckBox>("MenuComienzo/MarginContainer/VBoxContainer/Sizes/Small/CheckBox");
		var check3 = GetNode<CheckBox>("MenuComienzo/MarginContainer/VBoxContainer/Sizes/Mid/CheckBox");
		var check4 = GetNode<CheckBox>("MenuComienzo/MarginContainer/VBoxContainer/Sizes/Big/CheckBox");
		
		if(check2 != null && check2.ButtonPressed){
			return 2;
		}
		else if(check3 != null && check3.ButtonPressed){
			return 3;
		}
		else if(check4 != null && check4.ButtonPressed){
			return 4;
		}
		else{
			return 2;
		}
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

		for (int j = 0; j < GameManager.Instance.cantidadJugadores; j++)
		{
			HPJugadores[j].Text = GameManager.Instance.JugadoresEnPartida[j].HP + " HP";
		}
	}

	private void SalirDelJuego(){
		AudioManager.Instance.PlaySound(UiSound1);
		GetTree().Quit();
	}

	private void OnTirarDadoPressed(){	
		int resultado = GameManager.Instance.TirarDado();
		resultadoDados.Text = resultado.ToString();
	}

	private void OnAtacarPressed()
	{
		if(playerManager == null)
		return;
		
		GameManager.Instance.jugadorEnTurno.ModoAtaque = true;
		playerManager.MostrarAbejasObjetivo();
		
		AudioManager.Instance.PlaySound(UiSound1);
	}

	private void DeshabilitarDado(){
		/* if(GameManager.Instance.jugadorEnTurno.EsSuTurno && GameManager.Instance.jugadorEnTurno.Estado == AbejaReina.EstadoTurno.EsperandoAccion){
			botonDado.Disabled = true;
		}
		else if(GameManager.Instance.jugadorEnTurno.EsSuTurno && GameManager.Instance.jugadorEnTurno.Estado == AbejaReina.EstadoTurno.EsperandoDado){
			botonDado.Disabled = false;
		} */

		var jugador = GameManager.Instance.jugadorEnTurno;
		if (jugador == null) return;

		bool yaTiro = jugador.TiroLosDados || jugador.Estado == AbejaReina.EstadoTurno.EsperandoAccion;
		botonDado.Disabled = !jugador.EsSuTurno || yaTiro;
	}

	private void AlternarEstadoDeAtaque(){
		if (playerManager == null) return;
		
		bool puedeAtacar = GameManager.Instance.PuedeAtacar() && playerManager.TieneObjetivosCerca();

		botonAtacar.Disabled = !puedeAtacar;

		if(puedeAtacar){
			playerManager.MostrarJugadoresObjetivo();
		}
	}

	public void MostrarResultadoDado(){
		if (botonDado.Disabled) return;

		int resultado = GameManager.Instance.TirarDado();

		if (resultado != -1)
		{
			resultadoDados.Text = resultado.ToString();
			MostrarTextoInstrucciones("Podés moverte por las celdas, atacar o invocar un súbdito.");
			botonDado.Disabled = true;

			if(AudioManager.Instance?.GameAudio?.Sound4 != null)
			{
				var dado = AudioManager.Instance.GameAudio.Sound4;
				AudioManager.Instance.PlaySound(dado);
			}
		}
	}

	private void MostrarJugadorEnTurno(){ 
		AbejaReina jugadorEnTurno = GameManager.Instance.jugadorEnTurno;

		var jugador1 = GetNode<PanelContainer>("VBoxContainer/Jugador1");
		var jugador2 = GetNode<PanelContainer>("VBoxContainer/Jugador2");
		var jugador3 = GetNode<PanelContainer>("VBoxContainer/Jugador3");
		var jugador4 = GetNode<PanelContainer>("VBoxContainer/Jugador4");

		List<PanelContainer> UIJugadores = new List<PanelContainer>{ jugador1, jugador2, jugador3, jugador4};

		for (int j = 0; j < GameManager.Instance.JugadoresEnPartida.Count; j++)
		{
			if( GameManager.Instance.JugadoresEnPartida[j] == jugadorEnTurno && !(GameManager.Instance.JugadoresEnPartida[j].FueraDeJuego)){
				UIJugadores[j].Modulate = Color.FromHtml("#9005F2");
				playerManager.OutlineJugadorEnTurno(playerManager.OutlinesJugadores[j]);
				playerManager.OutlinesJugadores[j].Visible = true;
			}
			else if(GameManager.Instance.JugadoresEnPartida[j].FueraDeJuego){
				UIJugadores[j].Visible = false;
			}
			else{
				UIJugadores[j].Modulate = Color.FromHtml("#ffffff");
				playerManager.OutlinesJugadores[j].Visible = false;
			}
		}
		
		//ffd01f
	}

	private void InvocarSubdito()
	{
		if(GameManager.Instance.jugadorEnTurno.TiroLosDados){	
			GameManager.Instance.jugadorEnTurno.ModoInvocacion = true;
			playerManager.MostrarCeldasDisponiblesParaInvocar();
		}
		AudioManager.Instance.PlaySound(UiSound1);
	}

	public void MostrarTextoInstrucciones(string texto)
	{
		feedback.Text = texto;
	}
};
