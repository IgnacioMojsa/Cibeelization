using Godot;

[GlobalClass]
public partial class GameAudio : Resource
{
	[Export] public AudioSetting Sound1{get; set;} //ClickSound
	[Export] public AudioSetting Sound2{get; set;} //PopSound
	[Export] public AudioSetting Sound3{get; set;} //PunchSound
	[Export] public AudioSetting Sound4{get;set;} //DiceSound
	[Export] public AudioSetting Sound5{get;set;} //BeeSound1
	[Export] public AudioSetting Sound6{get;set;} //BeeSound2
	[Export] public AudioSetting Sound7{get;set;} //VictorySound
	[Export] public AudioSetting Sound8{get;set;} //RecompensaSound1
	[Export] public AudioSetting Sound9{get;set;} //RecompensaSound2
	[Export] public AudioSetting Sound10{get;set;} //RecompensaSound3
}
