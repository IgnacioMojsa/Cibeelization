using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;

public partial class GameUI : UIBase {
	
	[Export] public TroopsManager tropasManager;
	[Export] public CeldasManager celdasManager; 
	[Export] public AttackManager attackManager;
	[Export] public Label feedback;
	[Export] public PackedScene menuRadialPrefab;
	public MenuRadial instanciaMenuRadial;
	public Button botonPausa;
	public TextureButton botonColmena;
	public PanelContainer containerPausa;
	public HBoxContainer uiPausa;
	public Label resultadoDados;
	public TextureButton botonDado;
	public TextureButton botonAtacar;
	public bool hayJugadorVictorioso = false;
	public bool reinicioConfirmado = false;
	public Button botonContinuar;
	public Button botonReiniciarPartida;
	public Button botonMenuPrincipal;
	public PanelContainer confirmacionSalir;
	public PanelContainer confirmacionReiniciar;
	public PanelContainer confirmacionSalirVictoria;
	public PanelContainer confirmacionReiniciarVictoria;
	public HBoxContainer MenuVictoria;
	public Button botonSalirVictoria;
	public Button botonReiniciarPartidaVictoria;

	[Export] public PlayerManager playerManager;
	public Control recursos;

	public override void _Ready(){
		ProcessMode = ProcessModeEnum.Always;
		
		string currentScene = GetTree().CurrentScene.SceneFilePath;

		if(currentScene == "res://Scenes/escenaPrueba.tscn"){
			ConfigurarEscenaDeJuego();
		}
		else if(currentScene == "res://Scenes/pantallaVictoria.tscn"){
			MostrarPantallaDeVictoria();
		}
		else if(currentScene == "res://Scenes/pantallaEmpate.tscn"){
			MostrarPantallaDeEmpate();
		}

		Multiplayer.PeerDisconnected += OnPeerDisconnectedInGame;
	}

	private MenuRadial ObtenerMenuRadial(){
	    if (instanciaMenuRadial == null){
	        instanciaMenuRadial = menuRadialPrefab.Instantiate<MenuRadial>();
	        AddChild(instanciaMenuRadial);
	        instanciaMenuRadial.EstablecerHUD(this);
	    }
	    return instanciaMenuRadial;
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

		GameManager.Instance.CamaraActual = root.GetNode<CamaraController>("Camara");
		playerManager = root.GetNode<PlayerManager>("PlayerManager");
		GameManager.Instance.SetTiles(GameManager.Instance.sizeTablero);
		
		InicializarGameUI();
		SuscribirAEventos();
		MostrarDataDeJugadores();
		ActualizarUI();
	}
	
	private void OnCambioDeTurno(object _) => ActualizarUI();
	
	private void InicializarGameUI(){
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
		
		var contenedorReiniciarPausa = GetNodeOrNull<Control>("MenuPausa/PausaBorder/MarginContainer/VBoxContainer/Reiniciar");
		if (contenedorReiniciarPausa != null){
			contenedorReiniciarPausa.Visible = !GameManager.Instance.modoMultiplayer;
		}

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
		//_ExitTree();

		var turnManager = GameManager.Instance.TurnManager;
		EventosUI.OnMensajeAMostrar += ActualizarInstrucciones;
		if (turnManager != null){
			turnManager.OnCambioDeTurnoJugador += OnCambioDeTurno;
			turnManager.OnTurnoCambiado += ActualizarUI;
		}

		GameManager.Instance.OnEstadoAccionesCambiado += AlternarEstadoDeAtaque;
		GameManager.Instance.OnEstadoAccionesCambiado += AlternarEstadoDeAbsorcion;
		GameManager.Instance.OnEstadoAccionesCambiado += ActualizarUI;
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

		if (instanciaMenuRadial != null && instanciaMenuRadial.ContenedorDeTransformaciones != null){
			if(instanciaMenuRadial.ContenedorDeTransformaciones.Visible){
				instanciaMenuRadial.OcultarTransformaciones();
			} else {
				instanciaMenuRadial.OcultarMenu();
			}
		}
	}

	private void ActualizarInstrucciones(string texto){
		if(feedback != null && IsInstanceValid(feedback)){
		feedback.Text = texto;
		}
	}

	private void PausarPartida(){
		GetTree().Paused = !GetTree().Paused;
	
		uiPausa.Visible = GetTree().Paused;

		AudioManager.Instance.PlaySound(UiSound1);

		if (GetTree().Paused){
			GD.Print("Pausa activada");
			MostrarUIPanel(containerPausa, false);
		}else{
			GD.Print("Juego Reanudado");
			MostrarUIPanel(containerPausa, true);
		}
	}

