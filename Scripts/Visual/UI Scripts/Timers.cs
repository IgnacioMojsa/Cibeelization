using System.Collections.Generic;
using Godot;

public partial class Timers : Control{
	private Label labelTimerPartida; 
	private Label labelTimerTurno;
	private Timer timerPartida;
	private Timer timerTurno;
	private Dictionary<string, Timer> timersReconexion = new Dictionary<string, Timer>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		labelTimerPartida = GetNode<Label>("LabelTimerPartida");
		labelTimerTurno = GetNode<Label>("LabelTimerTurno");
		timerPartida = GetNode<Timer>("TimerPartida");
		timerTurno = GetNode<Timer>("TimerTurno");

		GameManager.Instance.DuracionTimerTurno = timerTurno.WaitTime;
		timerTurno.Timeout += OnTimerTurnoTimeout;
		timerPartida.Timeout += OnTimerPartidaTimeout;

		if(GameManager.Instance.TurnManager != null){
            GameManager.Instance.TurnManager.OnTurnoCambiado += ReiniciarTimerTurno;
        }
		
		Multiplayer.PeerDisconnected += OnPeerDisconnected;
		timerPartida.Start();
		timerTurno.Start();
	}

	public override void _Process(double delta){
		labelTimerPartida.Text = TiempoRestanteDeTimer(timerPartida.TimeLeft);
		labelTimerTurno.Text = TiempoRestanteDeTimer(timerTurno.TimeLeft);
	}

	private void OnTimerTurnoTimeout() => GameManager.Instance.ForzarFinDeTurnoPorTiempo();

	private void OnTimerPartidaTimeout(){
		timerPartida.Stop();
        timerTurno.Stop();
        GameManager.Instance.ResolverPartidaPorTiempo();

        if(GameManager.Instance.JugadorGanador != null){
            GetTree().ChangeSceneToFile("res://Scenes/pantallaVictoria.tscn");
        } else {
            GetTree().ChangeSceneToFile("res://Scenes/pantallaEmpate.tscn");
        }
	}
    
	private void ReiniciarTimerTurno(){
		timerTurno.Stop();
        timerTurno.Start();
    }

	private string TiempoRestanteDeTimer(double tiempoRestante){
		int minutos = (int)(tiempoRestante / 60);
		int segundos = (int)(tiempoRestante % 60);
		return $"{minutos:D2}:{segundos:D2}";
	}

	private void OnPeerDisconnected(long id){
        var jugador = GameManager.Instance.JugadoresEnPartida.Find(j => j.Id == id);
        if (jugador == null || jugador.FueraDeJuego) return;
        GameManager.Instance.IniciarDesconexionTemporal(id);
        IniciarTimerReconexion(jugador);
    }

	public void IniciarTimerReconexion(AbejaReina jugador){
        if (timersReconexion.ContainsKey(jugador.Name)) return;
        Timer timerReconexion = new Timer();
        timerReconexion.WaitTime = timerTurno.WaitTime * 2; //el doble de lo que dure un turno
        timerReconexion.OneShot = true;
        AddChild(timerReconexion);
        timerReconexion.Timeout += () => OnTimerReconexionTimeout(jugador);
        timersReconexion[jugador.Name] = timerReconexion;
        timerReconexion.Start();
        GD.Print($"Esperando reconexión, quedan {timerReconexion.WaitTime} segundos.");
    }

    public void CancelarTimerReconexion(string nombreJugador){
        if (timersReconexion.TryGetValue(nombreJugador, out Timer timer)){
            timer.Stop();
            timer.QueueFree();
            timersReconexion.Remove(nombreJugador);
            GD.Print("Timer de reconexión cancelado.");
        }
    }

    private void OnTimerReconexionTimeout(AbejaReina jugador){
        CancelarTimerReconexion(jugador.Name);
        if (jugador.DesconectadoTemporalmente){
            GD.Print("Se agotó el tiempo de reconexión y se eliminó al jugador.");
            GameManager.Instance.ConfirmarEliminacionPorDesconexion(jugador);

            if(GameManager.Instance.CondicionVictoria()){
                GameManager.Instance.EstablecerJugadorGanador();
                GetTree().ChangeSceneToFile("res://Scenes/pantallaVictoria.tscn");
            }
        }
    }

	public override void _ExitTree(){
		Multiplayer.PeerDisconnected -= OnPeerDisconnected;
        if (GameManager.Instance?.TurnManager != null){
            GameManager.Instance.TurnManager.OnTurnoCambiado -= ReiniciarTimerTurno;
        }
    }
}
