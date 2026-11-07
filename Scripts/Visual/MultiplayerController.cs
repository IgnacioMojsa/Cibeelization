using Godot;

public partial class MultiplayerController : Node
{
	[Export] private int port = 8910;
	[Export] private string adress = "127.0.0.1";
	private ENetMultiplayerPeer peer;
	public Button botonHost;
	public Button botonCliente;
	public Button botonJugar;
	private bool estaConectadoAlServidor = false;

	public override void _Ready(){
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;

		ConfigurarBotones();
	}

	private void ConfigurarBotones(){
		botonHost =	GetNode<Button>("Host");
		botonCliente = GetNode<Button>("Cliente");		
		botonJugar = GetNode<Button>("Jugar");

		if(!GameManager.Instance.jugadorEsHost){
			botonHost.Visible = false;
		} else {
			botonCliente.Visible = false;
		}
		ActualizarEstadoBotonJugar();
	}

	private void ActualizarEstadoBotonJugar(){
		if (botonJugar == null) return;

		if (GameManager.Instance.jugadorEsHost){
			bool hayClientesConectados = GameManager.Instance.JugadoresEnPartida.Count > 1;
			botonJugar.Disabled = !hayClientesConectados;
		} else {
			botonJugar.Disabled = !estaConectadoAlServidor;
			botonJugar.Visible = false;
		}
	}

	private void PeerConnected(long id){
		GD.Print("PLAYER CONNECTED: " + id.ToString());
	}

	private void PeerDisconnected(long id){
		GD.Print("PLAYER DISCONNECTED: " + id.ToString());
	}

	private void ConnectedToServer(){
		GD.Print("CONNECTED TO SERVER");
		estaConectadoAlServidor = true;
		ActualizarEstadoBotonJugar();
		RpcId(1, nameof(SendPlayerInformation), GetNode<LineEdit>("LineEdit").Text, Multiplayer.GetUniqueId());
	}

	private void ConnectionFailed(){
		GD.Print("CONNECTION FAILED");
		ActualizarEstadoBotonJugar();
	}

	private void _on_host_button_down(){
		botonHost.Disabled = true;
		peer = new ENetMultiplayerPeer();
		var error = peer.CreateServer(port, 4);
		if(error != Error.Ok){
			GD.Print("Error cannot host: " + error.ToString());
			return;
		}
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Waiting for players");
		SendPlayerInformation(GetNode<LineEdit>("LineEdit").Text, 1);
		ActualizarEstadoBotonJugar();
	}

	private void _on_cliente_button_down(){
		botonCliente.Disabled = true;
		peer = new ENetMultiplayerPeer();
		peer.CreateClient(adress, port);
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Joining game");
	}

	private void _on_jugar_button_down(){
		Rpc(nameof(startGame));
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void startGame(){
		GameManager.Instance.cantidadJugadores = GameManager.Instance.JugadoresEnPartida.Count;
		if(GameManager.Instance.TurnManager == null && GameManager.Instance.JugadoresEnPartida.Count > 0){
			GameManager.Instance.TurnManager = new TurnManager(GameManager.Instance.JugadoresEnPartida);
			GameManager.Instance.TurnManager.EstablecerPrimerTurno();
			GameManager.Instance.jugadorEnTurno = GameManager.Instance.TurnManager.jugadorEnTurno;
		}

		// foreach (var item in GameManager.Instance.JugadoresEnPartida)
		// {
		// 	GD.Print(item.Id + "is playing");
		// }

		GetTree().ChangeSceneToFile("res://Scenes/escenaPrueba.tscn");
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]

	private void SendPlayerInformation(string name, int id){
		var jugadorExistente = GameManager.Instance.JugadoresEnPartida.Find(j => j.Id == id);
		if(jugadorExistente == null){
			AbejaReina infoJugadores = new AbejaReina(id){Name = name, Id = id};
			GameManager.Instance.JugadoresEnPartida.Add(infoJugadores);
		} else {
			jugadorExistente.Name = name;
		}

		if(Multiplayer.IsServer()){
			foreach (var item in GameManager.Instance.JugadoresEnPartida){
				Rpc(nameof(SendPlayerInformation), item.Name, item.Id);
			}
		}
		ActualizarEstadoBotonJugar();
	}
}
