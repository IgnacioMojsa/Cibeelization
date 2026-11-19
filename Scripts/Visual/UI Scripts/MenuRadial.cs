using Godot;
using System;
using System.Collections.Generic;

public partial class MenuRadial : UIBase {
	private HudInferior hud; 
	private Control ContenedorDeBotones;
	public Control ContenedorDeTransformaciones;
	public Control ContenedorDeAtaque;
	private HashSet<Abeja> TransformacionesDisponiblesActualmente = new HashSet<Abeja>();
	private readonly HashSet<Control> contenedoresCerrandose = new();
	public bool Activo = false;
	public Vector2 PosicionCentral;
	public float Radio = 110f;
	public float VelocidadAnimacion = 0.25f;
	[Export] public float AnchoBotonTransformacion = 220f;  
	[Export] public float RadioTransformaciones = 190f; 

	public override void _Ready(){
		ContenedorDeBotones = GetNode<Control>("%ContenedorBotones");
		ContenedorDeTransformaciones = GetNode<Control>("%ContenedorTransformaciones");
		ContenedorDeAtaque = GetNode<Control>("%ContenedorAtaque");

		PosicionCentral = GetViewportRect().Size / 2 - ContenedorDeBotones. GetRect().Size / 2;
		ContenedorDeBotones.Hide();
		ContenedorDeTransformaciones.Hide();
		ContenedorDeAtaque.Hide();

		foreach (Node nodoHijo in ContenedorDeBotones.GetChildren()){
			if(nodoHijo is BaseButton boton){
				ConfigurarBotonRadial(boton);
			}
		}

		foreach (Node nodoHijo in ContenedorDeTransformaciones.GetChildren()){
			if(nodoHijo is BaseButton boton){
				ConfigurarBotonRadial(boton, true);
			}
		}

		foreach (Node nodoHijo in ContenedorDeAtaque.GetChildren()){
			if(nodoHijo is BaseButton boton){
				ConfigurarBotonRadial(boton);
			}
		}
	}

	private void ConfigurarBotonRadial(BaseButton boton, bool esTransformacion = false){
		Vector2 tamano = boton.Size;
		if (tamano == Vector2.Zero) tamano = boton.GetCombinedMinimumSize();

		float escalaFinal = 1f;
		if (esTransformacion && tamano.X > 0f){
			escalaFinal = AnchoBotonTransformacion / tamano.X;
		}
		boton.SetMeta("escala_final", escalaFinal);
		
		boton.PivotOffset = tamano  / 2;
		boton.Position = Vector2.Zero - (tamano  / 2);
		boton.Scale = Vector2.Zero;
		boton.Pressed += () => SeleccionarOpcion(boton.Name);
	}


