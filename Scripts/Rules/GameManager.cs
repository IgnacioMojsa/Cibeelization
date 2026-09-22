//using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using JuegoAbeja.Scripts.Data;

public partial class GameManager 
{
	public static GameManager Instance { get; } = new GameManager();
	public TurnManager TurnManager { get; private set; }
	public List<string> TiposDeAbejas = new List<string>();
	public List<AbejaReina> JugadoresEnPartida = new List<AbejaReina>();
	public AbejaReina jugadorEnTurno;
	public AbejaReina JugadorGanador {get; private set;}
	public Tablero TableroActual { get; set; }
	public CamaraController CamaraActual { get; set; }

	private GameManager(){}

	public int cantidadJugadores { get; set; }
	public int sizeTablero { get; set; } // Representa la opción elegida (2 = Small, 3 = Medium, 4 = Large)
	public int DimensionActual { get; private set; } = 15;

	public Partida PartidaActual { get; private set; }

	// Método para iniciar la partida con datos
	public void IniciarPartida(int cantidadJugadores, int sizeTablero)
	{
		PartidaActual = new Partida(cantidadJugadores, sizeTablero);

		this.cantidadJugadores = cantidadJugadores;
		this.sizeTablero = sizeTablero;

		// Generar jugadores
		CargarJugadores(cantidadJugadores);

		// Generar tablero
		//SetTiles(sizeTablero);
	}


	public event Action OnEstadoAccionesCambiado;
	
	
	public void NotificarCambioDeEstado()
	{
		OnEstadoAccionesCambiado?.Invoke();
	}	

	public bool CondicionVictoria(){
		var jugadoresFueraDeJuego = JugadoresEnPartida.Where(j => j.FueraDeJuego).ToList();

		return jugadoresFueraDeJuego.Count == cantidadJugadores - 1;
	}

	public void EstablecerJugadorGanador(){
		var jugadorGanador = GameManager.Instance.JugadoresEnPartida.Find(j => !j.FueraDeJuego);

		JugadorGanador = jugadorGanador;
	}

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
		if(jugadorEnTurno == null) return -1;
		if(!jugadorEnTurno.EsSuTurno) return -1;
		if(jugadorEnTurno.Estado != AbejaReina.EstadoTurno.EsperandoDado) return -1;
		if(jugadorEnTurno.TiroLosDados) return -1;

		//int numeroAleatorio = GD.RandRange(1, 6);
		Random r = new Random();
		int numeroAleatorio = r.Next(1, 7);

		jugadorEnTurno.TiroLosDados = true;

		jugadorEnTurno.MovimientosDisponibles = numeroAleatorio;
		jugadorEnTurno.Estado = AbejaReina.EstadoTurno.EsperandoAccion;
		
		return numeroAleatorio;
	}

	public void ConsumirMovimiento()
	{
		if (jugadorEnTurno == null) return;

		jugadorEnTurno.MovimientosDisponibles--;

		//GD.Print("Movimientos restantes: " + jugadorEnTurno.MovimientosDisponibles);

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

		jugadorEnTurno.ModoAtaque = false;
		jugadorEnTurno.AtacoRecien = true;
		TurnManager.TerminarTurno();
	}

	public void EliminarJugador(int Id)
	{
		JugadoresEnPartida[Id].FueraDeJuego = true;
		//GD.Print("El jugador " + JugadoresEnPartida[Id].Id + " ha sido eliminado");
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

	public List<Abeja> SubditosAdyacentes()
	{
		var celdasAdyacentes = TableroActual.ObtenerVecinos(jugadorEnTurno.UbicacionActual);
		
		return jugadorEnTurno.ColmenaDeReina.AbejasDeColmena.Where(a => celdasAdyacentes.Any(c => c == a.CeldaActual)).ToList();
	}

	public void PotenciarAtaqueDeJugadorEnTurno()
	{
		var subditosAdyacentes = SubditosAdyacentes();

		if(subditosAdyacentes.Count() > 2)
		{
			//GD.Print("Ataque de jugador " + jugadorEnTurno.Id + " potenciado"); 
			jugadorEnTurno.AtaquePotenciado = true;
		}
	}

	public void ResetearEstadoPartida()
	{
		//GD.Print("Reseteando la partida");
		JugadoresEnPartida.Clear();
		jugadorEnTurno = null;
		JugadorGanador = null;
		OnEstadoAccionesCambiado = null;

		if(TurnManager != null)
		{
			TurnManager.ReiniciarTurnos();
		}
		else
		{
			TurnManager = new TurnManager(JugadoresEnPartida);
			cantidadJugadores = 0;
			sizeTablero = 0;
		}
	}

	public void ReiniciarPartida()
	{
		if (PartidaActual == null)
		{
			//GD.PrintErr("No hay partida inicializada para reiniciar.");
			return;
		}
	
		JugadoresEnPartida.Clear();
		jugadorEnTurno = null;
		JugadorGanador = null;
	
		// Volver a cargar jugadores con los mismos parámetros
		CargarJugadores(PartidaActual.CantidadJugadores);
	
		// NO llamar a SetTiles acá → el nuevo Tablero lo hará en su _Ready()
		//GD.Print($"Partida reiniciada con {PartidaActual.CantidadJugadores} jugadores y tablero {PartidaActual.SizeTablero}");
	}



}
