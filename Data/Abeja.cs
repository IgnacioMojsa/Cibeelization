using Godot;
using System;

public partial class Abeja : Node3D{
	public int HP { get; set; } = 5;

	public override void _Input(InputEvent @event){
		if (@event is not InputEventMouseButton mouse ||
            mouse.ButtonIndex != MouseButton.Left ||
            !mouse.Pressed)
            return;
			GD.Print("Hiciste click");
	}

}
