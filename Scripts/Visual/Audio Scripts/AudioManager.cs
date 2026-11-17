using System.Collections.Generic;
using Godot;

public partial class AudioManager : Node
{
	public static AudioManager Instance { get; private set; }
	[Export] public GameAudio GameAudio {get; set;}
	private AudioStreamPlayer sfx_stream;
	private AudioStreamPlayer music_stream;
	private List<AudioStreamPlayer> sfx_pool = new();
	private const int poolSize = 3; //cantidad maxima de sonidos que pueden reproducirse a la vez

	public override void _Ready()
	{
		Instance = this;

		for (int i = 0; i < poolSize; i++)
        {
            CrearNuevoSfxPlayer();
        }

		sfx_stream = new AudioStreamPlayer();
		AddChild(sfx_stream);

		// music_stream = new AudioStreamPlayer();
		// AddChild(music_stream);
	}

	public override void _Process(double delta)
	{
	}
	
	public void PlaySound(AudioSetting setting)
	{
		var sfx_stream = sfx_pool.Find(p => !p.Playing);
		if(sfx_stream == null) sfx_stream = CrearNuevoSfxPlayer();

		sfx_stream.Stream = setting.Source;
		sfx_stream.VolumeDb = setting.Volume_db;
		//sfx_stream.Bus = "SFX";
		sfx_stream.Play();
	}

	public void PlayMusic(AudioSetting setting)
	{
		if(music_stream.Playing)
			return;

		music_stream.Stream = setting.Source;
		music_stream.VolumeDb = setting.Volume_db;
		music_stream.Play();
	}

	public void StopMusic()
	{
		music_stream.Stop();
	}

	private AudioStreamPlayer CrearNuevoSfxPlayer()
    {
        var player = new AudioStreamPlayer { Bus = "SFX" };
        AddChild(player);
        sfx_pool.Add(player);
        return player;
    }

}
