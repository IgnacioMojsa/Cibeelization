using Godot;
using System.Collections.Generic;
public partial class UIPantallaDeInicio : GameUI {

	private PanelContainer uiComienzo;
	private PanelContainer uiCreditos;
	private PanelContainer uiTutorial;
	private PanelContainer uiAjustes;

	public override void _Ready(){
		base._Ready();

		uiComienzo = GetNode<PanelContainer>("MenuComienzo");
		uiCreditos = GetNode<PanelContainer>("PantallaCreditos");
		uiTutorial = GetNode<PanelContainer>("Tutorial");
		uiAjustes = GetNode<PanelContainer>("Ajustes");
	}

	public void MostrarUIPanel(Control panel, bool mostrar)
	{
		if (panel == null) return;
		panel.Visible = mostrar;
		AudioManager.Instance.PlaySound(UiSound1);
	}

	private void Jugar(){
		MostrarUIPanel(uiComienzo, true);
		GD.Print("El botón play ha sido presionado");
	}

	private void ComenzarPartida(){
		int jugadores = ObtenerCantJugadores();
		int size = ObtenerSizeTablero();
	
		GameManager.Instance.IniciarPartida(jugadores, size);

		GD.Print("La partida se desarrollará con " + GameManager.Instance.cantidadJugadores + " jugadores");
		GD.Print("Opción de tamaño seleccionada: " + GameManager.Instance.sizeTablero);

		AudioManager.Instance.PlaySound(UiSound1);

		reinicioConfirmado = false;
		hayJugadorVictorioso = false;

		GetTree().ChangeSceneToFile("res://Scenes/escenaPrueba.tscn");
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
		MostrarUIPanel(uiCreditos, true);
	}

	private void CerrarCreditos(){
		MostrarUIPanel(uiCreditos, false);
	}

	private void MostrarTutorial(){
		MostrarUIPanel(uiTutorial, true);
	}

	private void OcultarTutorial(){
		MostrarUIPanel(uiTutorial, false);
	}
}
