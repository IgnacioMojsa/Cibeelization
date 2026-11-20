using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class HudInferior : UIBase
{
	public PlayerManager playerManager;
	public TroopsManager tropasManager;
	public CeldasManager celdasManager; 
	public AttackManager attackManager;
	public GameUI gameUI;
	
	[Export] public PackedScene menuRadialPrefab;
	public MenuRadial instanciaMenuRadial;
	public Label feedback;
	public Label feedbackEspectador;
	
	public Label resultadoDados;
	public TextureButton botonAtacar;
	public TextureButton botonDado;
	public TextureButton botonColmena;
	public bool tocoBotonAtacarYDebeDesplegarseElMenu = false;
	public event System.Action OnUIActualizacionRequerida;

	public void Inicializar(GameUI ui, PlayerManager pm, TroopsManager tm, CeldasManager cm, AttackManager am){
		gameUI = ui;
		playerManager = pm;
		tropasManager = tm;
		celdasManager = cm;
		attackManager = am;
		
		resultadoDados = GetNode<Label>("Dado/NumeroDado");
		//recursos = GetNode<Control>("CantRecursos");
		feedback = GetNode<Label>("Feedback");
		feedbackEspectador = GetNode<Label>("FeedbackEspectador");
		botonDado = GetNode<TextureButton>("Dado/DadoButton");
		botonAtacar = GetNode<TextureButton>("Atacar/AtacarButton");
		botonColmena = GetNode<TextureButton>("Colmena/ColmenaButton");

		botonDado.Pressed += MostrarResultadoDado;
		botonAtacar.Pressed += OnAtacarPressed;
		botonColmena.Pressed += OnBotonColmenaPressed;

		if(GameManager.Instance.modoEspectador){
			feedback.Visible = false;
			feedbackEspectador. Visible = true;
		}
	}

	private void OnTirarDadoPressed(){	
		int resultado = GameManager.Instance.TirarDado();
		resultadoDados.Text = resultado.ToString();
	}

	public void DeshabilitarDado(){
		var jugador = GameManager.Instance.jugadorEnTurno;
		if (jugador == null || botonDado == null) return;

		bool yaTiro = jugador.TiroLosDados || jugador.Estado == AbejaReina.EstadoTurno.EsperandoAccion;
		bool esSuTurnoAhora = jugador.EsSuTurno && !yaTiro;
		bool esLocal = gameUI != null ? gameUI.EsTurnoLocal() : (!GameManager.Instance.modoMultiplayer || Multiplayer.GetUniqueId() == jugador.Id);
		botonDado.Disabled = !esSuTurnoAhora || !esLocal;
		}

	public void MostrarMovimientosDisponibles(){
		var jugador = GameManager.Instance.jugadorEnTurno;
		if(jugador != null){
			resultadoDados.Text = jugador.MovimientosDisponibles.ToString();
			}
	}

	private void MostrarResultadoDado(){
		var jugador = GameManager.Instance.jugadorEnTurno;
		if (botonDado.Disabled) return;

		bool esTurnoLocal = !GameManager.Instance.modoMultiplayer || Multiplayer.GetUniqueId() == jugador.Id;
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

			SolicitarActualizarUI();
		}
	}

	public void ActualizarInstrucciones(string texto){
		if(feedback != null && IsInstanceValid(feedback)){
			feedback.Text = texto;
		}
	}

	public void SolicitarActualizarUI() => OnUIActualizacionRequerida?.Invoke();

	public MenuRadial ObtenerMenuRadial(){
		if (instanciaMenuRadial == null){
			instanciaMenuRadial = menuRadialPrefab.Instantiate<MenuRadial>();
			AddChild(instanciaMenuRadial);
			instanciaMenuRadial.EstablecerHUD(this);
		}
		return instanciaMenuRadial;
	}

	private void OnBotonColmenaPressed(){
		if(!gameUI.EsTurnoLocal()) return;
		ObtenerMenuRadial().DesplegarEnPosicion(GetViewportRect().Size / 2);

		AlternarEstadoDeInvocacion();
		AlternarEstadoDeAbsorcion();
		AlternarEstadoDeControl();
		AlternarEstadoDeTransformacion();
	}

	private void OnAtacarPressed(){
		var jugador = GameManager.Instance.jugadorEnTurno;
		if(playerManager == null || !gameUI.EsTurnoLocal()) return;
		tocoBotonAtacarYDebeDesplegarseElMenu = true;
		ObtenerMenuRadial().DesplegarEnPosicion(GetViewportRect().Size / 2);
		AlternarEstadoDeAtaque();
		AudioManager.Instance.PlaySound(UiSound1);
		GD.Print("Se desplegó el menu de ataque. Modo ataque:" + jugador.ModoAtaque.ToString());
	}

	public void OnControlarPressed(){
		var jugador = GameManager.Instance.jugadorEnTurno;
		if(playerManager == null) return;

		if(!jugador.MenteColmena){		
			jugador.MenteColmena = true;
			tropasManager.MostrarAbejasCercanas();
		}
		else{
			if(jugador.MoviendoSubdito && tropasManager.SubditoActual != null){
				if(tropasManager.SubditoActual is AbejaPolinizadora) jugador.MovimientosDisponibles /= 2;
				if(tropasManager.SubditoActual is AbejaGuardiana) jugador.MovimientosDisponibles *= 2;
				playerManager.TerminarControlDeSubdito();
			}
			jugador.MenteColmena = false;
			tropasManager.OcultarAbejasCercanas();
		}
	}

	public void OnCurarPressed(){
		var jugador = GameManager.Instance.jugadorEnTurno;
		if(playerManager == null) return;

		if(!jugador.ModoAbsorcion){		
			jugador.ModoAbsorcion = true;
			if(tropasManager.PuedeAbsorberSubdito()){
				tropasManager.MostrarAbejasCercanas();
				GD.Print("Modo de absorcion = " + jugador.ModoAbsorcion);
			}
		} else {
			jugador.ModoAbsorcion = false;
			tropasManager.OcultarAbejasCercanas();
			GD.Print("Modo de absorcion = " + jugador.ModoAbsorcion);
		}
	}

	public void AtaqueAObjetivo(){
		var jugador = GameManager.Instance.jugadorEnTurno;
		if (!GameManager.Instance.PuedeAtacar()) return;

		if (jugador.MoviendoSubdito && tropasManager.SubditoActual is AbejaGuardiana){
			jugador.AtaqueAObjetivo = false;
			attackManager.AtacarConGuardiana();
			return;
		}

		bool subditoAdyacenteAJugador = (jugador.MenteColmena || jugador.MoviendoSubdito)
			&& tropasManager.SubditoActual != null
			&& GameManager.Instance.JugadoresEnPartida.Any(j => j != jugador && !j.FueraDeJuego && tropasManager.SubditoTieneAAlguienCerca(j));

		if (attackManager.TieneObjetivosCerca() || subditoAdyacenteAJugador){
			jugador.ModoAtaque = true;
			playerManager.vfxManager.MostrarJugadoresObjetivo();
			playerManager.vfxManager.MostrarAbejasObjetivo();
			GameManager.Instance.PotenciarAtaqueDeJugadorEnTurno();
			if (jugador.AtaquePotenciado){
				playerManager.celdasManager.PintarCeldaDeAbejaBuffeada();
			}
		} else {
			jugador.AtaqueAObjetivo = false;
		}
	}

	public void DestruirCeldasReforzadas(){
		var jugador = GameManager.Instance.jugadorEnTurno;
		var subdito = playerManager.tropasManager.SubditoActual;
		if(subdito is AbejaObrera obrera) obrera.ModoDestruccion = true;
		jugador.ModoAtaque = true;
		celdasManager.MostrarCeldasReforzadasCercanas(tropasManager.SubditoActual);
	}

	public void AlternarEstadoDeAtaque(){
		var jugador = GameManager.Instance.jugadorEnTurno;
		if (playerManager == null || instanciaMenuRadial == null || attackManager == null) return;

		bool modoAtaqueDisponible = !jugador.AccionConsumida
			&& !jugador.AtaqueConsumido
			&& !jugador.ModoInvocacion
			&& !jugador.ModoAbsorcion
			&& !jugador.ModoTransformacion;

		bool controlValido = !jugador.MenteColmena || (jugador.MoviendoSubdito && tropasManager.SubditoActual != null);

		bool puedeAtacar = GameManager.Instance.PuedeAtacar()
			&& modoAtaqueDisponible
			&& controlValido
			&& attackManager.TieneObjetivosCerca();

		instanciaMenuRadial.ActualizarEstadoBoton(puedeAtacar, "AtacarButton", instanciaMenuRadial.ContenedorDeAtaque);
	}

	private void AlternarEstadoDeComandar(){
		var jugador = GameManager.Instance.jugadorEnTurno;
		if (playerManager == null || instanciaMenuRadial == null || attackManager == null) return;

		bool modoAtaqueDisponible = !jugador.AccionConsumida
			&& !jugador.AtaqueConsumido
			&& !jugador.ModoInvocacion
			&& !jugador.ModoAbsorcion
			&& !jugador.ModoTransformacion;

		bool puedeComandar = GameManager.Instance.PuedeAtacar()
			&& modoAtaqueDisponible
			&& attackManager.SubditoTieneSubditoEnemigoCerca();

		instanciaMenuRadial.ActualizarEstadoBoton(puedeComandar, "ComandarButton", instanciaMenuRadial.ContenedorDeAtaque);
	}

	private void AlternarEstadoDeDestruccion(){
		var jugador = GameManager.Instance.jugadorEnTurno;
		if(playerManager == null || instanciaMenuRadial == null || attackManager == null) return;
		bool modoAtaqueDisponible = !jugador.AccionConsumida && !jugador.ModoInvocacion && !jugador.ModoAbsorcion;
		bool puedeDestruir = GameManager.Instance.PuedeAtacar()
			&& modoAtaqueDisponible 
			&& tropasManager.SubditoActual is AbejaObrera 
			&& celdasManager.ExistenCeldasReforzadasCerca(tropasManager.SubditoActual);

		instanciaMenuRadial.ActualizarEstadoBoton(puedeDestruir, "DestruirButton", instanciaMenuRadial.ContenedorDeAtaque);
	}

	private void AlternarEstadoDeInvocacion(){
		var jugador = GameManager.Instance.jugadorEnTurno;
		bool puedeInvocar = !jugador.AccionConsumida && !jugador.ModoAtaque && !jugador.ModoAbsorcion;;
		if (instanciaMenuRadial != null){
			instanciaMenuRadial.ActualizarEstadoBoton(puedeInvocar, "InvocarButton");
		}
	}

	private void AlternarEstadoDeControl(){
		if (playerManager == null || tropasManager == null || instanciaMenuRadial == null) return;
		var jugador = GameManager.Instance.jugadorEnTurno;
		bool tieneSubditosCerca = tropasManager.TieneSubditosCerca() || jugador.MenteColmena;
		bool puedeControlar = tieneSubditosCerca
			&& !jugador.AccionConsumida
			&& !jugador.ModoAtaque
			&& !jugador.ModoAbsorcion
			&& !jugador.ModoInvocacion;

		instanciaMenuRadial.ActualizarEstadoBoton(puedeControlar, "ControlarButton");
	}

	public void AlternarEstadoDeAbsorcion(){
		var jugador = GameManager.Instance.jugadorEnTurno;
		if (playerManager == null) return;
		bool puedeAbsorber = tropasManager.PuedeAbsorberSubdito() && !jugador.AccionConsumida && !jugador.ModoInvocacion && !jugador.ModoAtaque;
		if (instanciaMenuRadial != null){
			instanciaMenuRadial.ActualizarEstadoBoton(puedeAbsorber, "CurarButton");
		}
	}

	public void AlternarEstadoDeTransformacion(){
		var jugador = GameManager.Instance.jugadorEnTurno;
		if (playerManager == null) return;
		bool noEstaEnOtroModo = !jugador.ModoInvocacion && !jugador.ModoAtaque && !jugador.ModoAbsorcion;
		bool puedeTransformar = tropasManager.ExistenSubditosTransformables() && !jugador.AccionConsumida && noEstaEnOtroModo;
		if (instanciaMenuRadial != null){
			instanciaMenuRadial.ActualizarEstadoBoton(puedeTransformar, "TransformarButton");
		}
	}

	public void InvocarSubdito(){
		var jugador = GameManager.Instance.jugadorEnTurno;
		if(jugador.TiroLosDados && !jugador.AccionConsumida 
			&& !jugador.ModoInvocacion && !jugador.ModoAtaque){	
			jugador.ModoInvocacion = true;

			AlternarEstadoDeAtaque();
			AlternarEstadoDeAbsorcion();
			celdasManager.MostrarCeldasDisponiblesParaInvocar();
		}else if(jugador.TiroLosDados && jugador.ModoInvocacion){
			jugador.ModoInvocacion = false;

			AlternarEstadoDeAtaque();
			AlternarEstadoDeAbsorcion();
			celdasManager.OcultarCeldasDisponiblesParaInvocar();
		}

		AudioManager.Instance.PlaySound(UiSound1);
	}

	public void ActualizarUIAtaques(){
		AlternarEstadoDeAtaque();
		AlternarEstadoDeComandar();
		AlternarEstadoDeDestruccion();
	}

	public void ComandarAtaque(){
		if(playerManager == null || attackManager == null || !gameUI.EsTurnoLocal()) return;
		attackManager.ComandarAtaque();
	}

	private void AlternarComandoDeAtaque(){
		var jugador = GameManager.Instance.jugadorEnTurno;
		if(playerManager == null || instanciaMenuRadial == null || attackManager == null) return;
		bool modoAtaqueDisponible = !jugador.AccionConsumida && !jugador.ModoInvocacion && !jugador.ModoAbsorcion;
		bool puedeComandar = GameManager.Instance.PuedeAtacar() && attackManager.SubditoTieneSubditoEnemigoCerca() && modoAtaqueDisponible;
		instanciaMenuRadial.ActualizarEstadoBoton(puedeComandar, "ComandarButton");
	}

	public void ValidarTurnoLocalUI(){
		bool esNuestroTurno = gameUI.EsTurnoLocal();
		var jugador = GameManager.Instance.jugadorEnTurno;
		DeshabilitarDado();
		bool turnoHabilitado = esNuestroTurno && jugador != null && jugador.TiroLosDados;
		if (botonAtacar != null) botonAtacar.Disabled = !turnoHabilitado;
		if (botonColmena != null) botonColmena.Disabled = !turnoHabilitado;
		if (!esNuestroTurno && instanciaMenuRadial != null && instanciaMenuRadial.Activo){
			instanciaMenuRadial.OcultarMenu();
			if (instanciaMenuRadial.ContenedorDeTransformaciones != null)
				instanciaMenuRadial.OcultarTransformaciones();
		}
	}
}
