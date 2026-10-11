using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class VFXManager : Node3D
{
    [Export] PlayerManager playerManager;

    private List<MeshInstance3D> ObtenerTodosLosMeshes(Node3D visual){
		var meshes = new List<MeshInstance3D>();

		if (visual == null) return meshes;

		foreach (var node in visual.FindChildren("*", "MeshInstance3D"))
		{
			if (node is MeshInstance3D mesh)
			{
				meshes.Add(mesh);
			}
		}
		return meshes;
	}

    public void EstablecerNextPass(Node3D visual, Material materialOutline)
	{
		var meshes = ObtenerTodosLosMeshes(visual);

		foreach (var mesh in meshes)
		{
			if (mesh.Mesh == null) continue;

			int cantidadSuperficies = mesh.Mesh.GetSurfaceCount();

			for (int i = 0; i < cantidadSuperficies; i++)
			{
				var materialBase = mesh.GetActiveMaterial(i);
				
				if (materialBase == null) continue;

				if (!materialBase.IsLocalToScene())
				{
					materialBase = (Material)materialBase.Duplicate();
					mesh.SetSurfaceOverrideMaterial(i, materialBase);
				}

				if (materialOutline != null && materialBase == materialOutline)
				{
					GD.PrintErr($"[PlayerManager] Conflicto de material en {mesh.Name} (Superficie {i}): El material base y el outline son la misma instancia.");
					continue;
				}

				materialBase.NextPass = materialOutline;
			}
		}
	}

    public void MostrarJugadoresObjetivo(){
		var materialAtaque = GD.Load<StandardMaterial3D>("res://outlineAttack.tres");
		var jugadoresObjetivo = new List<Node3D>();

		if(GameManager.Instance.jugadorEnTurno.MoviendoSubdito)
		{
			var reinas = playerManager.tropasManager.AlmacenJugadores.Keys.ToList();
			jugadoresObjetivo = playerManager.VisualesJugadores.Where(j => playerManager.tropasManager.SubditoTieneAAlguienCerca(reinas[j.GetIndex()])).ToList();
		}
		else
		{
			jugadoresObjetivo = playerManager.VisualesJugadores.Where(j => playerManager.JugadorEnTurnoAdyacenteAOtro(j)).ToList();
		}

		foreach (var jugador in jugadoresObjetivo)
		{
			if (jugador != playerManager.VisualJugadorActual)
			{
				EstablecerNextPass(jugador, materialAtaque);
			}
		}
	}

    public void OutlineJugadorEnTurno(Node3D visualActual){
		var IndiceDeJugadorEnTurno = GameManager.Instance.jugadorEnTurno.Id - 1;
		
		var materialTurnoJ1 = GD.Load<StandardMaterial3D>("res://outlineJugador1.tres");
		var materialTurnoJ2 = GD.Load<StandardMaterial3D>("res://outlineJugador2.tres");
		var materialTurnoJ3 = GD.Load<StandardMaterial3D>("res://outlineJugador3.tres");
		var materialTurnoJ4 = GD.Load<StandardMaterial3D>("res://outlineJugador4.tres");

		List<StandardMaterial3D> OutlinesJugadores = new List<StandardMaterial3D>{ materialTurnoJ1, materialTurnoJ2, materialTurnoJ3, materialTurnoJ4 };

		EsconderOutlineDeJugadores();

		if (visualActual != null)
		{
			EstablecerNextPass(visualActual, OutlinesJugadores[IndiceDeJugadorEnTurno]);
		}
	}

    public void EsconderOutlineDeJugadores()
	{
		var outlineBase = GD.Load<StandardMaterial3D>("res://outlineBase.tres");
		
		foreach (var visual in playerManager.VisualesJugadores)
		{
			if (visual != playerManager.VisualJugadorActual)
			{
				EstablecerNextPass(visual, outlineBase);
			}
		}
	}

    public void MostrarAbejasObjetivo(){
		var materialAtaque = GD.Load<StandardMaterial3D>("res://outlineAttack.tres");
		var jugadoresObjetivo = new List<Node3D>();
		
		playerManager.ActualizarAbejasObjetivo();

		if(GameManager.Instance.jugadorEnTurno.MoviendoSubdito)
		{
			var reinas = playerManager.tropasManager.AlmacenJugadores.Keys.ToList();
			jugadoresObjetivo = playerManager.VisualesJugadores.Where(j => playerManager.tropasManager.SubditoTieneAAlguienCerca(reinas[j.GetIndex()])).ToList();
		}
		else
		{
			jugadoresObjetivo = playerManager.VisualesJugadores.Where(j => playerManager.JugadorEnTurnoAdyacenteAOtro(j)).ToList();
		}

		foreach (var abejaVisual in playerManager.AbejasObjetivo)
		{
			if(playerManager.tropasManager.VisualAbejas[abejaVisual] != null)
			{
				EstablecerNextPass(playerManager.tropasManager.VisualAbejas[abejaVisual], materialAtaque);
			} 
		}
	}

    public void OcultarAbejasObjetivo(){
		var outlineBase = GD.Load<StandardMaterial3D>("res://outlineBase.tres");

		foreach (var abejaVisual in playerManager.AbejasObjetivo)
		{
			if(abejaVisual != null)
			{
				EstablecerNextPass(playerManager.tropasManager.VisualAbejas[abejaVisual], outlineBase);
			} 
		}

		playerManager.AbejasObjetivo.Clear();
	}
}
