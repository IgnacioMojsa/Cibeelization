using Godot;
using System.Collections.Generic;
public partial class UIPantallaDeInicio : GameUI {
	
	private PanelContainer uiModos;
	private PanelContainer uiOpcionesOnline;
	private PanelContainer uiComienzo;
	private PanelContainer uiCreditos;
	private PanelContainer uiTutorial;
	private PanelContainer uiAjustes;
	private List<Button> BotonesDeInicio; 

	public override void _Ready(){
		base._Ready();
		
		uiModos = GetNode<PanelContainer>("ElegirModo");
		uiOpcionesOnline = GetNode<PanelContainer>("OpcionesModoOnline");
		uiComienzo = GetNode<PanelContainer>("MenuComienzo");
		uiCreditos = GetNode<PanelContainer>("PantallaCreditos");
		uiTutorial = GetNode<PanelContainer>("Tutorial");
		uiAjustes = GetNode<PanelContainer>("Ajustes");

		BotonesDeInicio = new List<Button>{
				GetNode<Button>("BotonesMenuDeInicio/Play/PlayButton"),
				GetNode<Button>("BotonesMenuDeInicio/Tutorial/TutorialButton"),
				GetNode<Button>("BotonesMenuDeInicio/Settings/SettingsButton"),
				GetNode<Button>("BotonesMenuDeInicio/Credits/CreditsButton"),
				GetNode<Button>("BotonesMenuDeInicio/Exit/ExitButton"),
			};
	}

	public void MostrarUIPanel(Control panel, bool mostrar)
	{
		if (panel == null) return;
		panel.Visible = mostrar;
		AudioManager.Instance.PlaySound(UiSound1);
	}
	
	private void ConfigurarBoton(string nombre, bool? Estado = null){
		foreach (var boton in BotonesDeInicio){
			if (boton.Name == nombre) continue;
			boton.Disabled = Estado ?? !boton.Disabled;
		}
	}

	public void ElegirModo(){
		MostrarUIPanel(uiModos, !uiModos.Visible);
		MostrarUIPanel(uiComienzo, false);
		ConfigurarBoton("PlayButton");
	}

	private void MostrarOpcionesModoOnline()
	{
		GameManager.Instance.modoMultiplayer = true;
		MostrarUIPanel(uiOpcionesOnline, true);
		MostrarUIPanel(uiComienzo, false);
	}

	private void Jugar(){
		MostrarUIPanel(uiOpcionesOnline, false);
		MostrarUIPanel(uiModos, false);
		MostrarUIPanel(uiComienzo, true);

		GD.Print("El botón play ha sido presionado");
	}

	private void ComenzarPartida(){
		int jugadores = ObtenerCantJugadores();
		int size = ObtenerSizeTablero();
		reinicioConfirmado = false;
		hayJugadorVictorioso = false;

		if (!GameManager.Instance.modoMultiplayer)
		{
			ComenzarPartidaLocal(jugadores, size);
		}
		else
		{
			ComenzarPartidaMultijugador(jugadores, size);
		}
	}

	private void ComenzarPartidaLocal(int jugadores, int size)
	{
		GameManager.Instance.IniciarPartida(jugadores, size);
		GD.Print("La partida se desarrollará con " + GameManager.Instance.cantidadJugadores + " jugadores");
		GD.Print("Opción de tamaño seleccionada: " + GameManager.Instance.sizeTablero);
		AudioManager.Instance.PlaySound(UiSound1);
		GetTree().ChangeSceneToFile("res://Scenes/escenaPrueba.tscn");
		GameManager.Instance.jugadorEsHost = true;
	}

	private void ComenzarPartidaMultijugador(int jugadores, int size)
	{
		GameManager.Instance.cantidadJugadores = jugadores;
		GameManager.Instance.sizeTablero = size;
		GD.Print("Se creó una partida MILTIJUGADOR para " + GameManager.Instance.cantidadJugadores + " jugadores");
		GD.Print("Tamaño del tablero elegido: " + GameManager.Instance.sizeTablero);
		AudioManager.Instance.PlaySound(UiSound1);
		GetTree().ChangeSceneToFile("res://Scenes/lobby.tscn");

	}

	private void EnviarJugadorAlLobby()
	{
		GameManager.Instance.jugadorEsHost = false;
		GetTree().ChangeSceneToFile("res://Scenes/lobby.tscn");
	}

	private void SeleccionarCantidadDeJugadores(bool estaPresionado){
		// Solo actuamos cuando el botón pasa a estado presionado (true)
		if (!estaPresionado) return;
		GameManager.Instance.cantidadJugadores = ObtenerCantJugadores();
		GD.Print($"Cantidad de jugadores seleccionada: {GameManager.Instance.cantidadJugadores}");
	}

	private void SeleccionarSizeTablero(bool estaPresionado){
		// Solo actuamos cuando el botón pasa a estado presionado (true)
		if (!estaPresionado) return;
		GameManager.Instance.sizeTablero = ObtenerSizeTablero();
		GD.Print($"Size del tablero seleccionado: {GameManager.Instance.sizeTablero}");
	}

	private int ObtenerCantJugadores(){
		if (GameManager.Instance.PartidaActual != null && (reinicioConfirmado || hayJugadorVictorioso))
			return GameManager.Instance.PartidaActual.CantidadJugadores;
		
		var check3 = GetNode<CheckBox>("MenuComienzo/MarginContainer/VBoxContainer/VBoxContainer/3Players/CheckBox");
		var check4 = GetNode<CheckBox>("MenuComienzo/MarginContainer/VBoxContainer/VBoxContainer/4Players/CheckBox");
		
		if(check3 != null && check3.ButtonPressed) return 3;
		if(check4 != null && check4.ButtonPressed) return 4;
		return 2;
	}

	private int ObtenerSizeTablero(){
		if (GameManager.Instance.PartidaActual != null && (reinicioConfirmado || hayJugadorVictorioso))
			return GameManager.Instance.PartidaActual.SizeTablero;
		
		var check3 = GetNode<CheckBox>("MenuComienzo/MarginContainer/VBoxContainer/Sizes/Mid/CheckBox");
		var check4 = GetNode<CheckBox>("MenuComienzo/MarginContainer/VBoxContainer/Sizes/Big/CheckBox");
		
		if(check3 != null && check3.ButtonPressed) return 3;
		if(check4 != null && check4.ButtonPressed) return 4;
		return 2;
	}

	private void SalirDelJuego(){
		AudioManager.Instance.PlaySound(UiSound1);
		GetTree().Quit();
	}

	private void Creditos(){
		MostrarUIPanel(uiCreditos, !uiCreditos.Visible);
		ConfigurarBoton("CreditsButton");
	}

	private void CerrarCreditos(){
		MostrarUIPanel(uiCreditos, false);
		ConfigurarBoton("CreditsButton", false);
	}

	private void MostrarTutorial(){
		MostrarUIPanel(uiTutorial, true);
	}

	private void OcultarTutorial(){
		MostrarUIPanel(uiTutorial, false);
	}

	private void Ajustes()
	{
		MostrarUIPanel(uiAjustes, !uiAjustes.Visible);
		ConfigurarBoton("SettingsButton");
	}

	private void AceptarConfigAudio()
	{
		MostrarUIPanel(uiAjustes, false);
		ConfigurarBoton("SettingButton", false);
	}
}