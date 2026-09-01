using Godot;
using System.Collections.Generic;

public partial class PlayerManager : Node3D
{
	[Export] private Camera3D camera;

	private List<Node3D> VisualesJugadores = new List<Node3D>();
	private Vector3 posicionNueva; 
	private Node3D ReinaDelJugador; 

	public override void _Ready()
	{
		InstanciarJugadores();
		EstablecerSpawns();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("move"))
		{
			if (GetViewport().GuiGetHoveredControl() != null)
				return;

			if (camera == null)
				camera = GetViewport().GetCamera3D();

			// Solo movemos si el raycast encontró una posición válida
			if (ObtenerPosicionNueva())
			{
				MoverAbeja();
			}
		}
	}

	public bool ObtenerPosicionNueva()
	{
		Vector2 mousePosition = GetViewport().GetMousePosition();
		Vector3 rayOrigin = camera.ProjectRayOrigin(mousePosition);
		Vector3 rayEnd = rayOrigin + camera.ProjectRayNormal(mousePosition) * 1000.0f;

		var spaceState = GetWorld3D().DirectSpaceState;
		var query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);

		var result = spaceState.IntersectRay(query);

		if (result.Count > 0)
		{
			// Casteo directo desde Godot.Variant a Vector3
			posicionNueva = result["position"].AsVector3();
			GD.Print("Nueva posición guardada: " + posicionNueva);
			return true;
		}

		GD.Print("El raycast no chocó con ningún objeto 3D con colisión.");
		return false;
	}

	public void MoverAbeja()
	{
		if (VisualesJugadores.Count > 0)
		{
			ReinaDelJugador = VisualesJugadores[0]; 

			if (ReinaDelJugador != null)
			{
				ReinaDelJugador.GlobalPosition = posicionNueva;
			}
		}
	}

	public void Atacar()
	{
		
	}	
	

	public void InstanciarJugadores()
	{
		var VisualReina1 = GD.Load<PackedScene>("res://Scenes/AbejaReina.tscn");
		
		for (int j = 0; j < GameManager.Instance.cantidadJugadores; j++)
		{
			var InstanciaNueva = VisualReina1.Instantiate<Node3D>();
			AddChild(InstanciaNueva);
			VisualesJugadores.Add(InstanciaNueva);
		}
	}

	public void EstablecerSpawns()
	{ 
		List<Vector3> posiciones = new List<Vector3>() 
		{
			new Vector3(-6, 0, 2), 
			new Vector3(25, 0, 3), 
			new Vector3(3, 0, 14), 
			new Vector3(19, 0, 19)
		};

		for (int i = 0; i < VisualesJugadores.Count; i++)
		{
			if (i < posiciones.Count)
			{
				VisualesJugadores[i].GlobalPosition = posiciones[i];
			}
		}
	}
}
