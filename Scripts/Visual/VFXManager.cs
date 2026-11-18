using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class VFXManager : Node3D
{
	[Export] PlayerManager playerManager;
	[Export] public AttackManager attackManager;

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
					GD.PrintErr($"[VFXManager] Conflicto de material en {mesh.Name} (Superficie {i}): El material base y el outline son la misma instancia.");
					continue;
				}

				materialBase.NextPass = materialOutline;
			}
		}
	}

	public void MostrarJugadoresObjetivo(){
		var materialAtaque = GD.Load<StandardMaterial3D>("res://Assets/Resources/outlineAttack.tres");
		List<Node3D> jugadoresObjetivo;

		if (GameManager.Instance.jugadorEnTurno.MoviendoSubdito)
		{
			jugadoresObjetivo = playerManager.VisualesJugadores
				.Where(kvp => !kvp.Key.FueraDeJuego && playerManager.tropasManager.SubditoTieneAAlguienCerca(kvp.Key))
				.Select(kvp => kvp.Value)
				.ToList();
		}
		else
		{
			jugadoresObjetivo = playerManager.VisualesJugadores.Values
				.Where(j => playerManager.JugadorEnTurnoAdyacenteAOtro(j))
				.ToList();
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
		var jugadorEnTurno = GameManager.Instance.jugadorEnTurno;
		if (jugadorEnTurno == null) return;

		int indiceDeJugadorEnTurno = GameManager.Instance.JugadoresEnPartida.IndexOf(jugadorEnTurno);
		
		var materialTurnoJ1 = GD.Load<StandardMaterial3D>("res://Assets/Resources/outlineJugador1.tres");
		var materialTurnoJ2 = GD.Load<StandardMaterial3D>("res://Assets/Resources/outlineJugador2.tres");
		var materialTurnoJ3 = GD.Load<StandardMaterial3D>("res://Assets/Resources/outlineJugador3.tres");
		var materialTurnoJ4 = GD.Load<StandardMaterial3D>("res://Assets/Resources/outlineJugador4.tres");

		List<StandardMaterial3D> outlinesJugadores = new List<StandardMaterial3D>{ materialTurnoJ1, materialTurnoJ2, materialTurnoJ3, materialTurnoJ4 };

		EsconderOutlineDeJugadores();

		if (visualActual != null && indiceDeJugadorEnTurno >= 0 && indiceDeJugadorEnTurno < outlinesJugadores.Count)
		{
			EstablecerNextPass(visualActual, outlinesJugadores[indiceDeJugadorEnTurno]);
		}
	}

	public void EsconderOutlineDeJugadores()
	{
		var outlineBase = GD.Load<StandardMaterial3D>("res://Assets/Resources/outlineBase.tres");
		
		foreach (var visual in playerManager.VisualesJugadores.Values)
		{
			if (visual != playerManager.VisualJugadorActual)
			{
				EstablecerNextPass(visual, outlineBase);
			}
		}
	}

	public void MostrarAbejasObjetivo(){
		var materialAtaque = GD.Load<StandardMaterial3D>("res://Assets/Resources/outlineAttack.tres");

		attackManager.ActualizarAbejasObjetivo();

		foreach (var abejaObjetivo in attackManager.AbejasObjetivo)
		{
			if (abejaObjetivo != null && playerManager.tropasManager.VisualAbejas.TryGetValue(abejaObjetivo, out Node3D visual) && IsInstanceValid(visual))
			{
				EstablecerNextPass(visual, materialAtaque);
			} 
		}
	}

	public void OcultarAbejasObjetivo(){
		var outlineBase = GD.Load<StandardMaterial3D>("res://Assets/Resources/outlineBase.tres");

		foreach (var abejaObjetivo in attackManager.AbejasObjetivo)
		{
			if (abejaObjetivo != null && playerManager.tropasManager.VisualAbejas.TryGetValue(abejaObjetivo, out Node3D visual) && IsInstanceValid(visual))
			{
				EstablecerNextPass(visual, outlineBase);
			} 
		}

		playerManager.AbejasObjetivo.Clear();
	}

	public void EstablecerUnOutlineAAbeja(string material, Node3D abeja)
	{
		var outline = GD.Load<StandardMaterial3D>(material);

		EstablecerNextPass(abeja, outline);
	}
}