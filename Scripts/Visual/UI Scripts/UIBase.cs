using System;
using Godot;
using Microsoft.CodeAnalysis.Options;

public partial class UIBase : Control{
	protected AudioSetting UiSound1 => AudioManager.Instance?.GameAudio?.Sound1; //Click
	protected AudioSetting UiSound4 => AudioManager.Instance?.GameAudio?.Sound4; //Dado
	protected AudioSetting UiSound7 => AudioManager.Instance?.GameAudio?.Sound7; //Victoria
	protected AudioSetting UiSound10 => AudioManager.Instance?.GameAudio?.Sound10; //Empate
	
	public virtual void MostrarUIPanel(Control panel, bool mostrar){
		if (panel == null) return;
		panel.Visible = mostrar;
		AudioManager.Instance.PlaySound(UiSound1);
	}

	public virtual void CambiarMenu(Control menuAOcultar, Control menuAMostrar){
		if (menuAOcultar != null) menuAOcultar.Visible = false;
		if (menuAMostrar != null) menuAMostrar.Visible = true;
		AudioManager.Instance.PlaySound(UiSound1);
	}

	public virtual void CambiarModoDePantalla(int indice){
		if (indice == 0)
		{
			GetTree().Root.Mode = Window.ModeEnum.Maximized;
			GetTree().Root.Borderless = false;
		}
		else if (indice == 1)
		{
			GetTree().Root.Mode = Window.ModeEnum.Windowed;
			GetTree().Root.Borderless = true;
		}
		else if (indice == 2)
		{
			GetTree().Root.Mode = Window.ModeEnum.ExclusiveFullscreen;
		}

		GameManager.Instance.modoPantallaSeleccionado = indice;
	}

	public virtual void CambiarResolucionDePantalla(int indice){
		
		if (indice == 0)
		{
			GetTree().Root.Size = new Vector2I(1280, 720);
		}
		else if (indice == 1)
		{
			GetTree().Root.Size = new Vector2I(1920, 1080);
		}
		else if (indice == 2)
		{
			GetTree().Root.Size = new Vector2I(2560, 1440);
		}

		GameManager.Instance.resolucionSeleccionada = indice;
	}

	public virtual void ActualizarConfiguracionesGraficas()
	{
		var opcionesModo = GetNodeOrNull<OptionButton>("Ajustes/VBoxContainer/MarginContainer/HBoxContainer/VBoxContainer2/Margin3/ModosDisponibles");
		var opcionesResolucion = GetNodeOrNull<OptionButton>("Ajustes/VBoxContainer/MarginContainer/HBoxContainer/VBoxContainer2/Margin5/ResolucionesDisponibles");
    	
		if (opcionesModo == null || opcionesResolucion == null) return;

		opcionesModo.Select(GameManager.Instance.modoPantallaSeleccionado);
		opcionesResolucion.Select(GameManager.Instance.resolucionSeleccionada);
	}
}
