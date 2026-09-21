using Godot;

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
