using Godot;
using System.Collections.Generic;
public partial class UIGeneral : Control {

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
	public AudioSetting UiSound1 => AudioManager.Instance?.GameAudio?.Sound1;
	public AudioSetting UiSound4 => AudioManager.Instance?.GameAudio?.Sound4;
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

	public void CambiarMenu(Control menuAOcultar, Control menuAMostrar){
		menuAOcultar.Visible = false;
		menuAMostrar.Visible = true;
		AudioManager.Instance.PlaySound(UiSound1);
	}

	public void ActualizarInstrucciones(string texto){
		if(feedback != null){
		feedback.Text = texto;
		}
	}

	public void PausarPartida(){
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

	public void MostrarPantallaDeVictoria(){
		hayJugadorVictorioso = true;
		MostrarMensajeVictoria();
		UIVictoria();
	}

	public void MostrarMensajeVictoria(){
		var mensajeVictoria = GetNode<Label>("Titulo");
		mensajeVictoria.Text = "EL  JUGADOR   " + GameManager.Instance.JugadorGanador.Id + "   ES  EL  GANADOR";
	}

	public bool EsTurnoLocal(){
		if (!GameManager.Instance.modoMultiplayer) return true;
		var jugador = GameManager.Instance.jugadorEnTurno;
		if (jugador == null) return false;
		return Multiplayer.GetUniqueId() == jugador.Id;
	}

	public void ValidarTurnoLocalUI(){
		bool esNuestroTurno = EsTurnoLocal();
		if (botonAtacar != null) botonAtacar.Disabled = !esNuestroTurno || botonAtacar.Disabled;
		if (botonColmena != null) botonColmena.Disabled = !esNuestroTurno;
		if (!esNuestroTurno && instanciaMenuRadial != null && instanciaMenuRadial.Activo){
			instanciaMenuRadial.OcultarMenu();
			if (instanciaMenuRadial.ContenedorDeTransformaciones != null)
				instanciaMenuRadial.OcultarTransformaciones();
		}
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
}