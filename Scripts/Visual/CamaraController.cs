using Godot;
using System;

public partial class CamaraController : Node3D
{
	[Export] private Camera3D camera3D;
	[Export] private float paddingFactor = 1.25f; // Margen de seguridad alrededor del tablero
	[Export] private float anguloInclinacionDeg = -70.0f; // Grados de inclinación de la cámara

	public override void _Ready()
	{
		if (camera3D == null)
		{
			camera3D = GetNodeOrNull<Camera3D>("Camera3D");
		}

		// Aseguramos que el nodo Camera3D esté centrado respecto a este padre
		if (camera3D != null)
		{
			camera3D.Position = Vector3.Zero;
			camera3D.RotationDegrees = Vector3.Zero;
		}

		// Registrar la cámara en el Singleton
		GameManager.Instance.CamaraActual = this;

		// EJECUCIÓN CLAVE: Si el tablero ya existe o fue asignado, forzar el ajuste al cargar la escena
		CallDeferred(nameof(AjustarAlInicio));
	}

	private void AjustarAlInicio()
	{
		if (GameManager.Instance.TableroActual != null)
		{
			AjustarATablero(
				GameManager.Instance.DimensionActual, 
				GameManager.Instance.DimensionActual, 
				GameManager.Instance.TableroActual.TileSize
			);
		}
	}

	public void AjustarATablero(int filas, int columnas, float tileSize)
	{
		if (camera3D == null) return;
	
		// 1. Calcular el tamaño total real del tablero en unidades 3D
		// Para hexágonos en orientacion vertical:
		float anchoTablero = columnas * (tileSize * 1.732f); 
		float profundidadTablero = filas * (tileSize * 1.5f);
		float valorFloat;

		if(filas == 15)
		{
			valorFloat = 40.0f;// Small
		}
		else if(filas == 25)
		{
			valorFloat = 2.5f; // Mid
		}
		else if(filas == 30)
		{
			valorFloat = 2.0f; // Big
		}
		else
		{
			valorFloat = 40.0f; // Default
		}


	
		// 2. Calcular el centro exacto del tablero.
		// Si tu tablero se instancia en el origen (0,0,0) hacia coordenadas positivas:
		Vector3 centroTablero = new Vector3(anchoTablero / 2.0f, 0f, profundidadTablero / valorFloat);
	
		// NOTA: Si tu tablero se genera hacia el eje -Z (negativo), usa esta línea en su lugar:
		// Vector3 centroTablero = new Vector3(anchoTablero / 2.0f, 0f, -profundidadTablero / 2.0f);
	
		// 3. Tomar la dimensión más grande del tablero para definir el tamaño
		float dimensionMaxima = Mathf.Max(anchoTablero, profundidadTablero);
	
		// 4. Calcular la distancia (altura) en base al FOV de la cámara
		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		float aspectRatio = viewportSize.X / viewportSize.Y;
	
		float fovRad = Mathf.DegToRad(camera3D.Fov);
		// Ajuste trigonométrico de distancia
		float distanciaRequerida = (dimensionMaxima * paddingFactor / 2.0f) / Mathf.Tan(fovRad / 2.0f);
	
		// Ajustar si la pantalla es más alta que ancha
		if (aspectRatio < 1.0f)
		{
			distanciaRequerida /= aspectRatio;
		}
	
		// 5. Posicionar el pivote en el centro exacto del tablero
		Position = centroTablero;
	
		// 6. Colocar la cámara desplazada en Y (altura) y retraída en Z (distancia e inclinación)
		float anguloRad = Mathf.DegToRad(Mathf.Abs(anguloInclinacionDeg));
		float alturaY = distanciaRequerida * Mathf.Sin(anguloRad);
		float offsetZ = distanciaRequerida * Mathf.Cos(anguloRad);
	
		// Posicionamos la cámara desplazada desde el centro
		camera3D.Position = new Vector3(0f, alturaY, offsetZ);
	
		// 7. Forzar a la cámara a mirar directamente al centro del tablero
		camera3D.LookAt(centroTablero, Vector3.Up);
	}
}
