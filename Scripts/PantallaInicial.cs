using Godot;
using System;

public partial class PantallaInicial : Control
{
	private void _on_play_button_pressed(){
		GD.Print("El botón play ha sido presionado");
		GetTree().ChangeSceneToFile("res://Scenes/escenaPrueba.tscn");
	}
	
	private void _on_exit_button_pressed(){
		GetTree().Quit();
	}
};
