using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class CeldasManager : Node3D
{
    [Export] public PlayerManager playerManager;
    [Export] public Tablero tablero; 
    private Tween TweenBuffArcoiris;
    public List<Celda> CeldasDisponibles = new();

    public void PintarCelda(Celda unaCelda, Color unColor)
	{
		var hexagono = unaCelda.Tile.GetNode<Node3D>("hexagon_tile");
		var materialDeMesh = hexagono.GetChild<MeshInstance3D>(0).GetActiveMaterial(0); 

		StandardMaterial3D nuevoMaterial = (StandardMaterial3D)materialDeMesh.Duplicate();
		nuevoMaterial.AlbedoColor = unColor;

		hexagono.GetChild<MeshInstance3D>(0).SetSurfaceOverrideMaterial(0, nuevoMaterial);
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
		playerManager.VisualJugadorActual = playerManager.VisualesJugadores[GameManager.Instance.jugadorEnTurno.Id - 1];

		CeldasDisponibles = tablero.ObtenerVecinos(GameManager.Instance.jugadorEnTurno.CeldaActual);
		
		foreach (var jugador in playerManager.VisualesJugadores)
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
			PintarCelda(celda, Color.Color8(201, 113, 0, 255));
		}
	}

	public void PintarCeldaDeAbeja(Celda unaCelda, Abeja unaAbeja)
	{
		var jugadorActual = unaAbeja.ColmenaHogar.ReinaDeColmena;
		int indiceJugador = GameManager.Instance.JugadoresEnPartida.IndexOf(jugadorActual);
		
		var colorJ1 = Color.Color8(106, 38, 143, 255);
		var colorJ2 = Color.Color8(76, 29, 174, 255);
		var colorJ3 = Color.Color8(179, 54, 113, 255);
		var colorJ4 = Color.Color8(130, 96, 229, 255); 

		List<Color> coloresDeJugadores = new List<Color>{colorJ1, colorJ2, colorJ3, colorJ4};
		var hexagono = unaCelda.Tile.GetNode<Node3D>("hexagon_tile");
		var materialDeMesh = hexagono.GetChild<MeshInstance3D>(0).GetActiveMaterial(0); 

		StandardMaterial3D nuevoMaterial = (StandardMaterial3D)materialDeMesh.Duplicate();
		nuevoMaterial.AlbedoColor = coloresDeJugadores[indiceJugador];

		hexagono.GetChild<MeshInstance3D>(0).SetSurfaceOverrideMaterial(0, nuevoMaterial);
	}

	public void DespintarCeldaDeAbeja(Celda unaCelda)
	{
		var hexagono = unaCelda.Tile.GetNode<Node3D>("hexagon_tile");
		var materialDeMesh = hexagono.GetChild<MeshInstance3D>(0).GetActiveMaterial(0); 

		StandardMaterial3D nuevoMaterial = (StandardMaterial3D)materialDeMesh.Duplicate();
		nuevoMaterial.AlbedoColor = Color.Color8(201, 113, 0, 255);

		hexagono.GetChild<MeshInstance3D>(0).SetSurfaceOverrideMaterial(0, nuevoMaterial);
	}
}
