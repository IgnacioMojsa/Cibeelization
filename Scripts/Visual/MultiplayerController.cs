using System.Collections.Generic;
using Godot;

public partial class MultiplayerController : Node
{
	[Export] private int port = 8910;
	[Export] private string adress = "127.0.0.1";
	private ENetMultiplayerPeer peer;
	public Button botonHost;
	public Button botonCliente;
	public Button botonJugar;
	public List<Node3D> abejasEnEspera;
	private bool estaConectadoAlServidor = false;
	int maxJugadores = 4;

	public override void _Ready(){
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;
		Multiplayer.ServerDisconnected += ServerDisconnected;

		if(GameManager.Instance.jugadorEsHost){
			maxJugadores = Mathf.Clamp(GameManager.Instance.cantidadJugadores, 2, 4);
		}

		abejasEnEspera = new List<Node3D>(){
			GetNode<Node3D>("SalaDeEspera/AbejaReina"), 
			GetNode<Node3D>("SalaDeEspera/AbejaReina2"), 
			GetNode<Node3D>("SalaDeEspera/AbejaReina3"), 
			GetNode<Node3D>("SalaDeEspera/AbejaReina4")
		};

		ConfigurarBotones();
		ActualizarAbejasEnEspera();
	}

	private void ActualizarAbejasEnEspera()
	{
		int cantidad = GameManager.Instance.JugadoresEnPartida.Count;

		for (int i = 0; i < abejasEnEspera.Count; i++)
		{
			bool hayJugador = i < cantidad;
			abejasEnEspera[i].Visible = hayJugador;
		
			var label = abejasEnEspera[i].GetNodeOrNull<Label3D>("Nombre");
			if (label != null && hayJugador){
				label.Visible = true;
				label.Text = GameManager.Instance.JugadoresEnPartida[i].Name;
			}
		}
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
			int conectados = GameManager.Instance.JugadoresEnPartida.Count;
			bool hayClientesConectados = Multiplayer.MultiplayerPeer != null && Multiplayer.GetPeers().Length > 0;
			bool hayClientesRegistrados = GameManager.Instance.JugadoresEnPartida.Count > 1;
			bool cantidadValida = conectados >= 2 && conectados <= maxJugadores;
			//botonJugar.Disabled = !(hayClientesConectados && hayClientesRegistrados);
			botonJugar.Disabled = !(hayClientesConectados && cantidadValida);
		} else {
			botonJugar.Disabled = !estaConectadoAlServidor;
			botonJugar.Visible = false;
		}
	}

	private void PeerConnected(long id){
		GD.Print("PLAYER CONNECTED: " + id.ToString());
		if (Multiplayer.IsServer()){
			int maxClientes = maxJugadores - 1;
			if (Multiplayer.GetPeers().Length > maxClientes || GameManager.Instance.JugadoresEnPartida.Count >= maxJugadores){
				GD.Print($"Sala llena, ({maxJugadores} jugadores máximo). Rechazando a peer {id}");
				Multiplayer.MultiplayerPeer.DisconnectPeer((int)id);
			}
		}
	}

	private void PeerDisconnected(long id){
		GD.Print("PLAYER DISCONNECTED: " + id.ToString());
		
		var jugador = GameManager.Instance.JugadoresEnPartida.Find(j => j.Id == id);
		if (jugador != null){
			jugador.HP = 0;
			jugador.FueraDeJuego = true;
			GD.Print($"Jugador {jugador.Id} se desconectó. HP establecido en 0 y marcado fuera de juego.");

			if (GetTree().CurrentScene.SceneFilePath == "res://Scenes/escenaPrueba.tscn"){
				if (GameManager.Instance.CondicionVictoria()){
					GameManager.Instance.EstablecerJugadorGanador();
					GetTree().ChangeSceneToFile("res://Scenes/pantallaVictoria.tscn");
				}
			} else {
				GameManager.Instance.JugadoresEnPartida.Remove(jugador);
				ActualizarAbejasEnEspera();
			}
		}
		ActualizarEstadoBotonJugar();
	}

	private void ConnectedToServer(){
		GD.Print("CONNECTED TO SERVER");
		estaConectadoAlServidor = true;
		ActualizarEstadoBotonJugar();
		RpcId(1, nameof(SendPlayerInformation), GetNode<LineEdit>("LineEdit").Text, Multiplayer.GetUniqueId());
	}

	private void ConnectionFailed(){
		GD.Print("CONNECTION FAILED.");
		estaConectadoAlServidor = false;
		if (botonCliente != null) botonCliente.Disabled = false;
		if (Multiplayer.MultiplayerPeer != null){
			Multiplayer.MultiplayerPeer.Close();
			Multiplayer.MultiplayerPeer = null;
		}
		ActualizarEstadoBotonJugar();
	}

	private void ServerDisconnected(){
		GD.Print("SERVER DISCONNECTED");
		LimpiarYVolverAlMenu();
	}

	public void LimpiarYVolverAlMenu(){
		if(Multiplayer.MultiplayerPeer != null){
			Multiplayer.MultiplayerPeer.Close();
			Multiplayer.MultiplayerPeer = null;
		}

		GameManager.Instance.ResetearEstadoPartida();
		estaConectadoAlServidor = false;

		if(GetTree().CurrentScene.SceneFilePath != "res://Scenes/pantallaInicial.tscn"){
			GetTree().ChangeSceneToFile("res://Scenes/pantallaInicial.tscn");
		}
	}

	private void _on_host_button_down(){
		if (Multiplayer.MultiplayerPeer != null){
			Multiplayer.MultiplayerPeer.Close();
			Multiplayer.MultiplayerPeer = null;
		}

		botonHost.Disabled = true;

		maxJugadores = Mathf.Clamp(GameManager.Instance.cantidadJugadores, 2, 4);
		int maxClientes = maxJugadores - 1;

		peer = new ENetMultiplayerPeer();
		var error = peer.CreateServer(port, maxClientes);

		if(error != Error.Ok){
			GD.Print("Error cannot host: " + error.ToString());
			botonHost.Disabled = false;
			return;
		}

		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Esperando jugadores...");
		GD.Print($"Sala para {maxJugadores} jugadores. Tablero: {GameManager.Instance.sizeTablero})");
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
		if (!Multiplayer.IsServer()) return;
		Rpc(nameof(startGame), GameManager.Instance.sizeTablero);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void startGame(int sizeTablero){
		GameManager.Instance.cantidadJugadores = GameManager.Instance.JugadoresEnPartida.Count;
		GameManager.Instance.sizeTablero = sizeTablero;
		if(GameManager.Instance.TurnManager == null && GameManager.Instance.JugadoresEnPartida.Count > 0){
			GameManager.Instance.TurnManager = new TurnManager(GameManager.Instance.JugadoresEnPartida);
			GameManager.Instance.TurnManager.EstablecerPrimerTurno();
			GameManager.Instance.jugadorEnTurno = GameManager.Instance.TurnManager.jugadorEnTurno;
		}
		GetTree().ChangeSceneToFile("res://Scenes/escenaPrueba.tscn");
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void SendPlayerInformation(string name, int id){
		var jugadorExistente = GameManager.Instance.JugadoresEnPartida.Find(j => j.Id == id);
		if (Multiplayer.IsServer() && jugadorExistente == null && GameManager.Instance.JugadoresEnPartida.Count >= maxJugadores){
			GD.Print($"Limite de {maxJugadores} jugadores alcanzado. Desconectando a {id}");
			Multiplayer.MultiplayerPeer.DisconnectPeer(id);
			return;
		}
		
		if(jugadorExistente == null){
			AbejaReina infoJugadores = new AbejaReina(id){Name = name, Id = id};
			GameManager.Instance.JugadoresEnPartida.Add(infoJugadores);
		} else {
			jugadorExistente.Name = name;
		}

		if(Multiplayer.IsServer()){
			foreach (var jugador in GameManager.Instance.JugadoresEnPartida){
				Rpc(nameof(SendPlayerInformation), jugador.Name, jugador.Id);
			}
		}

		ActualizarEstadoBotonJugar();
		ActualizarAbejasEnEspera();
	}

	private void DesconectarYLimpiarDatos(){
		if (Multiplayer.MultiplayerPeer != null){
			Multiplayer.MultiplayerPeer.Close();
			Multiplayer.MultiplayerPeer = null;
		}
	
		GameManager.Instance.JugadoresEnPartida.Clear();
		estaConectadoAlServidor = false;
	}

	public override void _ExitTree(){
		if (Multiplayer != null){
			Multiplayer.PeerConnected -= PeerConnected;
			Multiplayer.PeerDisconnected -= PeerDisconnected;
			Multiplayer.ConnectedToServer -= ConnectedToServer;
			Multiplayer.ConnectionFailed -= ConnectionFailed;
			Multiplayer.ServerDisconnected -= ServerDisconnected;
		}
		base._ExitTree();
	}

}
