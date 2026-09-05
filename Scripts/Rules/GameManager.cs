using Godot;
using System.Collections.Generic;

public partial class GameManager 
{
	public static GameManager Instance { get; } = new GameManager();

	public TurnManager TurnManager { get; private set; }
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
	

	public void SetTiles(int opcionTamaño)
	{
		int dimension = 15; // Valor por defecto (Small)
	
		switch (opcionTamaño)
		{
			case 2:
				dimension = 15; // Small: 15x15
				break;
			case 3:
				dimension = 25; // Mid: 25x25
				break;
			case 4:
				dimension = 30; // Big: 30x30
				break;
		}
	
		if (TableroActual != null)
		{
			TableroActual.GenerarTablero(dimension, dimension);
	
			if (CamaraActual != null)
			{
				CamaraActual.AjustarATablero(dimension, dimension, TableroActual.TileSize);
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
		JugadoresEnPartida.RemoveAt(Id);
		GD.Print("El jugador " + JugadoresEnPartida[Id].Id + " ha sido eliminado");
	}

	public void TransformarAbejaObrera(Abeja unaAbeja, Abeja otraAbeja, Colmena unaColmena)
	{
		if(unaAbeja.AptaParaTransformar(otraAbeja, unaColmena))
		{
			// Transformación
		}
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
