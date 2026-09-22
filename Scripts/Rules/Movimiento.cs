
using System.Collections.Generic;
using System.Linq;

public class MovimientoManager
{
    private readonly Tablero tablero;

    public MovimientoManager(Tablero tablero)
    {
        this.tablero = tablero;
    }

	public bool PuedeMover(AbejaReina jugadorEnTurno){
		return jugadorEnTurno != null &&
			   jugadorEnTurno.EsSuTurno &&
			   jugadorEnTurno.Estado == AbejaReina.EstadoTurno.EsperandoAccion;
			   //&&
			   //Esta otra linea limita los movimientos de la misma abeja
			   //jugadorEnTurno.MovimientosDisponibles > 0;
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

	public bool CeldaTieneOtraAbeja(Celda celdaDestino)
	{
		bool hayAbejaEnCelda = GameManager.Instance.JugadoresEnPartida.Any(
				reina => reina.ColmenaDeReina.AbejasDeColmena.Any(
					abeja => abeja.CeldaActual == celdaDestino && !abeja.FueraDeJuego
				)
			);
		
		if(hayAbejaEnCelda){
			//GD.Print("Esta celda esta ocupada por otra abeja");
			return true;
		}
		else{
			return false;
		}
	}
}