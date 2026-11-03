using Godot;
using System.Collections.Generic;
public partial class UIJugadores : Control {
	[Export] public PlayerManager playerManager;
	public Control recursos;
	
    public void MostrarDataDeJugadores(){
		var jugador1 = GetNode<PanelContainer>("VBoxContainer/Jugador1");
		var jugador2 = GetNode<PanelContainer>("VBoxContainer/Jugador2");
		var jugador3 = GetNode<PanelContainer>("VBoxContainer/Jugador3");
		var jugador4 = GetNode<PanelContainer>("VBoxContainer/Jugador4");

		jugador1.Visible = true;
		jugador2.Visible = true;

		if(GameManager.Instance.cantidadJugadores == 3){
			jugador3.Visible = true;
		}
		else if(GameManager.Instance.cantidadJugadores == 4){
			jugador3.Visible = true;
			jugador4.Visible = true;
		}
	}

	public void MostrarHPDeJugaores(){
		var hpJ1 = GetNode<Label>("VBoxContainer/Jugador1/HBoxContainer/MarginContainer2/Label");
		var hpJ2 = GetNode<Label>("VBoxContainer/Jugador2/HBoxContainer/MarginContainer2/Label");
		var hpJ3 = GetNode<Label>("VBoxContainer/Jugador3/HBoxContainer/MarginContainer2/Label");
		var hpJ4 = GetNode<Label>("VBoxContainer/Jugador4/HBoxContainer/MarginContainer2/Label");

		List<Label> HPJugadores = new List<Label>{ hpJ1, hpJ2, hpJ3, hpJ4};

		for (int j = 0; j < GameManager.Instance.cantidadJugadores; j++){
			HPJugadores[j].Text = GameManager.Instance.JugadoresEnPartida[j].HP + " HP";
		}
	}

	public void MostrarJugadorEnTurno(){ 
		AbejaReina jugadorEnTurno = GameManager.Instance.jugadorEnTurno;
		TextureRect iconoJugadorEnTurno = GetNode<TextureRect>("Elementos/IconoJugador");

		var jugador1 = GetNode<PanelContainer>("VBoxContainer/Jugador1");
		var jugador2 = GetNode<PanelContainer>("VBoxContainer/Jugador2");
		var jugador3 = GetNode<PanelContainer>("VBoxContainer/Jugador3");
		var jugador4 = GetNode<PanelContainer>("VBoxContainer/Jugador4");

		var colorJ1 = Color.FromHtml("#6a268f");
		var colorJ2 = Color.FromHtml("#4c1dae");
		var colorJ3 = Color.FromHtml("#b33671");
		var colorJ4 = Color.FromHtml("#8260e5");

		var iconoJ1 = GD.Load<CompressedTexture2D>("res://Assets/UI/AbejarReina1Contorno.svg");
		var iconoJ2 = GD.Load<CompressedTexture2D>("res://Assets/UI/AbejarReina2Contorno.svg");
		var iconoJ3 = GD.Load<CompressedTexture2D>("res://Assets/UI/AbejarReina3Contorno.svg");
		var iconoJ4 = GD.Load<CompressedTexture2D>("res://Assets/UI/AbejarReina4Contorno.svg");

		List<PanelContainer> UIJugadores = new List<PanelContainer>{ jugador1, jugador2, jugador3, jugador4};
		List<Color> ColorJugadores = new List<Color>{ colorJ1, colorJ2, colorJ3, colorJ4};
		List<CompressedTexture2D> IconosJugadores = new List<CompressedTexture2D>{iconoJ1, iconoJ2, iconoJ3, iconoJ4};

		for (int j = 0; j < GameManager.Instance.JugadoresEnPartida.Count; j++){
			if( GameManager.Instance.JugadoresEnPartida[j] == jugadorEnTurno && !(GameManager.Instance.JugadoresEnPartida[j].FueraDeJuego)){
				UIJugadores[j].Modulate = ColorJugadores[j];
				iconoJugadorEnTurno.Texture = IconosJugadores[j];
			}else if(GameManager.Instance.JugadoresEnPartida[j].FueraDeJuego){
				UIJugadores[j].Visible = false;
			}else{
				UIJugadores[j].Modulate = Color.FromHtml("#ffffff");
			}
		}

		if (playerManager.VisualesJugadores.Count >= jugadorEnTurno.Id){
			playerManager.VisualJugadorActual = playerManager.VisualesJugadores[jugadorEnTurno];
			playerManager.vfxManager.OutlineJugadorEnTurno(playerManager.VisualJugadorActual);
		}
	}

	public void MostrarRecursosDeJugadorEnTurno(){
		var jugadorEnTurno = GameManager.Instance.jugadorEnTurno;

		HashSet<string> recursosActuales = new HashSet<string>();

		if(recursosActuales.Count == 0){
			var nodosHijos = recursos.GetChildren();

			foreach (var hijo in nodosHijos){
					if(hijo is Label hijoLabel)
					
					hijoLabel.Text = "0";
				}
		}

		foreach (var recurso in jugadorEnTurno.ColmenaDeReina.RecursosDeColmena){
			if(recurso == null) break;
			
			recursosActuales.Add(recurso);
		}

		foreach (var recurso in recursosActuales){
			var cantRecurso = recursos.GetNode<Label>(recurso);

			cantRecurso.Text = jugadorEnTurno.ColmenaDeReina.CantidadTotalDeUnRecurso(recurso).ToString();
		}
	}
}