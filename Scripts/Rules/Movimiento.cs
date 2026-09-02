using Godot;
using System.Collections.Generic;

public class MovimientoManager
{
    private readonly Tablero tablero;
    public MovimientoManager(Tablero tablero)
    {
        this.tablero = tablero;
    }

    public Celda ObtenerCeldaDesdePosicion(List<Celda> Celdas, Vector3 posicion)
	{
		if (tablero == null || tablero.Celdas == null || tablero.Celdas.Count == 0)
			return null;

		Celda celdaMasCercana = null;
		float distanciaMinima = float.MaxValue;

		foreach (Celda celda in tablero.Celdas)
		{
			if (celda.Tile == null) continue;

			float dist = celda.Tile.GlobalPosition.DistanceTo(posicion);
			if (dist < distanciaMinima)
			{
				distanciaMinima = dist;
				celdaMasCercana = celda;
			}
		}

		return celdaMasCercana;
	}

	public bool PuedeMoverseEntre(Celda origen, Celda destino)
	{
		if(origen == null || destino == null)
		return false;

		List<Celda> vecinos = tablero.ObtenerVecinos(origen);
		return vecinos.Contains(destino);
	}

	public bool EsCeldaOrigenValida(Celda celdaOrigen)
	{
		return celdaOrigen != null;
	}

}