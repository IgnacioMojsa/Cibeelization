using Godot;
using System;
using System.Data;

//https://www.youtube.com/watch?v=MOJVjFngOs4&t=1154s

public partial class MultiplayerController : Control
{
	[Export] private int port = 8910;
	[Export] private string adress = "127.0.0.1";
	private ENetMultiplayerPeer peer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void PeerConnected(long id)
	{
		GD.Print("PLAYER CONNECTED: " + id.ToString());
	}

	public void PeerDisconnected(long id)
	{
		GD.Print("PLAYER DISCONNECTED: " + id.ToString());
	}

	public void ConnectedToServer()
	{
		GD.Print("CONNECTED TO SERVER");
	}

	public void ConnectionFailed()
	{
		GD.Print("CONNECTION FAILED");
	}

	public void _on_host_button_down()
	{
		peer = new ENetMultiplayerPeer();
		var error = peer.CreateServer(port, 4);
		if(error != Error.Ok)
		{
			GD.Print("Error cannot host: " + error.ToString());
			return;
		}
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Waiting for players");
	}

	public void _on_cliente_button_down()
	{
		peer = new ENetMultiplayerPeer();
		peer.CreateClient(adress, port);
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Joining game");
	}

	public void _on_jugar_button_down()
	{
		Rpc("startGame");
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void startGame()
	{
		var scene = ResourceLoader.Load<PackedScene>("res://Scenes/escenaPrueba.tscn").Instantiate<Node3D>();
		GetTree().Root.AddChild(scene);
		this.Hide();
	}

}
