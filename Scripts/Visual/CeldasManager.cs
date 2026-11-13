using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Net;

public partial class CeldasManager : Node3D
{
    [Export] public PlayerManager playerManager;
    [Export] public Tablero tablero; 
    private Tween TweenBuffArcoiris;
	private readonly List<Celda> celdasReforzadasResaltadas = new();
    public List<Celda> CeldasDisponibles = new();
	public List<Color> ColoresDeJugadores = new List<Color>{Color.FromHtml("#6a268f"), Color.FromHtml("#4c1dae"), Color.FromHtml("#b33671"), Color.FromHtml("#8260e5")};

    public void PintarCelda(Celda unaCelda, Color unColor)
	{
		var hexagono = unaCelda.Tile.GetNode<Node3D>("hexagon_tile");
		var materialDeMesh = hexagono.GetChild<MeshInstance3D>(0).GetActiveMaterial(0); 

		StandardMaterial3D nuevoMaterial = (StandardMaterial3D)materialDeMesh.Duplicate();
		nuevoMaterial.AlbedoColor = unColor;

		hexagono.GetChild<MeshInstance3D>(0).SetSurfaceOverrideMaterial(0, nuevoMaterial);

		unaCelda.EstaPintada = true;
		if(ColoresDeJugadores.Contains(unColor))
		{
			unaCelda.ColorActual = unColor;
		}
	}

    public void PintarCeldaDeAbejaBuffeada()
	{
		var celdaAPintar = GameManager.Instance.jugadorEnTurno.CeldaActual.Tile.GetNode<Node3D>("hexagon_tile").GetChild(0);

		if (celdaAPintar is MeshInstance3D meshInstance)
		{
			if (TweenBuffArcoiris != null && TweenBuffArcoiris.IsValid())
			{
				TweenBuffArcoiris.Kill();
			}

			StandardMaterial3D material = meshInstance.GetSurfaceOverrideMaterial(0) as StandardMaterial3D;
			if (material == null)
			{
				material = new StandardMaterial3D();
			}
			else
			{
				material = (StandardMaterial3D)material.Duplicate();
			}

			meshInstance.SetSurfaceOverrideMaterial(0, material);

			TweenBuffArcoiris = CreateTween().SetLoops();
			TweenBuffArcoiris.TweenMethod(Callable.From<float>((hue) => 
			{
				material.AlbedoColor = Color.FromHsv(hue, 1.0f, 1.0f);
			}), 0.0f, 1.0f, 2.0f);
		}
	}

    public void DespintarCeldaDeAbejaBuffeada()
	{

		if (TweenBuffArcoiris != null && TweenBuffArcoiris.IsValid())
		{
			TweenBuffArcoiris.Kill();
		}

		var celda = GameManager.Instance.jugadorEnTurno.CeldaActual;

		if (celda.Tile == null) return;

		var nodoHexagon = celda.Tile.GetNode<Node3D>("hexagon_tile").GetChild(0);

		if (nodoHexagon is MeshInstance3D meshInstance)
		{
			StandardMaterial3D materialDeMesh = meshInstance.GetActiveMaterial(0) as StandardMaterial3D;
	
			StandardMaterial3D nuevoMaterial;
			if (materialDeMesh != null)
			{
				nuevoMaterial = (StandardMaterial3D)materialDeMesh.Duplicate();
			}
			else
			{
				nuevoMaterial = new StandardMaterial3D();
			}

			if(GameManager.Instance.jugadorEnTurno.AtaquePotenciado)
			{
				nuevoMaterial.AlbedoColor = Color.Color8(201, 113, 0, 255);
				meshInstance.SetSurfaceOverrideMaterial(0, nuevoMaterial);
			}
		}
	}

    public void MostrarCeldasDisponiblesParaInvocar(){
		playerManager.VisualJugadorActual = playerManager.VisualesJugadores[GameManager.Instance.jugadorEnTurno];

		CeldasDisponibles = tablero.ObtenerVecinos(GameManager.Instance.jugadorEnTurno.CeldaActual);
		
		foreach (var jugador in playerManager.VisualesJugadores.Values)
		{
			if(playerManager.JugadorEnTurnoAdyacenteAOtro(jugador)){
				var celdaOcupada = playerManager.CeldaActualPorJugador[jugador];

				CeldasDisponibles = CeldasDisponibles.Where(c => c != celdaOcupada).ToList();
				GD.Print("Hay otro jugador cerca");
			}
		}

        HashSet<Celda> celdasOcupadasPorAbejas = new HashSet<Celda>();

		foreach (var reina in GameManager.Instance.JugadoresEnPartida)
		{
			foreach (var abeja in reina.ColmenaDeReina.AbejasDeColmena)
            {
                if (!abeja.FueraDeJuego && abeja.CeldaActual != null)
                {
                    celdasOcupadasPorAbejas.Add(abeja.CeldaActual);
                }
            }
		}

        CeldasDisponibles = CeldasDisponibles.Where(celda => !celdasOcupadasPorAbejas.Contains(celda)).ToList();

		foreach (var celda in CeldasDisponibles)
		{
			PintarCelda(celda, Color.FromHtml("#d72f00"));
		}
	}

    public void OcultarCeldasDisponiblesParaInvocar(){
		foreach (var celda in CeldasDisponibles)
		{
			if(!celda.EstaPintada)
			{
				PintarCelda(celda, Color.FromHtml("#f9b600"));
			}
			else
			{
				PintarCelda(celda, celda.ColorActual); 
			}
		}
	}

