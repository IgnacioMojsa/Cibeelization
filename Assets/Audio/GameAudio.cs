using Godot;
using System;

//Acá se crean las propiedades visibles dentro del editor de godot

[GlobalClass]
public partial class GameAudio : Resource
{
	[Export] public AudioSetting Sound1{get; set;}

	[Export] public AudioSetting Sound2{get; set;}

	[Export] public AudioSetting Sound3{get; set;}

	[Export] public AudioSetting Sound4{get;set;}

	[Export] public AudioSetting Sound5{get;set;}
}
