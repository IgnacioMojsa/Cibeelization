using Godot;
using System;
using System.Collections.Generic;

public partial class MenuRadial : UIBase {
	[Export] private GameUI hud; 
	private Control ContenedorDeBotones;
	public Control ContenedorDeTransformaciones;
	public Control ContenedorDeAtaque;
	private HashSet<Abeja> TransformacionesDisponiblesActualmente = new HashSet<Abeja>();
	private readonly HashSet<Control> contenedoresCerrandose = new();
	public bool Activo = false;
	public Vector2 PosicionCentral;
	public float Radio = 110f;
	public float VelocidadAnimacion = 0.25f;

	public override void _Ready(){
		ContenedorDeBotones = GetNode<Control>("%ContenedorBotones");
		ContenedorDeTransformaciones = GetNode<Control>("%ContenedorTransformaciones");
		ContenedorDeAtaque = GetNode<Control>("%ContenedorAtaque");

		PosicionCentral = GetViewportRect().Size / 2 - ContenedorDeBotones. GetRect().Size / 2;
		ContenedorDeBotones.Hide();
		ContenedorDeTransformaciones.Hide();
		ContenedorDeAtaque.Hide();

		foreach (Node nodoHijo in ContenedorDeBotones.GetChildren()){
			if(nodoHijo is Button boton){
				ConfigurarBotonRadial(boton);
			}
		}

		foreach (Node nodoHijo in ContenedorDeTransformaciones.GetChildren()){
			if(nodoHijo is Button boton){
				ConfigurarBotonRadial(boton);
			}
		}

		foreach (Node nodoHijo in ContenedorDeAtaque.GetChildren()){
			if(nodoHijo is Button boton){
				ConfigurarBotonRadial(boton);
			}
		}
	}

	private void ConfigurarBotonRadial(Button boton){
		boton.PivotOffset = boton.Size / 2;
		boton.Position = Vector2.Zero - (boton.Size / 2);
		boton.Scale = Vector2.Zero;
		boton.Pressed += () => SeleccionarOpcion(boton.Name);
	}

	public void DesplegarEnPosicion(Vector2 posicionGlobal){
		GlobalPosition = posicionGlobal;
		if (Activo){
			OcultarMenu();

			if (ContenedorDeTransformaciones.Visible){
				OcultarTransformaciones();
				GameManager.Instance.jugadorEnTurno.ModoTransformacion = false;
			}

			if (ContenedorDeAtaque.Visible){
				OcultarAtaques();
				GameManager.Instance.jugadorEnTurno.ModoAtaque = false;
				hud.playerManager.vfxManager.EsconderOutlineDeJugadores();
				hud.playerManager.vfxManager.OcultarAbejasObjetivo();
				hud.playerManager.celdasManager.DespintarCeldaDeAbejaBuffeada();
			}
		} else if (GameManager.Instance.jugadorEnTurno.ModoAtaque){
			MostrarAtaques();
		}
		else
		{
			MostrarMenu();
		}
	}

	private void OnPressed()
	{
		if(Activo)
		{
			OcultarMenu();
		}
		else
		{
			MostrarMenu();
		}
	}

	public void MostrarMenu(){
		Activo = true;
		ContenedorDeBotones.Show();
		Show();
		MoveToFront();
		AnimarAperturaContenedor(ContenedorDeBotones);
	}

	public void MostrarTransformaciones(){
		Activo = true;
		ContenedorDeTransformaciones.Show();
		Show();
		MoveToFront();
		AnimarAperturaContenedor(ContenedorDeTransformaciones, deshabilitarBotones: true);
		GameManager.Instance.jugadorEnTurno.ModoTransformacion = true;
	}

	public void MostrarAtaques(){
		Activo = true;
		ContenedorDeAtaque.Show();
		Show();
		MoveToFront();
		AnimarAperturaContenedor(ContenedorDeAtaque, deshabilitarBotones: true);
		GameManager.Instance.jugadorEnTurno.ModoAtaque = true;
		if (hud != null) hud.ActualizarUIAtaques();
	}

	public void OcultarMenu(){
		OcultarContenedor(ContenedorDeBotones);
	}

	public void OcultarTransformaciones(){
		OcultarContenedor(ContenedorDeTransformaciones);
	}

	public void OcultarAtaques(){
		OcultarContenedor(ContenedorDeAtaque);
	}

	public void EstablecerHUD(GameUI gameUI){
		hud = gameUI;
	}

	private void AnimarAperturaContenedor(Control contenedor, bool deshabilitarBotones = false){
		contenedoresCerrandose.Remove(contenedor);
		
		int cantidadDeBotones = contenedor.GetChildCount();
		if (cantidadDeBotones == 0) return;

		float espaciado = MathF.Tau / cantidadDeBotones;

		for (int b = 0; b < cantidadDeBotones; b++){
			if (contenedor.GetChild(b) is Button boton){
				float angulo = b * espaciado;
				Vector2 posicionObjetivo = (new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * Radio) - (boton.Size / 2);

				Tween tween = CreateTween().SetParallel(true);
				tween.TweenProperty(boton, "position", posicionObjetivo, VelocidadAnimacion)
					.SetTrans(Tween.TransitionType.Back)
					.SetEase(Tween.EaseType.Out);

				tween.TweenProperty(boton, "scale", Vector2.One, VelocidadAnimacion)
					.SetTrans(Tween.TransitionType.Back)
					.SetEase(Tween.EaseType.Out);

				boton.Disabled = deshabilitarBotones;
			}
		}
	}

