using Godot;

public class Celda : PosicionCelda
{
	// Referencia al hexágono que vemos en Godot
	public Node3D Tile { get; set; }
	public Celda(
		int id,
		int widthRow,
		int heightRow
	) : base(id, widthRow, heightRow)
	{
	}
}
