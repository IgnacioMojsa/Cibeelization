using Godot;

public partial class Timers : Control{
	private Label labelTimerPartida; 
	private Label labelTimerTurno;
	private Timer timerPartida;
	private Timer timerTurno;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		labelTimerPartida = GetNode<Label>("LabelTimerPartida");
		labelTimerTurno = GetNode<Label>("LabelTimerTurno");
		timerPartida = GetNode<Timer>("TimerPartida");
		timerTurno = GetNode<Timer>("TimerTurno");

		timerTurno.Timeout += OnTimerTurnoTimeout;
		timerPartida.Timeout += OnTimerPartidaTimeout;

		if(GameManager.Instance.TurnManager != null){
            GameManager.Instance.TurnManager.OnTurnoCambiado += ReiniciarTimerTurno;
        }

		timerPartida.Start();
		timerTurno.Start();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
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

	public override void _ExitTree(){
        if (GameManager.Instance?.TurnManager != null){
            GameManager.Instance.TurnManager.OnTurnoCambiado -= ReiniciarTimerTurno;
        }
    }
}
