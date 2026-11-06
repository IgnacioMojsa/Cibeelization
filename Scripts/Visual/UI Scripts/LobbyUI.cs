using Godot;

public partial class LobbyUI : UIBase {
	[Export] private MultiplayerController multiplayerController;
	private Button opciones;
	private Button ajustes;
	private Button volver;
	private Button salir;
	private PanelContainer confirmacionSalir;
	private HBoxContainer opcionesContainer;
	private PanelContainer ajustesContainer;

	public override void _Ready(){
		opciones = GetNode<Button>("AjustesDeJuego/PausaButton");
		ajustes = GetNode<Button>("Opciones/PausaBorder/MarginContainer/VBoxContainer/Ajustes/AjustesButton");
		volver = GetNode<Button>("Opciones/PausaBorder/MarginContainer/VBoxContainer/Cerrar/CerrarButton");
		salir = GetNode<Button>("Opciones/PausaBorder/MarginContainer/VBoxContainer/SalirAMenuPrincipal/SalirButton");

		confirmacionSalir = GetNode<PanelContainer>("ConfirmacionSalir");
		opcionesContainer = GetNode<HBoxContainer>("Opciones");
		ajustesContainer = GetNode<PanelContainer>("Ajustes");
	}

	private void cargarUI(){
		opciones = GetNode<Button>("AjustesDeJuego/PausaButton");
		ajustes = GetNode<Button>("Opciones/PausaBorder/MarginContainer/VBoxContainer/Ajustes/AjustesButton");
		volver = GetNode<Button>("Opciones/PausaBorder/MarginContainer/VBoxContainer/Cerrar/CerrarButton");
		salir = GetNode<Button>("Opciones/PausaBorder/MarginContainer/VBoxContainer/SalirAMenuPrincipal/SalirButton");

		confirmacionSalir = GetNode<PanelContainer>("ConfirmacionSalir");
		opcionesContainer = GetNode<HBoxContainer>("Opciones");
		ajustesContainer = GetNode<PanelContainer>("Ajustes");
	}

	private void MostrarOpciones() => CambiarMenu(opciones, opcionesContainer);
	private void OcultarOpciones() => CambiarMenu(opcionesContainer, opciones);
	private void Ajustes() => MostrarUIPanel(ajustesContainer, true);
	private void AceptarConfigAudio() => MostrarUIPanel(ajustesContainer, false);
	private void MostrarConfirmacionDeSalir() => CambiarMenu(opcionesContainer, confirmacionSalir);
	private void OcultarConfirmacionSalir() => CambiarMenu(confirmacionSalir, opcionesContainer);

	private void VolverAPantallaDeInicio(){
		GetTree().ChangeSceneToFile("res://Scenes/pantallaInicial.tscn");
		Hide();
	}
}
