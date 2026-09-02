using Godot;

/*public abstract class Celda
{
	// Identificador de la celda
	public int Id { get; protected set; }

	// Posición dentro del GridMap
	public Vector3I PosicionA { get; protected set; }
	public Vector3I PosicionB { get; protected set; }

	// Constructor
	protected Celda(int id, Vector3I posicionA, Vector3I posicionB)
	{
		Id = id;
		PosicionA = posicionA;
		PosicionB = posicionB;
	}
} */

public abstract class Celda
{
	public int Id { get; protected set; }

	public int Q { get; protected set; }
	public int R { get; protected set; }

	// Referencia al hexágono que vemos en Godot
	public Node3D Tile { get; set; }

	protected Celda(int id, int q, int r)
	{
		Id = id;
		Q = q;
		R = r;
	}
}
