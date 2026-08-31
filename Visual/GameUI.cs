using Godot;
using System;

public partial class GameUI : Control
{
	private Label resultadoDados;
	private Button botonDado;
	public int cantidadJugadores {get; set;}

	public override void _Ready(){
		resultadoDados = GetNode<Label>("HBoxContainer/NumeroDado/MarginContainer/Label");
		botonDado = GetNode<Button>("HBoxContainer/TirarDado/TirarDadoButton");
	}

	private void Jugar(){
		PanelContainer UIComienzo = GetNode<PanelContainer>("MenuComienzo");

		GD.Print("El botón play ha sido presionado");

		UIComienzo.Visible = true;
	}

	private void ComenzarPartida(){
		int cantidadJugadores = ObtenerCantJugadores();
		GameManager.Instance.CargarJugadores(cantidadJugadores);
		GameManager.Instance.EstablecerPrimerTurno();
		
		GD.Print("La partida se desarrollara con " + GameManager.Instance.JugadoresEnPartida.Count + " jugadores");		
		GD.Print("Comienza el jugador " + GameManager.Instance.jugadorEnTurno.Id);		

		GetTree().ChangeSceneToFile("res://Scenes/escenaPrueba.tscn");
	}
	
	private void SeleccionarCantidadDeJugadores(bool estaPresionado){
		if (!estaPresionado) return;
		
		if (estaPresionado){
        	cantidadJugadores = ObtenerCantJugadores();
        	GD.Print($"Cantidad de jugadores seleccionada: {cantidadJugadores}");
        }
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

	private void SalirDelJuego(){
		GetTree().Quit();
	}

	private void MostrarResultadoDado(){
		resultadoDados.Text = GameManager.Instance.TirarDado().ToString();

		botonDado.Disabled = true;
	} 
};
