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

	public bool PuedeMover(AbejaReina jugadorEnTurno){
		return jugadorEnTurno != null &&
			   jugadorEnTurno.EsSuTurno &&
			   jugadorEnTurno.Estado == AbejaReina.EstadoTurno.EsperandoAccion;
			   //&&
			   //Esta otra linea limita los movimientos de la misma abeja
			   //jugadorEnTurno.MovimientosDisponibles > 0;
	} 

	public bool PuedeMoverseEntre(Celda origen, Celda destino, Dictionary<Node3D, Celda> celdasOcupadas,
    Node3D jugadorActual, List<Node3D> visualesJugadores)
	{
		if(origen == null || destino == null)
		return false;

		if(!CeldasSonAdyacentes(origen, destino))
		return false;

		if(CeldaTieneOtraReina(destino, jugadorActual, celdasOcupadas, visualesJugadores))
		return false;

		//List<Celda> vecinos = tablero.ObtenerVecinos(origen);
		//return vecinos.Contains(destino);

		return true;
	}

	public bool EsCeldaOrigenValida(Celda celdaOrigen)
	{
		return celdaOrigen != null;
	}

	public bool CeldasSonAdyacentes(Celda celdaOrigen, Celda celdaDestino)
	{
		if (celdaOrigen == null || celdaDestino == null)
			return false;

		List<Celda> vecinos = tablero.ObtenerVecinos(celdaOrigen);
		return vecinos.Contains(celdaDestino);
	}

	public bool CeldaTieneOtraReina(Celda celda, Node3D jugadorActual, Dictionary<Node3D, Celda> celdasOcupadas, List<Node3D> visualesJugadores)
	{
		foreach (var keyValuePair in celdasOcupadas)
		{
			if (keyValuePair.Key == jugadorActual)
            continue;

        	if (keyValuePair.Value == celda)
        	{
            	int indexJugador = visualesJugadores.IndexOf(keyValuePair.Key);

            	if (indexJugador != -1)
            	{
            	    AbejaReina jugadorOcupante = GameManager.Instance.JugadoresEnPartida[indexJugador];

            	    if (jugadorOcupante.FueraDeJuego)
            	    {
            	        continue; 
            	    }
            	}

            	return true;
        	}
		}

		return false;
	}
}