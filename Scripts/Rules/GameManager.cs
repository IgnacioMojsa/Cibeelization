using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class GameManager 
{
	public static GameManager Instance { get; } = new GameManager();

	public TurnManager TurnManager { get; private set; }
	public List<string> TiposDeAbejas = new List<string>();
	public List<AbejaReina> JugadoresEnPartida = new List<AbejaReina>();
	public AbejaReina jugadorEnTurno;

	// Referencia al tablero activo en la escena
	public Tablero TableroActual { get; set; }

	private GameManager(){}

	public int cantidadJugadores { get; set; }
	public int sizeTablero { get; set; } // Representa la opción elegida (2 = Small, 3 = Medium, 4 = Large)

	// Referencia al controlador de la cámara
	public CamaraController CamaraActual { get; set; }

	// Convierte la opción de UI en dimensiones de celdas (WidthRows x HeightRows)
	
	public bool CondicionVictoria(){
		var jugadoresFueraDeJuego = JugadoresEnPartida.Where(j => j.FueraDeJuego).ToList();
		var jugadorGanador = JugadoresEnPartida.Find(j => !j.FueraDeJuego);

		return jugadoresFueraDeJuego.Count == cantidadJugadores - 1;
	}  

	public int DimensionActual { get; private set; } = 15;

	public void SetTiles(int opcionTamaño)
	{
	    switch (opcionTamaño)
	    {
	        case 2: DimensionActual = 15; break; // Small
	        case 3: DimensionActual = 25; break; // Mid
	        case 4: DimensionActual = 30; break; // Big
	        default: DimensionActual = 15; break;
	    }
	
	    // Intentamos actualizar si los nodos ya están presentes
	    ActualizarTableroYCamara();
	}
	
	public void ActualizarTableroYCamara()
	{
	    if (TableroActual != null)
	    {
	        TableroActual.GenerarTablero(DimensionActual, DimensionActual);
	
	        if (CamaraActual != null)
	        {
	            CamaraActual.AjustarATablero(DimensionActual, DimensionActual, TableroActual.TileSize);
	        }
	    }
	}

	public int TirarDado()
	{
		if(jugadorEnTurno == null) return 1;
		if(!jugadorEnTurno.EsSuTurno) return 1;
		if(jugadorEnTurno.Estado != AbejaReina.EstadoTurno.EsperandoDado) return 1;
		if(jugadorEnTurno.TiroLosDados) return 1;

		int numeroAleatorio = GD.RandRange(1, 6);
		jugadorEnTurno.TiroLosDados = true;

		jugadorEnTurno.MovimientosDisponibles = numeroAleatorio;
		jugadorEnTurno.Estado = AbejaReina.EstadoTurno.EsperandoAccion;

		return numeroAleatorio;
	}

	public void ConsumirMovimiento()
	{
		if (jugadorEnTurno == null) return;

		jugadorEnTurno.MovimientosDisponibles--;

		GD.Print("Movimientos restantes: " + jugadorEnTurno.MovimientosDisponibles);

		if (jugadorEnTurno.MovimientosDisponibles <= 0)
		{
			TurnManager.TerminarTurno();
		}
	}

	public bool PuedeAtacar()
	{
		return jugadorEnTurno != null &&
			   jugadorEnTurno.EsSuTurno &&
			   jugadorEnTurno.Estado == AbejaReina.EstadoTurno.EsperandoAccion;
	}

	public void ConsumirAtaque()
	{
		if (jugadorEnTurno == null) return;

		jugadorEnTurno.AtacoRecien = true;
		TurnManager.TerminarTurno();
	}

	public void EliminarJugador(int Id)
	{
		JugadoresEnPartida[Id].FueraDeJuego = true;
		GD.Print("El jugador " + JugadoresEnPartida[Id].Id + " ha sido eliminado");
	}

	public void GenerarAbejaNueva(Abeja unaAbeja){
		var abejaNueva = unaAbeja;

		jugadorEnTurno.ColmenaDeReina.AbejasDeColmena.Add(abejaNueva);
		jugadorEnTurno.InvocoRecien = true;
		
		TurnManager.TerminarTurno();
	}

	public void TransformarAbejaObrera(Abeja unaAbeja, Abeja otraAbeja, Colmena unaColmena)
	{
		if(unaAbeja.AptaParaTransformar(otraAbeja, unaColmena))
		{
			// Transformación
		}
	}

	public void CargarTipoDeAbejas(){
		TiposDeAbejas.Add("Abeja");
	}

	public void CargarJugadores(int cantidadDeJugadores)
	{
		JugadoresEnPartida.Clear();
		for (int i = 1; i <= cantidadDeJugadores; i++)
		{
			var NuevoJugador = new AbejaReina(i);
			JugadoresEnPartida.Add(NuevoJugador);
		}	

		TurnManager = new TurnManager(JugadoresEnPartida);
		TurnManager.EstablecerPrimerTurno();
		jugadorEnTurno = TurnManager.jugadorEnTurno;
	}
}
