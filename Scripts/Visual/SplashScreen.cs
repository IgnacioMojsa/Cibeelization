using Godot;
using System;

public partial class SplashScreen : Node2D
{
    public AnimationPlayer animacion;

    public override void _Ready()
    {
        animacion = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public override void _Process(double delta)
    {
        if(!animacion.IsPlaying()) GetTree().ChangeSceneToFile("res://Scenes/pantallaInicial.tscn");
    }
}