	private void OcultarContenedor(Control contenedor){
		contenedoresCerrandose.Add(contenedor);
		
		foreach (var hijo in contenedor.GetChildren()){
			if (hijo is Button boton){
				Vector2 posicionCentroLocal = Vector2.Zero - (boton.Size / 2);
				
				Tween tween = CreateTween().SetParallel(true);
				tween.TweenProperty(boton, "position", posicionCentroLocal, VelocidadAnimacion)
					.SetTrans(Tween.TransitionType.Linear)
					.SetEase(Tween.EaseType.In);

				tween.TweenProperty(boton, "scale", Vector2.Zero, VelocidadAnimacion)
					.SetTrans(Tween.TransitionType.Linear)
					.SetEase(Tween.EaseType.In);
			}
		}

		GetTree().CreateTimer(VelocidadAnimacion).Timeout += () =>{
        	if (!contenedoresCerrandose.Remove(contenedor)) return;
        	contenedor.Hide();
        	Activo = ContenedorDeBotones.Visible
        	      || ContenedorDeTransformaciones.Visible
        	      || ContenedorDeAtaque.Visible;
    	};
	}
	
	private void SeleccionarOpcion(string nombreBoton){
		if (GameManager.Instance.modoMultiplayer){
			long idLocal = Multiplayer.GetUniqueId();
			var jugadorEnTurno = GameManager.Instance.jugadorEnTurno;
			bool esTurnoLocal = (idLocal == jugadorEnTurno.Id) || (GameManager.Instance.jugadorEsHost && idLocal == 1);
			if (!esTurnoLocal){
				OcultarMenu();
				return;
			}
		}

		switch(nombreBoton)
		{
			case "ControlarButton":
				hud.OnControlarPressed();
				break;
			case "CurarButton":
				hud.OnCurarPressed();
				break;
			case "InvocarButton":
				hud.InvocarSubdito();
				break;
			case "TransformarButton":
				MostrarTransformaciones();
				hud.tropasManager.ObtenerSubditosTransformables();
				MostrarTransformacionesDisponibles();
				break;
			case "AbejaObrera":
				OcultarTransformaciones();
				hud.tropasManager.MostrarTransformablesEn(typeof(AbejaObrera));
				break;
			case "AbejaPolinizadora":
				OcultarTransformaciones();
				hud.tropasManager.MostrarTransformablesEn(typeof(AbejaPolinizadora));
				break;
			case "AbejaConstructora":
				OcultarTransformaciones();
				hud.tropasManager.MostrarTransformablesEn(typeof(AbejaConstructora));
				break;
			case "AbejaGuardiana":
				OcultarTransformaciones();
				hud.tropasManager.MostrarTransformablesEn(typeof(AbejaGuardiana));
				break;
			case "AtacarButton":
				if (GameManager.Instance.jugadorEnTurno.AtaqueAObjetivo)
				{
					GameManager.Instance.jugadorEnTurno.ModoAtaque = false;
					GameManager.Instance.jugadorEnTurno.AtaqueAObjetivo = false;
					hud.playerManager.vfxManager.EsconderOutlineDeJugadores();
					hud.playerManager.vfxManager.OcultarAbejasObjetivo();
					hud.playerManager.celdasManager.DespintarCeldaDeAbejaBuffeada();
					OcultarAtaques();
					break;
				}
				else
				{
					GameManager.Instance.jugadorEnTurno.AtaqueAObjetivo = true;
					hud.AtaqueAObjetivo();
					OcultarAtaques();
					break;
				}
			case "ComandarButton":
				hud.ComandarAtaque();
				OcultarAtaques();
				GameManager.Instance.jugadorEnTurno.ModoAtaque = false;
				break;
			case "DestruirButton":	
				if(hud.playerManager.tropasManager.SubditoActual is AbejaObrera subdito && subdito.ModoDestruccion)
				{
					hud.celdasManager.OcultarCeldasReforzadasCercanas();
					GameManager.Instance.jugadorEnTurno.ModoAtaque = false;
					subdito.ModoDestruccion = false;
					OcultarAtaques();
					break;
				}
				else
				{
					hud.DestruirCeldasReforzadas();
					OcultarAtaques();
					hud.playerManager.tropasManager.SubditoActual.HabilidadDeAbeja();
					break;
				}
		}
		
		OcultarMenu();
	}

	public void ActualizarEstadoBoton(bool condicion, string unBoton, Control unContenedor = null){
		//para ver si hay contenedores con nombres repetidos
		Control[] contenedoresAProbar = unContenedor != null 
			? new[] { unContenedor } 
			: new[] { ContenedorDeBotones, ContenedorDeAtaque };

		foreach (var contenedor in contenedoresAProbar){
			var boton = contenedor?.GetNodeOrNull<Button>(unBoton);
			if (boton != null){
				boton.Disabled = !condicion;
				return;
			}
		}
	}

	public void VerificarTransformacionesDisponibles(){
		TransformacionesDisponiblesActualmente.Clear();
		
		foreach (var abeja in hud.tropasManager.AbejasTransformables){
			if(abeja == null) continue;
			
			List<Abeja> transformacionesObtenidas = new List<Abeja>();
			
			foreach (var t in GameManager.Instance.ObtenerTransformacionesDisponibles(abeja)){
				if (GameManager.Instance.jugadorEnTurno.ColmenaDeReina.CantidadTotalDeUnRecurso(t.RecursoDeTransformacion) >= t.RecursosNecesarios)
					TransformacionesDisponiblesActualmente.Add(t); 
			}
		}
	}

	public void MostrarTransformacionesDisponibles(){
		VerificarTransformacionesDisponibles();
		
		foreach (var transformacion in TransformacionesDisponiblesActualmente){
			var boton = ContenedorDeTransformaciones.GetNodeOrNull<Button>(transformacion.GetType().Name);
			if (boton != null) boton.Disabled = false;
		}
	}

}
