using Godot;
public partial class UIPantallaDeInicio : GameUI {
	private void Jugar(){
		PanelContainer UIComienzo = GetNode<PanelContainer>("MenuComienzo");
		AudioManager.Instance.PlaySound(UiSound1);

		GD.Print("El botón play ha sido presionado");

		UIComienzo.Visible = true;
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
		if (GameManager.Instance.PartidaActual != null && (reinicioConfirmado || hayJugadorVictorioso))
			return GameManager.Instance.PartidaActual.SizeTablero;
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

	private void SalirDelJuego(){
		AudioManager.Instance.PlaySound(UiSound1);
		GetTree().Quit();
	}

	private void Creditos(){
		PanelContainer UICreditos = GetNode<PanelContainer>("PantallaCreditos");
		AudioManager.Instance.PlaySound(UiSound1);
		UICreditos.Visible = true;
	}

	private void CerrarCreditos(){
		PanelContainer UICreditos = GetNode<PanelContainer>("PantallaCreditos");
		AudioManager.Instance.PlaySound(UiSound1);
		UICreditos.Visible = false;
	}

	private void MostrarTutorial(){
		PanelContainer UITutorial = GetNode<PanelContainer>("Tutorial");
		AudioManager.Instance.PlaySound(UiSound1);
		UITutorial.Visible = true;
	}

	private void OcultarTutorial(){
		PanelContainer UITutorial = GetNode<PanelContainer>("Tutorial");
		AudioManager.Instance.PlaySound(UiSound1);
		UITutorial.Visible = false;
	}
}
