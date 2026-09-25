using Godot;
using System;

public partial class CamaraController : Node3D
{
	[Export] private Camera3D camera3D;
	[Export] private float paddingFactor = 1.25f; // Margen de seguridad alrededor del tablero
	[Export] private float anguloInclinacionDeg = -70.0f; // Grados de inclinación de la cámara

	[Export] private float alturaZoomTurno = 11.0f;  
	[Export] private float offsetZTurno = 6.0f;     
	[Export] private float duracionAnimacion = 0.8f;
	[Export] private float velocidadRotacion = 0.07f;

	private bool controlJugador = false;
	private Vector2 ultimaPosMouse;
	[Export] private float velocidadManual = 0.04f;

	private bool enModoOrbita = false;

	private Tween tweenCamara;

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

	public override void _Process(double delta)
	{
	    if (!controlJugador && enModoOrbita)
	    {
	        RotateY((float)delta * velocidadRotacion);
	    }
	}


	public override void _Input(InputEvent @event)
	{
	    if (@event is InputEventMouseButton mouseEvent)
	    {
	        if (mouseEvent.ButtonIndex == MouseButton.Right)
	        {
	            if (mouseEvent.Pressed)
	            {
	                controlJugador = true;
	                enModoOrbita = false;
	                ultimaPosMouse = mouseEvent.Position; 
	            }
	            else
	            {
	                controlJugador = false; 
	            }
	        }
	    }
	    else if (@event is InputEventMouseMotion motionEvent && controlJugador)
	    {
	        float deltaX = motionEvent.Position.X - ultimaPosMouse.X;
	        ultimaPosMouse = motionEvent.Position;

	        RotateY(deltaX * velocidadManual * -1f);
	    }
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

		enModoOrbita = false;
	
		float anchoTablero = columnas * (tileSize * 1.732f); 
		float profundidadTablero = filas * (tileSize * 1.5f);
		float valorFloat;

		if(filas == 15)
		{
			valorFloat = 40.0f;
		}
		else if(filas == 25)
		{
			valorFloat = 2.5f; 
		}
		else if(filas == 30)
		{
			valorFloat = 2.0f;
		}
		else
		{
			valorFloat = 40.0f;
		}

		Vector3 centroTablero = new Vector3(anchoTablero / 2.0f, 0f, profundidadTablero / valorFloat);

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

	public void EnfocarNodo(Node3D objetivo, float multiplicadorPivote, float multiplicadorCamara)
	{
		if (objetivo == null || camera3D == null) return;

		enModoOrbita = false;

		if (tweenCamara != null && tweenCamara.IsValid())
		{
			tweenCamara.Kill();
		}

		tweenCamara = CreateTween().SetParallel(true).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);

		Vector3 posicionObjetivoPivote = objetivo.GlobalPosition;
		posicionObjetivoPivote.Y = 0;

		tweenCamara.TweenProperty(this, "global_position", posicionObjetivoPivote, duracionAnimacion*multiplicadorPivote);

		Vector3 posicionObjetivoCamara = new Vector3(0f, alturaZoomTurno, offsetZTurno);
		tweenCamara.TweenProperty(camera3D, "position", posicionObjetivoCamara, duracionAnimacion*multiplicadorCamara);

		tweenCamara.Chain().TweenCallback(Callable.From(() => 
		{
			enModoOrbita = true;
		}));
	}

	public void ResetearCamaraAutomaticamente(Node3D jugadorActual)
	{
	    controlJugador = false;
	    enModoOrbita = false;

	    EnfocarNodo(jugadorActual, 1.0f, 1.0f); // vuelve a la lógica automática
	}

}
