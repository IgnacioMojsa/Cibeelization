using System;
using Godot;

public partial class TutorialUI : UIBase{
	public enum ModoTutorial{
		PantallaInicial,
		InicioPartida,
		MenuPausa
	}
	[Export] public Texture2D[] Diapositivas { get; set; } = [];
	private TextureButton botonPrevio;
	private TextureButton botonSiguiente;
	private CheckBox checkNoVolverAMostrar;
	private Button botonOmitir;
	private TextureRect imagenActual;
	private int indiceActual = 0;
	private ModoTutorial modoActual = ModoTutorial.PantallaInicial;
	private const string RutaDeConfiguracion = "user://tutorial_config.cfg";
	private const string SeleccionDeConfiguracion = "Tutorial";
	private const string OpcionOcultar = "no_volver_a_mostrar";
	public event Action OnTutorialCerrado;

	public override void _Ready(){
		ProcessMode = ProcessModeEnum.Always;

		botonPrevio = GetNode<TextureButton>("Previo");
		botonSiguiente = GetNode<TextureButton>("Siguiente");
		checkNoVolverAMostrar = GetNode<CheckBox>("NoVolverAMostrar");
		botonOmitir = GetNode<Button>("Omitir");
		imagenActual = GetNode<TextureRect>("Imagenes");

		botonPrevio.Pressed += ImagenPrevia;
		botonSiguiente.Pressed += ImagenSiguiente;
		botonOmitir.Pressed += CerrarTutorial;

		DetectarContextoInicial();
	}

	private void DetectarContextoInicial(){
		string currentScene = GetTree().CurrentScene?.SceneFilePath ?? Owner?.SceneFilePath;
		if(currentScene == "res://Scenes/pantallaInicial.tscn"){
			CambiarModo(ModoTutorial.PantallaInicial);
			ReiniciarCarrusel();
		}
	}

	public void AbrirTutorial(ModoTutorial modo){
		CambiarModo(modo);
		ReiniciarCarrusel();
		Visible = true;
		//AudioManager.Instance?.PlaySound(UiSound1);
	}

	private void CambiarModo(ModoTutorial modo){
		modoActual = modo;
		switch(modo){
			case ModoTutorial.PantallaInicial:
				checkNoVolverAMostrar.Visible = false;
				botonOmitir.Visible = true;
				break;

			case ModoTutorial.InicioPartida:
				checkNoVolverAMostrar.Visible = true;
				checkNoVolverAMostrar.ButtonPressed = false;
				botonOmitir.Visible = true;
				break;

			case ModoTutorial.MenuPausa:
				checkNoVolverAMostrar.Visible = false;
				botonOmitir.Visible = true;
				break;
		}
	}

	public void ReiniciarCarrusel(){
		indiceActual = 0;
		ActualizarImagenMostrada();
	}

	private void ImagenPrevia(){
		if(Diapositivas == null || Diapositivas.Length == 0) return;
		if(indiceActual > 0){
			indiceActual--;
			ActualizarImagenMostrada();
			AudioManager.Instance?.PlaySound(UiSound1);
		}
	}

	private void ImagenSiguiente(){
		if(Diapositivas == null || Diapositivas.Length == 0) return;
		if(indiceActual < Diapositivas.Length - 1)	{
			indiceActual++;
			ActualizarImagenMostrada();
			AudioManager.Instance?.PlaySound(UiSound1);
		}
	}

	private void ActualizarImagenMostrada(){
		if(Diapositivas == null || Diapositivas.Length == 0){
			imagenActual.Texture = null;
			botonPrevio.Disabled = true;
			botonSiguiente.Disabled = true;
			return;
		}
		imagenActual.Texture = Diapositivas[indiceActual];
		botonPrevio.Disabled = indiceActual == 0;
		botonSiguiente.Disabled = indiceActual >= Diapositivas.Length - 1;
	}

	private void CerrarTutorial(){
		if(checkNoVolverAMostrar.Visible && checkNoVolverAMostrar.ButtonPressed){
			GuardarPreferenciaDeConfiguracion(true);
		}
		Visible = false;
		AudioManager.Instance?.PlaySound(UiSound1);
		OnTutorialCerrado?.Invoke();
	}

	public void GuardarPreferenciaDeConfiguracion(bool noMostrar){
		var config = new ConfigFile();
		config.Load(RutaDeConfiguracion);
		config.SetValue(SeleccionDeConfiguracion, OpcionOcultar, noMostrar);
		config.Save(RutaDeConfiguracion);
	}

	public bool SeMuestraAlComenzarPartida(){
		var config = new ConfigFile();
		if(config.Load(RutaDeConfiguracion) != Error.Ok) return true;
		bool noVolverAMostrar = (bool)config.GetValue(SeleccionDeConfiguracion, OpcionOcultar, false);
		return !noVolverAMostrar;
	}
}
