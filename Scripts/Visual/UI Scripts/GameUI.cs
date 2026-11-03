using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class GameUI : UIGeneral {

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

	public void ConfigurarEscenaDeJuego(){
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
	
	public void OnCambioDeTurno(object _) => ActualizarUI();
	
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

	public void MostrarJugadorEnTurno(){ 
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
			}else if(GameManager.Instance.JugadoresEnPartida[j].FueraDeJuego){
				UIJugadores[j].Visible = false;
			}else{
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
