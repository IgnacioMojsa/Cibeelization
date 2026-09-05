using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class Tablero : Node3D
{
	public List<Celda> Celdas { get; private set; } = new List<Celda>();

	// Diccionario para buscar rápidamente una celda por su posición (Q, R)
	private Dictionary<(int Q, int R), Celda> _celdasPorPosicion = new Dictionary<(int Q, int R), Celda>();

	// Las 6 direcciones vecinas en coordenadas axiales (Q, R)
	private static readonly Vector2I[] DireccionesHexagonales = new Vector2I[]
	{
		new Vector2I(+1,  0), new Vector2I(+1, -1), new Vector2I( 0, -1),
		new Vector2I(-1,  0), new Vector2I(-1, +1), new Vector2I( 0, +1)
	};

	[Export]
	public float TileSize
	{
		get => _tileSize;
		set
		{
			_tileSize = value;
			Refresh();
		}
	}
	private float _tileSize = 1.05f;

	[Export]
	public int WidthRows
	{
		get => _widthRows;
		set
		{
			_widthRows = value;
			Refresh();
		}
	}
	private int _widthRows = 4;

	[Export]
	public int HeightRows
	{
		get => _heightRows;
		set
		{
			_heightRows = value;
			Refresh();
		}
	}
	private int _heightRows = 4;

	private const string HEXAGON_TILE_PATH = "res://Scenes/tiles.tscn";
	private PackedScene _hexagonTile;

	public override void _Ready()
	{
		// Registrar esta instancia en GameManager
		GameManager.Instance.TableroActual = this;

		_hexagonTile = GD.Load<PackedScene>(HEXAGON_TILE_PATH);

		// Si hay una opción válida seleccionada en el menú, genera el tablero con ese tamaño
		if (GameManager.Instance.sizeTablero > 0)
		{
			GameManager.Instance.SetTiles(GameManager.Instance.sizeTablero);
		}
		else
		{
			Refresh();
		}
	}

	// Método público que asigna las filas y columnas directamente
	public void GenerarTablero(int filas, int columnas)
	{
		_widthRows = filas;
		_heightRows = columnas;

		Refresh();
	}

	private void Refresh()
	{
		if (!IsInsideTree())
			return;

		if (_hexagonTile == null)
		{
			_hexagonTile = GD.Load<PackedScene>(HEXAGON_TILE_PATH);
		}

		DeleteOld();
		SetTiles();
	}

	private void DeleteOld()
	{
		Celdas.Clear();
		_celdasPorPosicion.Clear();

		foreach (Node child in GetChildren())
		{
			child.QueueFree();
		}
	}


	private void SetTiles()
	{
		for (int widthRow = 0; widthRow < WidthRows; widthRow++)
		{
			for (int heightRow = 0; heightRow < HeightRows; heightRow++)
			{
				Node3D tileNode = _hexagonTile.Instantiate<Node3D>();
				AddChild(tileNode);

				float xPos = widthRow * TileSize / 1.25f;
				float zPos = heightRow * TileSize;

				bool isOdd = widthRow % 2 != 0;
				if (isOdd)
				{
					zPos += TileSize / 2f;
				}

				tileNode.Position = new Vector3(xPos, 0, zPos);

				// Convertir (widthRow, heightRow) a coordenadas Axiales (Q, R)
				// Para filas desfasadas en columnas impares (odd-q):
				int q = widthRow;
				int r = heightRow - (widthRow - (widthRow & 1)) / 2;

				Celda celda = new CeldaSinReforzar(
					Celdas.Count,
					q,
					r
				);

				celda.Tile = tileNode;

				Celdas.Add(celda);
				_celdasPorPosicion[(q, r)] = celda;
			}
		}
	}

	// --- MÉTODOS DE BÚSQUEDA Y MOVIMIENTO ---

	// Obtener celda por coordenadas axiales (Q, R)
	public Celda ObtenerCelda(int q, int r)
	{
		if (_celdasPorPosicion.TryGetValue((q, r), out Celda celda))
		{
			return celda;
		}
		return null;
	}

	// Obtener vecinos válidos de una celda
	public List<Celda> ObtenerVecinos(Celda celdaActual)
	{
		List<Celda> vecinos = new List<Celda>();

		foreach (Vector2I dir in DireccionesHexagonales)
		{
			Celda vecino = ObtenerCelda(celdaActual.Q + dir.X, celdaActual.R + dir.Y);
			if (vecino != null)
			{
				vecinos.Add(vecino);
			}
		}

		return vecinos;
	}

	// Devuelve todas las celdas alcanzables en exactamente 'pasos' movimientos (resultado del dado)
	public List<Celda> ObtenerCeldasAlcanzables(Celda celdaInicio, int pasos)
	{
		HashSet<Celda> visitadas = new HashSet<Celda>();
		List<Celda> resultado = new List<Celda>();
		
		// Tupla de (Celda, DistanciaActual)
		Queue<(Celda Celda, int Distancia)> cola = new Queue<(Celda, int)>();

		cola.Enqueue((celdaInicio, 0));
		visitadas.Add(celdaInicio);

		while (cola.Count > 0)
		{
			var (actual, dist) = cola.Dequeue();

			if (dist == pasos)
			{
				resultado.Add(actual);
				continue;
			}

			foreach (Celda vecino in ObtenerVecinos(actual))
			{
				if (!visitadas.Contains(vecino))
				{
					visitadas.Add(vecino);
					cola.Enqueue((vecino, dist + 1));
				}
			}
		}

		return resultado;
	}

	// Calcula la distancia mínima directa entre dos celdas
	public int CalcularDistancia(Celda a, Celda b)
	{
		int dq = a.Q - b.Q;
		int dr = a.R - b.R;
		int ds = (-a.Q - a.R) - (-b.Q - b.R);

		return (Math.Abs(dq) + Math.Abs(dr) + Math.Abs(ds)) / 2;
	}
}
