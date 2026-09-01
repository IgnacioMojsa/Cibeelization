using Godot;

[Tool]
public partial class Tablero : Node3D
{
	// 1.0 tile size + 0.05 border size
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


	// Esta es la escena que en GDScript tenías como:
	// const HEXAGON_TILE = preload(...)
	private const string HEXAGON_TILE_PATH = "res://Scenes/tiles.tscn";

	private PackedScene _hexagonTile;


	public override void _Ready()
	{
		// Cargar la escena del hexágono
		_hexagonTile = GD.Load<PackedScene>(HEXAGON_TILE_PATH);

		Refresh();
	}


	private void Refresh()
	{
		// Evita ejecutar esto demasiado pronto
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
				// Crear una copia de tiles.tscn
				Node3D tileNode = _hexagonTile.Instantiate<Node3D>();

				// Agregarla como hijo de Tablero
				AddChild(tileNode);

				// Posición X
				float xPos = widthRow * TileSize / 1.25f;

				// Posición Z
				float zPos = heightRow * TileSize;

				// Columnas impares
				bool isOdd = widthRow % 2 != 0;

				if (isOdd)
				{
					zPos += TileSize / 2f;
				}

				// Posicionar el hexágono
				tileNode.Position = new Vector3(
					xPos,
					0,
					zPos
				);
			}
		}
	}
}
