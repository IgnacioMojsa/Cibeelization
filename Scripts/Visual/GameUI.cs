using Godot;
using System.Collections.Generic;

public partial class GameUI : Control
{
	[Export] private PlayerManager playerManager;
	private Label resultadoDados;
	private Button botonDado;
	private Button botonAtacar;

	
	public override void _Ready(){
		resultadoDados = GetNode<Label>("HBoxContainer/NumeroDado/MarginContainer/Label");
		botonDado = GetNode<Button>("HBoxContainer/TirarDado/TirarDadoButton");

		botonAtacar = GetNode<Button>("Atacar/AtacarButton");

		botonAtacar.Pressed += OnAtacarPressed;

		MostrarDataDeJugadores(); 
	}

	public override void _Process(double delta){
		if(GetTree().CurrentScene.SceneFilePath == "res://Scenes/escenaPrueba.tscn"){
			MostrarJugadorEnTurno();
		}
	}

	private void Jugar(){
		PanelContainer UIComienzo = GetNode<PanelContainer>("MenuComienzo");

		GD.Print("El botón play ha sido presionado");

		UIComienzo.Visible = true;
	}

	private void ComenzarPartida(){
		GameManager.Instance.cantidadJugadores = ObtenerCantJugadores();
		GameManager.Instance.CargarJugadores(GameManager.Instance.cantidadJugadores);
		GameManager.Instance.TurnManager.EstablecerPrimerTurno();

		GD.Print("La partida se desarrollara con " + GameManager.Instance.cantidadJugadores + " jugadores");		
		GD.Print("Comienza el jugador " + GameManager.Instance.jugadorEnTurno.Id);		

		GetTree().ChangeSceneToFile("res://Scenes/escenaPrueba.tscn");
	}
	
	private void SeleccionarCantidadDeJugadores(bool estaPresionado){
		if (!estaPresionado) return;
		
		if (estaPresionado){
			GameManager.Instance.cantidadJugadores = ObtenerCantJugadores();
			GD.Print($"Cantidad de jugadores seleccionada: {GameManager.Instance.cantidadJugadores}");
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

	public void MostrarDataDeJugadores(){
		var jugador1 = GetNode<PanelContainer>("VBoxContainer/Jugador1");
		var jugador2 = GetNode<PanelContainer>("VBoxContainer/Jugador2");
		var jugador3 = GetNode<PanelContainer>("VBoxContainer/Jugador3");
		var jugador4 = GetNode<PanelContainer>("VBoxContainer/Jugador4");

		if(GameManager.Instance.cantidadJugadores == 3){
			jugador3.Visible = true;
		}
		else if(GameManager.Instance.cantidadJugadores == 4){
			jugador3.Visible = true;
			jugador4.Visible = true;
		}
	}

	private void SalirDelJuego(){
		GetTree().Quit();
	}

	private void OnTirarDadoPressed()
	{	
		int resultado = GameManager.Instance.TirarDado();
		resultadoDados.Text = resultado.ToString();
	}

	public void MostrarResultadoDado(){
		resultadoDados.Text = GameManager.Instance.TirarDado().ToString();

		//botonDado.Disabled = true;
	} 

	private void MostrarJugadorEnTurno(){ 
		AbejaReina jugadorEnTurno = GameManager.Instance.jugadorEnTurno;

		var jugador1 = GetNode<PanelContainer>("VBoxContainer/Jugador1");
		var jugador2 = GetNode<PanelContainer>("VBoxContainer/Jugador2");
		var jugador3 = GetNode<PanelContainer>("VBoxContainer/Jugador3");
		var jugador4 = GetNode<PanelContainer>("VBoxContainer/Jugador4");

		List<PanelContainer> UIJugadores = new List<PanelContainer>{ jugador1, jugador2, jugador3, jugador4};
		
		for (int j = 0; j < GameManager.Instance.cantidadJugadores; j++)
		{
			if( GameManager.Instance.JugadoresEnPartida[j] == jugadorEnTurno){
				UIJugadores[j].Modulate = Color.FromHtml("#ffd01f");
			}
			else{
				UIJugadores[j].Modulate = Color.FromHtml("#ffffff");
			}
		}
		
		//ffd01f
	}

	private void OnAtacarPressed()
	{
		if(playerManager != null)
		playerManager.Atacar();
	}
};
