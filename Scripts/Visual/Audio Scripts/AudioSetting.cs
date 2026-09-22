using Godot;

//Acá está la configuracion del audio para la interfaz

[GlobalClass]
public partial class AudioSetting : Resource
{
	[Export] public AudioStream Source { get; set; }

	[Export] public float Volume_db { get; set; } = 0.0f;

}
