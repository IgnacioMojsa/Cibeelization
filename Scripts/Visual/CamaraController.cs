using Godot;

public partial class CamaraController : Node3D
{
	[Export] private Camera3D camera3D;

	public override void _Ready()
	{
		GameManager.Instance.CamaraActual = this;

		if (camera3D == null)
		{
			camera3D = GetNodeOrNull<Camera3D>("Camera3D");
		}
	}

	public void AjustarATablero(int filas, int columnas, float tileSize)
	{
		// Si se usó la opción de menú, elegimos la transformación según las filas
		if (filas <= 15)
		{
			AplicarCamaraSmall();
		}
		else if (filas <= 25)
		{
			AplicarCamaraMid();
		}
		else
		{
			AplicarCamaraBig();
		}
	}

	public void AplicarCamaraSmall()
	{
		// Small (15x15)
		Position = new Vector3(15.563f, 19.085f, -26.227f);
		RotationDegrees = new Vector3(-60.1f, -179.0f, -0.8f);
	}

	public void AplicarCamaraMid()
	{
		// Mid (25x25)
		Position = new Vector3(25.563f, 27.085f, -30.227f);
		RotationDegrees = new Vector3(-50.1f, -179.0f, -0.8f);
	}

	public void AplicarCamaraBig()
	{
		// Big (30x30)
		Position = new Vector3(30.563f, 31.085f, -28.227f);
		RotationDegrees = new Vector3(-55.1f, -179.0f, -0.8f);
	}
}
