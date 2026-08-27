using Godot;
using System;

public partial class GameUI : Control
{
	private Label resultadoDados;

	public override void _Ready(){
		resultadoDados = GetNode<Label>("HBoxContainer/NumeroDado/MarginContainer/Label");
	}

	private void ComenzarPartida(){
		GD.Print("El botón play ha sido presionado");
		GetTree().ChangeSceneToFile("res://Scenes/escenaPrueba.tscn");
	}
	
	private void SalirDelJuego(){
		GetTree().Quit();
	}

	private void TirarLosDados(){
		var gameManager = new GameManager();

		resultadoDados.Text = gameManager.TirarDado().ToString();
	}
};
