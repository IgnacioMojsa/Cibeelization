using Godot;
using System;

//Estos son los métodos de reproducción de audio
//Singleton
public partial class AudioManager : Node
{
	public static AudioManager Instance { get; private set; }
	[Export] public GameAudio GameAudio {get; set;}
	private AudioStreamPlayer sfx_stream;
	private AudioStreamPlayer music_stream;

	public override void _Ready()
	{
		Instance = this;

		sfx_stream = new AudioStreamPlayer();
		AddChild(sfx_stream);

		music_stream = new AudioStreamPlayer();
		AddChild(music_stream);
	}

	public override void _Process(double delta)
	{
	}
	
	public void PlaySound(AudioSetting setting)
	{
		sfx_stream.Stream = setting.Source;
		sfx_stream.VolumeDb = setting.Volume_db;
		sfx_stream.Play();
	}

	public void PlayMusic(AudioSetting setting)
	{
		if (music_stream.Playing)
			return;

		music_stream.Stream = setting.Source;
		music_stream.VolumeDb = setting.Volume_db;
		music_stream.Play();
	}

	public void StopMusic()
	{
		music_stream.Stop();
	}

}