	public void PintarCeldaDeAbeja(Celda unaCelda, Abeja unaAbeja)
	{
		var jugadorActual = unaAbeja.ColmenaHogar.ReinaDeColmena;
		int indiceJugador = GameManager.Instance.JugadoresEnPartida.IndexOf(jugadorActual);

		var hexagono = unaCelda.Tile.GetNode<Node3D>("hexagon_tile");
		var materialDeMesh = hexagono.GetChild<MeshInstance3D>(0).GetActiveMaterial(0); 

		StandardMaterial3D nuevoMaterial = (StandardMaterial3D)materialDeMesh.Duplicate();
		nuevoMaterial.AlbedoColor = ColoresDeJugadores[indiceJugador];

		hexagono.GetChild<MeshInstance3D>(0).SetSurfaceOverrideMaterial(0, nuevoMaterial);

		unaCelda.EstaPintada = true;
		unaCelda.ColorActual = ColoresDeJugadores[indiceJugador];
		unaCelda.Ocupante = unaAbeja;
		
		foreach (var jugador in GameManager.Instance.JugadoresEnPartida)
		{
			if (jugador.ColmenaDeReina.CeldasEnPropiedad.Contains(unaCelda))
			{
				jugador.ColmenaDeReina.CeldasEnPropiedad.Remove(unaCelda);
			}
		}

		GameManager.Instance.jugadorEnTurno.ColmenaDeReina.CeldasEnPropiedad.Add(unaCelda);
		
		GD.Print(jugadorActual.GetType() + " " + jugadorActual.Id + " tiene un " + tablero.PorcentajeDelTableroOcupadoPorUnaReina(jugadorActual) + "% del tablero pintado");
	}

	public void DespintarCeldaDeAbeja(Celda unaCelda)
	{
		var hexagono = unaCelda.Tile.GetNode<Node3D>("hexagon_tile");
		var materialDeMesh = hexagono.GetChild<MeshInstance3D>(0).GetActiveMaterial(0); 

		StandardMaterial3D nuevoMaterial = (StandardMaterial3D)materialDeMesh.Duplicate();
		nuevoMaterial.AlbedoColor = Color.FromHtml("#f9b600");

		hexagono.GetChild<MeshInstance3D>(0).SetSurfaceOverrideMaterial(0, nuevoMaterial);

		unaCelda.EstaPintada = false;
		unaCelda.ColorActual = Color.FromHtml("#f9b600");
		unaCelda.Ocupante = null;
	}

	public void ReforzarCelda(Celda unaCelda, Abeja unaAbeja)
	{
		var jugadorActual = unaAbeja.ColmenaHogar.ReinaDeColmena;
		int indiceJugador = GameManager.Instance.JugadoresEnPartida.IndexOf(jugadorActual);
		
		var visualFortificada = unaCelda.Tile.GetNode<Node3D>("hexagon_fortified");
		var materialFortificacion = visualFortificada.GetChild<MeshInstance3D>(0).GetActiveMaterial(0);

		StandardMaterial3D nuevoMaterial = (StandardMaterial3D)materialFortificacion.Duplicate();
		nuevoMaterial.AlbedoColor = ColoresDeJugadores[indiceJugador];

		//visualFortificada.GetChild<MeshInstance3D>(0).SetSurfaceOverrideMaterial(0, nuevoMaterial); // Lo desactive por ahora pero porque me gusta mas como queda con el amarillo
		visualFortificada.Visible = true;
		unaCelda.EstaReforzada = true;
	}

	public bool ExistenCeldasReforzadasCerca(Abeja unaAbeja)
	{
		return tablero.ObtenerVecinos(unaAbeja.CeldaActual).Any(c => c.EstaReforzada);
	}

	public void MostrarCeldasReforzadasCercanas(Abeja unaAbeja)
	{
		OcultarCeldasReforzadasCercanas();

		var materialAtaque = GD.Load<StandardMaterial3D>("res://Assets/Resources/outlineAttack.tres");
		celdasReforzadasResaltadas.AddRange(tablero.ObtenerVecinos(unaAbeja.CeldaActual).Where(c => c.EstaReforzada));

		foreach (var celda in celdasReforzadasResaltadas)
		{
			var refuerzo = celda.Tile.GetNode<Node3D>("hexagon_fortified");
			
			playerManager.vfxManager.EstablecerNextPass(refuerzo, materialAtaque);
		}
	}

	public void OcultarCeldasReforzadasCercanas()
	{
		var outlineBase = GD.Load<StandardMaterial3D>("res://Assets/Resources/outlineCeldas.tres");

		foreach (var celda in celdasReforzadasResaltadas)
		{
			var refuerzo = celda.Tile.GetNode<Node3D>("hexagon_fortified");
			playerManager.vfxManager.EstablecerNextPass(refuerzo, outlineBase);
		}
		celdasReforzadasResaltadas.Clear();
	}

	public bool DestruirCeldaReforzada(Celda unaCelda)
	{
		if(unaCelda == null) return false;

		var subdito = playerManager.tropasManager.SubditoActual;
		if(subdito is not AbejaObrera obrera || !obrera.ModoDestruccion) return false; 

		if(!unaCelda.EstaReforzada || !tablero.ObtenerVecinos(subdito.CeldaActual).Contains(unaCelda))
		{
			EventosUI.MostrarMensaje("Esa celda no esta reforzada, no puede ser destruida");
			return false;
		}
	
		unaCelda.EstaReforzada = false;
		unaCelda.Tile.GetNode<Node3D>("hexagon_fortified").Visible = false;
		return true;
	}
}
