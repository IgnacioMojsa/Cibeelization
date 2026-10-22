using Godot;
using System;
using System.Collections.Generic;

public partial class MenuRadial : Control
{
	[Export] private GameUI hud; 
	private Control ContenedorDeBotones;
	private Control ContenedorDeTransformaciones;
	private HashSet<Abeja> TransformacionesDisponiblesActualmente = new HashSet<Abeja>();
	public bool Activo = false;
	public Vector2 PosicionCentral;
	public float Radio = 110f;
	public float VelocidadAnimacion = 0.25f;

	public override void _Ready()
	{
		ContenedorDeBotones = GetNode<Control>("%ContenedorBotones");
		ContenedorDeTransformaciones = GetNode<Control>("%ContenedorTransformaciones");

		PosicionCentral = GetViewportRect().Size / 2 - ContenedorDeBotones. GetRect().Size / 2;
		ContenedorDeBotones.Hide();
		ContenedorDeTransformaciones.Hide();

		foreach (Node nodoHijo in ContenedorDeBotones.GetChildren()) 
		{
			if(nodoHijo is Button boton)
			{
				boton.PivotOffset = boton.Size / 2;
				boton.Position = Vector2.Zero - (boton.Size / 2);
				boton.Scale = Vector2.Zero;

				boton.Pressed += () => SeleccionarOpcion(boton.Name);
			}
		}

		foreach (Node nodoHijo in ContenedorDeTransformaciones.GetChildren()) 
		{
			if(nodoHijo is Button boton)
			{
				boton.PivotOffset = boton.Size / 2;
				boton.Position = Vector2.Zero - (boton.Size / 2);
				boton.Scale = Vector2.Zero;

				boton.Pressed += () => SeleccionarOpcion(boton.Name);
			}
		}
	}

	public void EstablecerHUD(GameUI gameUI)
	{
		hud = gameUI;
	}

	public void DesplegarEnPosicion(Vector2 posicionGlobal)
	{
		GlobalPosition = posicionGlobal;

		if (Activo)
		{
			OcultarMenu();
			
			if (ContenedorDeTransformaciones.Visible){
				OcultarTransformaciones();
				GameManager.Instance.jugadorEnTurno.ModoTransformacion = false;
			}
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

	public void MostrarMenu()
	{
		Activo = true;
		ContenedorDeBotones.Show();
		Show();
		MoveToFront();
		
		int cantidadDeBotones = ContenedorDeBotones.GetChildCount();
		if (cantidadDeBotones == 0) return;

		float espaciado = MathF.Tau / cantidadDeBotones;

		for (int b = 0; b < cantidadDeBotones; b++)
		{
			if(ContenedorDeBotones.GetChild(b) is Button boton)
			{
				float angulo = b * espaciado;
				Vector2 posicionObjetivo = (new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * Radio) - (boton.Size / 2);

				Tween tween = CreateTween().SetParallel(true);
				tween.TweenProperty(boton, "position", posicionObjetivo, VelocidadAnimacion)
					.SetTrans(Tween.TransitionType.Back)
					.SetEase(Tween.EaseType.Out);

				tween.TweenProperty(boton, "scale", Vector2.One, VelocidadAnimacion)
					.SetTrans(Tween.TransitionType.Back)
					.SetEase(Tween.EaseType.Out);
			}
		}
	}

	public void MostrarTransformaciones()
	{
		Activo = true;
		ContenedorDeTransformaciones.Show();
		Show();
		MoveToFront();
		
		int cantidadDeBotones = ContenedorDeTransformaciones.GetChildCount();
		if (cantidadDeBotones == 0) return;

		float espaciado = MathF.Tau / cantidadDeBotones;

		for (int b = 0; b < cantidadDeBotones; b++)
		{
			if(ContenedorDeTransformaciones.GetChild(b) is Button boton)
			{
				float angulo = b * espaciado;
				Vector2 posicionObjetivo = (new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * Radio) - (boton.Size / 2);

				Tween tween = CreateTween().SetParallel(true);

				tween.TweenProperty(boton, "position", posicionObjetivo, VelocidadAnimacion)
					.SetTrans(Tween.TransitionType.Back)
					.SetEase(Tween.EaseType.Out);

				tween.TweenProperty(boton, "scale", Vector2.One, VelocidadAnimacion)
					.SetTrans(Tween.TransitionType.Back)
					.SetEase(Tween.EaseType.Out);
				
				boton.Disabled = true;
			}
		}

		GameManager.Instance.jugadorEnTurno.ModoTransformacion = true;
	}

	public void OcultarMenu()
	{
		int cantidadBotones = ContenedorDeBotones.GetChildCount();

		for (int i = 0; i < cantidadBotones; i++)
		{
			if (ContenedorDeBotones.GetChild(i) is Button boton)
			{
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

		GetTree().CreateTimer(VelocidadAnimacion).Timeout += () =>
		{
			ContenedorDeBotones.Hide();
			Activo = ContenedorDeBotones.Visible || ContenedorDeTransformaciones.Visible;
		};
	}

	public void OcultarTransformaciones()
	{
		foreach (var hijo in ContenedorDeTransformaciones.GetChildren())
		{
			if (hijo is Button boton)
			{
				Tween tween = CreateTween().SetParallel(true);
				tween.TweenProperty(boton, "position", Vector2.Zero - (boton.Size / 2), VelocidadAnimacion)
					.SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.In);
				tween.TweenProperty(boton, "scale", Vector2.Zero, VelocidadAnimacion)
					.SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.In);
			}
		}

		GetTree().CreateTimer(VelocidadAnimacion).Timeout += () =>
		{
			ContenedorDeTransformaciones.Hide();
			Activo = ContenedorDeBotones.Visible || ContenedorDeTransformaciones.Visible;
		};
	}
	
	private void SeleccionarOpcion(string nombreBoton)
	{
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
		}
		
		OcultarMenu();
	}

	public void ActualizarEstadoBoton(bool condicion, string unBoton)
	{
		if (ContenedorDeBotones.HasNode(unBoton))
		{
			var boton = ContenedorDeBotones.GetNode<Button>(unBoton);
			boton.Disabled = !condicion;
		}
	}

	public void VerificarTransformacionesDisponibles()
	{
		TransformacionesDisponiblesActualmente.Clear();
		
		foreach (var abeja in hud.tropasManager.AbejasTransformables)
		{
			if(abeja == null) continue;
			
			List<Abeja> transformacionesObtenidas = new List<Abeja>();
			
			foreach (var t in GameManager.Instance.ObtenerTransformacionesDisponibles(abeja))
			{
				if (GameManager.Instance.jugadorEnTurno.ColmenaDeReina.CantidadTotalDeUnRecurso(t.RecursoDeTransformacion) >= t.RecursosNecesarios)
					TransformacionesDisponiblesActualmente.Add(t); 
			}
		}
	}

	public void MostrarTransformacionesDisponibles()
	{
		VerificarTransformacionesDisponibles();

		foreach (var transformacion in TransformacionesDisponiblesActualmente)
		{
			var boton = ContenedorDeTransformaciones.GetNodeOrNull<Button>(transformacion.GetType().Name);
			if (boton != null) boton.Disabled = false;
		}
	}

}
