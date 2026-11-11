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

		timerPartida.Start();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta){
		labelTimerPartida.Text = TiempoRestanteDeTimer(timerPartida.TimeLeft);
	}

	private string TiempoRestanteDeTimer(double tiempoRestante){
		int minutos = (int)(tiempoRestante / 60);
		int segundos = (int)(tiempoRestante % 60);
		return $"{minutos:D2}:{segundos:D2}";
	}
}