	public void DesplegarEnPosicion(Vector2 posicionGlobal){
		var jugador = GameManager.Instance.jugadorEnTurno;
		GlobalPosition = posicionGlobal;
		if (Activo){
			OcultarMenu();

			if (ContenedorDeTransformaciones.Visible){
				OcultarTransformaciones();
				jugador.ModoTransformacion = false;
			}

			if (ContenedorDeAtaque.Visible){
				OcultarAtaques();
				jugador.ModoAtaque = false;
				jugador.AtaqueAObjetivo = false;
				hud.playerManager.vfxManager.EsconderOutlineDeJugadores();
				hud.playerManager.vfxManager.OcultarAbejasObjetivo();
				hud.playerManager.celdasManager.DespintarCeldaDeAbejaBuffeada();
			}

			if (jugador.ModoInvocacion){
				jugador.ModoInvocacion = false;
				hud.celdasManager.OcultarCeldasDisponiblesParaInvocar();
			}

			if (jugador.ModoAbsorcion){
				jugador.ModoAbsorcion = false;
				hud.tropasManager.OcultarAbejasCercanas();
			}
		} else if (hud != null && hud.tocoBotonAtacarYDebeDesplegarseElMenu){
			MostrarAtaques();
		} else {
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
		AnimarAperturaContenedor(ContenedorDeTransformaciones, deshabilitarBotones: true, radioPersonalizado: RadioTransformaciones);

		if(hud?.playerManager != null){
			hud.playerManager.TransformacionSeleccionada = null;
		}
		if(hud?.tropasManager != null){
			hud.tropasManager.ObtenerSubditosTransformables();
			MostrarTransformacionesDisponibles();
		}
	}

	public void MostrarAtaques(){
		Activo = true;
		ContenedorDeAtaque.Show();
		Show();
		MoveToFront();
		AnimarAperturaContenedor(ContenedorDeAtaque, deshabilitarBotones: true);
		if (hud != null) hud.ActualizarUIAtaques();
	}

	public void OcultarMenu(){
		OcultarContenedor(ContenedorDeBotones);
	}

	public void OcultarTransformaciones(){
		var jugador = GameManager.Instance.jugadorEnTurno;
		OcultarContenedor(ContenedorDeTransformaciones);
		/* jugador.ModoTransformacion = false; */
		if (jugador != null) jugador.ModoTransformacion = false;
		if (hud?.playerManager != null){
			hud.playerManager.TransformacionSeleccionada = null;
		}
	}

	public void OcultarAtaques(){
		if(hud != null) hud.tocoBotonAtacarYDebeDesplegarseElMenu = false;
		OcultarContenedor(ContenedorDeAtaque);
	}

	public void EstablecerHUD(HudInferior hudInferior){
		hud = hudInferior;
	}

	private void AnimarAperturaContenedor(Control contenedor, bool deshabilitarBotones = false, float? radioPersonalizado = null){
		contenedoresCerrandose.Remove(contenedor);
		
		int cantidadDeBotones = contenedor.GetChildCount();
		if (cantidadDeBotones == 0) return;

		float radio = radioPersonalizado ?? Radio;
		float espaciado = MathF.Tau / cantidadDeBotones;

		for (int b = 0; b < cantidadDeBotones; b++){
			if (contenedor.GetChild(b) is BaseButton boton){
				float angulo = b * espaciado;
				Vector2 posicionObjetivo = (new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * radio) - (boton.PivotOffset);

				float escalaFinal = boton.HasMeta("escala_final") ? (float)boton.GetMeta("escala_final") : 1f;

				Tween tween = CreateTween().SetParallel(true);
				tween.TweenProperty(boton, "position", posicionObjetivo, VelocidadAnimacion)
					.SetTrans(Tween.TransitionType.Back)
					.SetEase(Tween.EaseType.Out);

				tween.TweenProperty(boton, "scale", Vector2.One * escalaFinal, VelocidadAnimacion)
					.SetTrans(Tween.TransitionType.Back)
					.SetEase(Tween.EaseType.Out);

				boton.Disabled = deshabilitarBotones;
			}
		}
	}

	private void OcultarContenedor(Control contenedor){
		contenedoresCerrandose.Add(contenedor);
		
		foreach (var hijo in contenedor.GetChildren()){
			if (hijo is BaseButton boton){
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

		Activo = (ContenedorDeBotones.Visible && !contenedoresCerrandose.Contains(ContenedorDeBotones))
		      || (ContenedorDeTransformaciones.Visible && !contenedoresCerrandose.Contains(ContenedorDeTransformaciones))
		      || (ContenedorDeAtaque.Visible && !contenedoresCerrandose.Contains(ContenedorDeAtaque));

		GetTree().CreateTimer(VelocidadAnimacion).Timeout += () =>{
        	if (!contenedoresCerrandose.Remove(contenedor)) return;
        	if (contenedor != null && IsInstanceValid(contenedor)) {
				contenedor.Hide();
			}
        	Activo = ContenedorDeBotones.Visible
        	      || ContenedorDeTransformaciones.Visible
        	      || ContenedorDeAtaque.Visible;
    	};
	}
	
	private void SeleccionarOpcion(string nombreBoton){
		if(hud == null) return;
		var jugadorEnTurno = GameManager.Instance.jugadorEnTurno;
		if (GameManager.Instance.modoMultiplayer){
			long idLocal = Multiplayer.GetUniqueId();
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
				//hud.tropasManager.ObtenerSubditosTransformables();
				//MostrarTransformacionesDisponibles();
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
				if (jugadorEnTurno.AtaqueAObjetivo)
				{
					GD.Print("Deshabilitando boton de ataque objetivo");
					hud.tocoBotonAtacarYDebeDesplegarseElMenu = false;
					jugadorEnTurno.ModoAtaque = false;
					jugadorEnTurno.AtaqueAObjetivo = false;
					hud.playerManager.vfxManager.EsconderOutlineDeJugadores();
					hud.playerManager.vfxManager.OcultarAbejasObjetivo();
					hud.playerManager.celdasManager.DespintarCeldaDeAbejaBuffeada();
					OcultarAtaques();
					//break;
				}
				else
				{
					GD.Print("Habilitando ataque objetivo");
					jugadorEnTurno.AtaqueAObjetivo = true;
					hud.AtaqueAObjetivo();
					OcultarAtaques();
					//break
				}
				break;
			case "ComandarButton":
				jugadorEnTurno.ModoAtaque = true;
				hud.ComandarAtaque();
				jugadorEnTurno.ModoAtaque = false;
				jugadorEnTurno.AtaqueAObjetivo = false;
				OcultarAtaques();
				hud.SolicitarActualizarUI();
				break;
			case "DestruirButton":	
				if(hud.playerManager.tropasManager.SubditoActual is AbejaObrera subdito && subdito.ModoDestruccion)
				{
					hud.celdasManager.OcultarCeldasReforzadasCercanas();
					jugadorEnTurno.ModoAtaque = false;
					subdito.ModoDestruccion = false;
					OcultarAtaques();
					//break;
				}
				else
				{
					hud.DestruirCeldasReforzadas();
					OcultarAtaques();
					hud.playerManager.tropasManager.SubditoActual.HabilidadDeAbeja();
					//break;
				}
				break;
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
		var jugador = GameManager.Instance.jugadorEnTurno;
		TransformacionesDisponiblesActualmente.Clear();
		if (jugador == null || jugador.ColmenaDeReina == null) return;

		foreach (var abeja in hud.tropasManager.AbejasTransformables){
			if(abeja == null) continue;
			var opciones = GameManager.Instance.ObtenerTransformacionesDisponibles(abeja);
			foreach (var t in opciones){
				if(t == null) continue;
				int cantActual = jugador.ColmenaDeReina.CantidadTotalDeUnRecurso(t.RecursoDeTransformacion);
				if(cantActual >= t.RecursosNecesarios && abeja.AptaParaTransformarEn(t)){
					TransformacionesDisponiblesActualmente.Add(t); 
				}
			}
		}
	}

	public void MostrarTransformacionesDisponibles(){
		VerificarTransformacionesDisponibles();
		foreach (var transformacion in TransformacionesDisponiblesActualmente){
			string nombreClase = transformacion.GetType().Name;
			var boton = ContenedorDeTransformaciones.GetNodeOrNull<BaseButton>(nombreClase)
				?? ContenedorDeTransformaciones.GetNodeOrNull<BaseButton>(nombreClase + "Button");

			//ver qué transformaciones hay disponibles
			if(boton != null){
				boton.Disabled = false;
				GD.Print($"[Transformar] Boton {boton.Name} habilitado.");
				}
		}
	}

}
