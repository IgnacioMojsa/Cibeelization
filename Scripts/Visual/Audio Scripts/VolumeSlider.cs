using Godot;

public partial class VolumeSlider : HSlider
{
	[Export] public string BusName { get; set; }
	private int busIndex;

	public override void _Ready()
	{
		busIndex = AudioServer.GetBusIndex(BusName);
		ValueChanged += OnValueChanged;
		Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(busIndex));
	}

	private void OnValueChanged(double value)
	{
		AudioServer.SetBusVolumeDb(busIndex, (float)Mathf.LinearToDb(value));
	}
}
