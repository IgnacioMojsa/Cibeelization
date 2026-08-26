using Godot;
using System;

public partial class GameUI : Control
{
	private void ComenzarPartida(){
		GD.Print("El botón play ha sido presionado");
		GetTree().ChangeSceneToFile("res://Scenes/escenaPrueba.tscn");
	}
	
	private void SalirDelJuego(){
		GetTree().Quit();
	}
};
