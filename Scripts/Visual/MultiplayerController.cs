using Godot;

public partial class MultiplayerController : Control
{
	[Export] private int port = 8910;
	[Export] private string adress = "127.0.0.1";
	private ENetMultiplayerPeer peer;
	public Button botonHost;
	public Button botonCliente;
	private Button opciones;
	private Button ajustes;
	private Button volver;
	private Button salir;
	private PanelContainer confirmacionSalir;
	private HBoxContainer opcionesContainer;
	private PanelContainer ajustesContainer;
	public AudioSetting UiSound1 => AudioManager.Instance?.GameAudio?.Sound1;

	public override void _Ready(){
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;

		cargarUI();
	}

	public void PeerConnected(long id){
		GD.Print("PLAYER CONNECTED: " + id.ToString());
	}

	public void PeerDisconnected(long id){
		GD.Print("PLAYER DISCONNECTED: " + id.ToString());
	}

	public void ConnectedToServer(){
		GD.Print("CONNECTED TO SERVER");
		RpcId(1, nameof(SendPlayerInformation), GetNode<LineEdit>("LineEdit").Text, Multiplayer.GetUniqueId());
	}

	public void ConnectionFailed(){
		GD.Print("CONNECTION FAILED");
	}

	public void _on_host_button_down(){
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
	}

	public void _on_cliente_button_down(){
		botonCliente.Disabled = true;
		peer = new ENetMultiplayerPeer();
		peer.CreateClient(adress, port);
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Joining game");
	}

	public void _on_jugar_button_down(){
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
		this.Hide();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]

	private void SendPlayerInformation(string name, int id){
		var jugadorExistente = GameManager.Instance.JugadoresEnPartida.Find(j => j.Id == id);
		if(jugadorExistente == null){
			AbejaReina infoJugadores = new AbejaReina(id){Name = name, Id = id};
			GameManager.Instance.JugadoresEnPartida.Add(infoJugadores);
		}
		else{
			jugadorExistente.Name = name;
		}

		if(Multiplayer.IsServer()){
			foreach (var item in GameManager.Instance.JugadoresEnPartida){
				Rpc(nameof(SendPlayerInformation), item.Name, item.Id);
			}
		}
	}

	private void cargarUI(){
		botonHost =	GetNode<Button>("Host");
		botonCliente = GetNode<Button>("Cliente");		

		if(!GameManager.Instance.jugadorEsHost){
			botonHost.Visible = false;
		}
		else{
			botonCliente.Visible = false;
		}

		opciones = GetNode<Button>("AjustesDeJuego/PausaButton");
		ajustes = GetNode<Button>("Opciones/PausaBorder/MarginContainer/VBoxContainer/Ajustes/AjustesButton");
		volver = GetNode<Button>("Opciones/PausaBorder/MarginContainer/VBoxContainer/Cerrar/CerrarButton");
		salir = GetNode<Button>("Opciones/PausaBorder/MarginContainer/VBoxContainer/SalirAMenuPrincipal/SalirButton");

		confirmacionSalir = GetNode<PanelContainer>("ConfirmacionSalir");
		opcionesContainer = GetNode<HBoxContainer>("Opciones");
		ajustesContainer = GetNode<PanelContainer>("Ajustes");
	}

	private void MostrarUIPanel(Control panel, bool mostrar){
		if (panel == null) return;
		panel.Visible = mostrar;
		AudioManager.Instance.PlaySound(UiSound1);
	}

	private void CambiarMenu(Control menuAOcultar, Control menuAMostrar){
		menuAOcultar.Visible = false;
		menuAMostrar.Visible = true;
		AudioManager.Instance.PlaySound(UiSound1);
	}

	private void MostrarOpciones(){
		CambiarMenu(opciones, opcionesContainer);
	}

	private void OcultarOpciones(){
		CambiarMenu(opcionesContainer, opciones);
	}

	private void Ajustes(){
		MostrarUIPanel(ajustesContainer, true);
	}

	private void AceptarConfigAudio(){
		MostrarUIPanel(ajustesContainer, false);
	}

	private void MostrarConfirmacionDeSalir(){
		CambiarMenu(opcionesContainer, confirmacionSalir);
	}

	private void OcultarConfirmacionSalir(){
		CambiarMenu(confirmacionSalir, opcionesContainer);
	}

	private void VolverAPantallaDeInicio(){
		GetTree().ChangeSceneToFile("res://Scenes/pantallaInicial.tscn");
		this.Hide();
	}
}
