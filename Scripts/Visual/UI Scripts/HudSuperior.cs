using System;
using System.Collections.Generic;
using Godot;

public partial class HudSuperior : UIBase
{
	public PlayerManager playerManager;
	public Control recursos;
	public Button botonPausa;
	public PanelContainer containerPausa;
	public event Action OnPausaAlternada;
	private PanelContainer[] uiJugadores;
	private Label[] HPJugadores;
	private TextureRect iconoJugadorEnTurno;
	private readonly Color[] colorJugadores = {
		Color.FromHtml("#6a268f"), 
		Color.FromHtml("#4c1dae"),
		Color.FromHtml("#b33671"), 
		Color.FromHtml("#8260e5")
	};
	private CompressedTexture2D[] iconosJugadores;

	public void Inicializar(PlayerManager pm){
		playerManager = pm;

		containerPausa = GetNode<PanelContainer>("Pausa");
		recursos = GetNode<Control>("CantRecursos");
		botonPausa = GetNode<Button>("Pausa/PausaButton");
		botonPausa.Pressed += () => OnPausaAlternada?.Invoke();

		iconoJugadorEnTurno = GetNode<TextureRect>("Elementos/IconoJugador");
		uiJugadores = new[] {
			GetNode<PanelContainer>("VBoxContainer/Jugador1"),
			GetNode<PanelContainer>("VBoxContainer/Jugador2"),
			GetNode<PanelContainer>("VBoxContainer/Jugador3"),
			GetNode<PanelContainer>("VBoxContainer/Jugador4")
		};

		HPJugadores = new[] {
			GetNode<Label>("VBoxContainer/Jugador1/HBoxContainer/MarginContainer2/Label"),
			GetNode<Label>("VBoxContainer/Jugador2/HBoxContainer/MarginContainer2/Label"),
			GetNode<Label>("VBoxContainer/Jugador3/HBoxContainer/MarginContainer2/Label"),
			GetNode<Label>("VBoxContainer/Jugador4/HBoxContainer/MarginContainer2/Label")
		};

		iconosJugadores = new[] {
			GD.Load<CompressedTexture2D>("res://Assets/UI/AbejarReina1Contorno.svg"),
			GD.Load<CompressedTexture2D>("res://Assets/UI/AbejarReina2Contorno.svg"),
			GD.Load<CompressedTexture2D>("res://Assets/UI/AbejarReina3Contorno.svg"),
			GD.Load<CompressedTexture2D>("res://Assets/UI/AbejarReina4Contorno.svg")
		};
	}

	public void MostrarDataDeJugadores(){
		for (int i = 0; i < uiJugadores.Length; i++){
			uiJugadores[i].Visible = i < GameManager.Instance.cantidadJugadores;
		}
	}

	public void MostrarHPDeJugaores(){
		int totalHPs = Mathf.Min(GameManager.Instance.cantidadJugadores, GameManager.Instance.JugadoresEnPartida.Count);
		for(int j = 0; j < totalHPs; j++){
			HPJugadores[j].Text = GameManager.Instance.JugadoresEnPartida[j].HP + " HP";
		}
	}

	public void MostrarJugadorEnTurno(){ 
		AbejaReina jugadorEnTurno = GameManager.Instance.jugadorEnTurno;
		if (jugadorEnTurno == null) return;

		for(int j = 0; j < GameManager.Instance.JugadoresEnPartida.Count; j++){
			var jugador = GameManager.Instance.JugadoresEnPartida[j];
			if (j >= uiJugadores.Length) break;

			if(jugador == jugadorEnTurno && !jugador.FueraDeJuego){
				if (uiJugadores[j] != null) uiJugadores[j].Modulate = colorJugadores[j];
				if (iconoJugadorEnTurno != null && j < iconosJugadores.Length) iconoJugadorEnTurno.Texture = iconosJugadores[j];
			} 
			else if (jugador.FueraDeJuego){
				if (uiJugadores[j] != null) uiJugadores[j].Visible = false;
			}
			else {
				if (uiJugadores[j] != null) uiJugadores[j].Modulate = Color.FromHtml("#ffffff");
			}
		}

		if(playerManager != null && playerManager.VisualesJugadores.TryGetValue(jugadorEnTurno, out Node3D visualActual)){
			playerManager.VisualJugadorActual = visualActual;
			try{
				playerManager.vfxManager?.OutlineJugadorEnTurno(visualActual);
			} catch (Exception ex){
				GD.PrintErr($"[HudSuperior] Error al aplicar outline a {visualActual?.Name}: {ex.Message}");
			}
		}

		GD.Print($"[HudSuperior][Peer {Multiplayer.GetUniqueId()}] Jugador resaltado en HUD: {jugadorEnTurno.Id}");
	}
	
	public void MostrarRecursosDeJugadorEnTurno(){
		AbejaReina jugadorEnTurno = GameManager.Instance.jugadorEnTurno;

		if(GameManager.Instance.modoMultiplayer){
			long idLocal = Multiplayer.GetUniqueId();
			var jugadorLocal = GameManager.Instance.JugadoresEnPartida.Find(j => j.Id == idLocal);
			if(jugadorLocal != null){
				jugadorEnTurno = jugadorLocal;
			}
		}

		if(jugadorEnTurno == null || jugadorEnTurno.ColmenaDeReina == null) return;
		
		var nodosHijos = recursos.GetChildren();
		foreach (var hijo in nodosHijos){
			if(hijo is Label hijoLabel){
				hijoLabel.Text = "0";
			}
		}

		//if(recursosActuales.Count == 0){
		//}

		HashSet<string> recursosActuales = new HashSet<string>();
		foreach (var recurso in jugadorEnTurno.ColmenaDeReina.RecursosDeColmena){
			if(recurso == null) continue; //break
			recursosActuales.Add(recurso);
		}

		foreach (var recurso in recursosActuales){
			var cantRecurso = recursos.GetNodeOrNull<Label>(recurso);
			if(cantRecurso != null){
				cantRecurso.Text = jugadorEnTurno.ColmenaDeReina.CantidadTotalDeUnRecurso(recurso).ToString();
			}
		}
	}
}