	private void OcultarConfirmacionSalir(){
		CambiarMenu(confirmacionSalir, uiPausa);
	}

	private void Reiniciar(){
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

	private void IrAlMenuPrincipal(){
		GetTree().Paused = false;
		ForzarDesconexionYLimpiezaTotal();
		if (confirmacionSalir != null) confirmacionSalir.Visible = false;
		if (confirmacionSalirVictoria != null) confirmacionSalirVictoria.Visible = false;
		if (containerPausa != null) containerPausa.Visible = true;
		
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
		if (GetTree().CurrentScene.SceneFilePath != "res://Scenes/pantallaInicial.tscn"){
			GetTree().ChangeSceneToFile("res://Scenes/pantallaInicial.tscn");
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

	private void MostrarDataDeJugadores(){
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

	private void MostrarHPDeJugaores(){
		var hpJ1 = GetNode<Label>("VBoxContainer/Jugador1/HBoxContainer/MarginContainer2/Label");
		var hpJ2 = GetNode<Label>("VBoxContainer/Jugador2/HBoxContainer/MarginContainer2/Label");
		var hpJ3 = GetNode<Label>("VBoxContainer/Jugador3/HBoxContainer/MarginContainer2/Label");
		var hpJ4 = GetNode<Label>("VBoxContainer/Jugador4/HBoxContainer/MarginContainer2/Label");

		List<Label> HPJugadores = new List<Label>{ hpJ1, hpJ2, hpJ3, hpJ4};

		for (int j = 0; j < GameManager.Instance.cantidadJugadores; j++){
			HPJugadores[j].Text = GameManager.Instance.JugadoresEnPartida[j].HP + " HP";
		}
	}

	private void MostrarJugadorEnTurno(){ 
		AbejaReina jugadorEnTurno = GameManager.Instance.jugadorEnTurno;
		TextureRect iconoJugadorEnTurno = GetNode<TextureRect>("Elementos/IconoJugador");

		var jugador1 = GetNode<PanelContainer>("VBoxContainer/Jugador1");
		var jugador2 = GetNode<PanelContainer>("VBoxContainer/Jugador2");
		var jugador3 = GetNode<PanelContainer>("VBoxContainer/Jugador3");
		var jugador4 = GetNode<PanelContainer>("VBoxContainer/Jugador4");

		var colorJ1 = Color.FromHtml("#6a268f");
		var colorJ2 = Color.FromHtml("#4c1dae");
		var colorJ3 = Color.FromHtml("#b33671");
		var colorJ4 = Color.FromHtml("#8260e5");

		var iconoJ1 = GD.Load<CompressedTexture2D>("res://Assets/UI/AbejarReina1Contorno.svg");
		var iconoJ2 = GD.Load<CompressedTexture2D>("res://Assets/UI/AbejarReina2Contorno.svg");
		var iconoJ3 = GD.Load<CompressedTexture2D>("res://Assets/UI/AbejarReina3Contorno.svg");
		var iconoJ4 = GD.Load<CompressedTexture2D>("res://Assets/UI/AbejarReina4Contorno.svg");

		List<PanelContainer> UIJugadores = new List<PanelContainer>{ jugador1, jugador2, jugador3, jugador4};
		List<Color> ColorJugadores = new List<Color>{ colorJ1, colorJ2, colorJ3, colorJ4};
		List<CompressedTexture2D> IconosJugadores = new List<CompressedTexture2D>{iconoJ1, iconoJ2, iconoJ3, iconoJ4};

		for (int j = 0; j < GameManager.Instance.JugadoresEnPartida.Count; j++){
			if( GameManager.Instance.JugadoresEnPartida[j] == jugadorEnTurno && !(GameManager.Instance.JugadoresEnPartida[j].FueraDeJuego)){
				UIJugadores[j].Modulate = ColorJugadores[j];
				iconoJugadorEnTurno.Texture = IconosJugadores[j];
			} else if (GameManager.Instance.JugadoresEnPartida[j].FueraDeJuego){
				UIJugadores[j].Visible = false;
			} else {
				UIJugadores[j].Modulate = Color.FromHtml("#ffffff");
			}
		}

		if (playerManager.VisualesJugadores.Count >= jugadorEnTurno.Id){
			playerManager.VisualJugadorActual = playerManager.VisualesJugadores[jugadorEnTurno];
			playerManager.vfxManager.OutlineJugadorEnTurno(playerManager.VisualJugadorActual);
		}
	}

	public void MostrarRecursosDeJugadorEnTurno(){
		var jugadorEnTurno = GameManager.Instance.jugadorEnTurno;

		HashSet<string> recursosActuales = new HashSet<string>();

		if(recursosActuales.Count == 0){
			var nodosHijos = recursos.GetChildren();

			foreach (var hijo in nodosHijos){
					if(hijo is Label hijoLabel)
					
					hijoLabel.Text = "0";
				}
		}

		foreach (var recurso in jugadorEnTurno.ColmenaDeReina.RecursosDeColmena){
			if(recurso == null) break;
			
			recursosActuales.Add(recurso);
		}

		foreach (var recurso in recursosActuales){
			var cantRecurso = recursos.GetNode<Label>(recurso);

			cantRecurso.Text = jugadorEnTurno.ColmenaDeReina.CantidadTotalDeUnRecurso(recurso).ToString();
		}
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
			if(tropasManager.SubditoActual is AbejaPolinizadora) GameManager.Instance.jugadorEnTurno.MovimientosDisponibles /= 2;
			if(tropasManager.SubditoActual is AbejaGuardiana) GameManager.Instance.jugadorEnTurno.MovimientosDisponibles *= 2;
			
			
			tropasManager.OcultarAbejasCercanas();
			playerManager.TerminarControlDeSubdito();
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
		if(playerManager == null || !EsTurnoLocal()) return;

		if(!GameManager.Instance.jugadorEnTurno.ModoAtaque)GameManager.Instance.jugadorEnTurno.ModoAtaque = true;

		ObtenerMenuRadial().DesplegarEnPosicion(GetViewportRect().Size / 2);
    	AlternarEstadoDeAtaque();

		AudioManager.Instance.PlaySound(UiSound1);
	}

	public void AtaqueAObjetivo()
	{
		bool subditoAdyacenteAJugador = GameManager.Instance.JugadoresEnPartida.Any(j => tropasManager.SubditoTieneAAlguienCerca(j));

		if(GameManager.Instance.PuedeAtacar() && (attackManager.TieneObjetivosCerca() || subditoAdyacenteAJugador)){
			GameManager.Instance.jugadorEnTurno.ModoAtaque = true;
			playerManager.vfxManager.MostrarJugadoresObjetivo();
			playerManager.vfxManager.MostrarAbejasObjetivo();
			GameManager.Instance.PotenciarAtaqueDeJugadorEnTurno();
			if(GameManager.Instance.jugadorEnTurno.AtaquePotenciado){
				playerManager.celdasManager.PintarCeldaDeAbejaBuffeada();
			}
		}

		if(GameManager.Instance.jugadorEnTurno.MoviendoSubdito && tropasManager.SubditoActual is AbejaGuardiana)
		{
			attackManager.AtacarConGuardiana();
			playerManager.TerminarControlDeSubdito();
		}
	}

	private void AlternarEstadoDeAtaque(){
		if (playerManager == null || instanciaMenuRadial == null) return;

		bool subditoAdyacenteAJugador = GameManager.Instance.JugadoresEnPartida.Any(j => tropasManager.SubditoTieneAAlguienCerca(j));
		bool modoAtaqueDisponible = !GameManager.Instance.jugadorEnTurno.AccionConsumida && !GameManager.Instance.jugadorEnTurno.ModoInvocacion && !GameManager.Instance.jugadorEnTurno.ModoAbsorcion;
		bool puedeAtacar = GameManager.Instance.PuedeAtacar() && (attackManager.TieneObjetivosCerca() || subditoAdyacenteAJugador) && modoAtaqueDisponible;

		instanciaMenuRadial.ActualizarEstadoBoton(puedeAtacar, "AtacarButton");
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

	public void MostrarMovimientosDisponibles(){
		var jugador = GameManager.Instance.jugadorEnTurno;
		if(jugador != null){
			resultadoDados.Text = GameManager.Instance.jugadorEnTurno.MovimientosDisponibles.ToString();
			}
	}

	private void MostrarResultadoDado(){
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
		if(GameManager.Instance.jugadorEnTurno.TiroLosDados && !GameManager.Instance.jugadorEnTurno.AccionConsumida 
		&& !GameManager.Instance.jugadorEnTurno.ModoInvocacion && !GameManager.Instance.jugadorEnTurno.ModoAtaque){	
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

	public override void _ExitTree(){
	Multiplayer.PeerDisconnected -= OnPeerDisconnectedInGame;
	EventosUI.OnMensajeAMostrar -= ActualizarInstrucciones;

	if(GameManager.Instance != null){
		GameManager.Instance.OnEstadoAccionesCambiado -= AlternarEstadoDeAtaque;
		GameManager.Instance.OnEstadoAccionesCambiado -= AlternarEstadoDeAbsorcion;
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
