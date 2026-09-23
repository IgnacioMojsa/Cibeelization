using Godot;

public abstract class PosicionCelda
{
	public int Id { get; protected set; }

	public int Q { get; protected set; }
	public int R { get; protected set; }

	protected PosicionCelda(int id, int q, int r)
	{
		Id = id;
		Q = q;
		R = r;
	}
}
