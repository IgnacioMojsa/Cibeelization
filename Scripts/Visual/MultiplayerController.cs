using Godot;

public partial class MultiplayerController : Control
{
	[Export] private int port = 8910;
	[Export] private string adress = "127.0.0.1";
	private ENetMultiplayerPeer peer;

	public Button botonHost;
	public Button botonCliente;

	public override void _Ready()
	{
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;
		
		botonHost =	GetNode<Button>("Host");
		botonCliente = GetNode<Button>("Cliente");

		if(!GameManager.Instance.jugadorEsHost)
		{
			botonHost.Visible = false;
		}
		else
		{
			botonCliente.Visible = false;
		}
	}

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
		RpcId(1, "sendPlayerInformation", GetNode<LineEdit>("LineEdit").Text, Multiplayer.GetUniqueId());
	}

	public void ConnectionFailed()
	{
		GD.Print("CONNECTION FAILED");
	}

	public void _on_host_button_down()
	{
		botonHost.Disabled = true;
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
		SendPlayerInformation(GetNode<LineEdit>("LineEdit").Text, 1);
	}

	public void _on_cliente_button_down()
	{
		botonCliente.Disabled = true;
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
		foreach (var item in GameManager.Instance.JugadoresEnPartida)
		{
			GD.Print(item.Id + "is playing");
		}
		var scene = ResourceLoader.Load<PackedScene>("res://Scenes/escenaPrueba.tscn").Instantiate<Node3D>();
		GetTree().Root.AddChild(scene);
		this.Hide();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]

	private void SendPlayerInformation(string name, int id)
	{
		AbejaReina infoJugadores = new AbejaReina(id)
		{
			Name = name,
			Id = id
		};

		if(!GameManager.Instance.JugadoresEnPartida.Contains(infoJugadores))
		{
			GameManager.Instance.JugadoresEnPartida.Add(infoJugadores);
		}

		if(Multiplayer.IsServer()){
			foreach (var item in GameManager.Instance.JugadoresEnPartida)
			{
				Rpc("sendPlayerInformation", name, id);
			}
		}
	}
}
