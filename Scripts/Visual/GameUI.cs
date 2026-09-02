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
			MostrarHPDeJugaores();
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

		var outlineJ1 = playerManager.VisualesJugadores[0].GetNode<Node3D>("Esqueleto/Outline");
		var outlineJ2 = playerManager.VisualesJugadores[1].GetNode<Node3D>("Outline");
		var outlineJ3 = playerManager.VisualesJugadores[2].GetNode<Node3D>("Esqueleto/Outline");
		var outlineJ4 = playerManager.VisualesJugadores[3].GetNode<Node3D>("Outline");

		List<PanelContainer> UIJugadores = new List<PanelContainer>{ jugador1, jugador2, jugador3, jugador4};
		
		List<Node3D> ContornoJugadores = new List<Node3D>{ outlineJ1, outlineJ2, outlineJ3, outlineJ4};
		
		for (int j = 0; j < GameManager.Instance.cantidadJugadores; j++)
		{
			if( GameManager.Instance.JugadoresEnPartida[j] == jugadorEnTurno){
				UIJugadores[j].Modulate = Color.FromHtml("#ff0000");
				ContornoJugadores[j].Visible = true;
			}
			else{
				UIJugadores[j].Modulate = Color.FromHtml("#ffffff");
				ContornoJugadores[j].Visible = false;
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
