using System;
using Godot;
using Microsoft.CodeAnalysis.Options;
using System.Collections.Generic;
using System.Linq;
using System.Net;

public partial class UIBase : Control{
	protected AudioSetting UiSound1 => AudioManager.Instance?.GameAudio?.Sound1; //Click
	protected AudioSetting UiSound4 => AudioManager.Instance?.GameAudio?.Sound4; //Dado
	protected AudioSetting UiSound7 => AudioManager.Instance?.GameAudio?.Sound7; //Victoria
	protected AudioSetting UiSound10 => AudioManager.Instance?.GameAudio?.Sound10; //Empate
	private static readonly List<Vector2I> Resoluciones = new List<Vector2I>(){
		new Vector2I(1280, 720),
		new Vector2I(1920, 1080),
		new Vector2I(2560, 1440)
	};
	
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
		GameManager.Instance.modoPantallaSeleccionado = indice;
		AplicarConfiguracionDePantalla();
	}

	public virtual void CambiarResolucionDePantalla(int indice){
		GameManager.Instance.resolucionSeleccionada = indice;
		AplicarConfiguracionDePantalla();
	}

	private async void AplicarConfiguracionDePantalla(){
		var ventana = GetTree().Root;
		int modo = GameManager.Instance.modoPantallaSeleccionado;
		int indiceRes = GameManager.Instance.resolucionSeleccionada;
		
		if (modo == 2){
			ventana.Mode = Window.ModeEnum.ExclusiveFullscreen;
		} else {
			ventana.Mode = Window.ModeEnum.Windowed;
			ventana.Borderless = (modo == 1);
		
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		
			// 3) Después el tamaño, sin pasarse del monitor
			int pantalla = DisplayServer.WindowGetCurrentScreen();
			Vector2I util = DisplayServer.ScreenGetUsableRect(pantalla).Size;
			Vector2I tamano = Resoluciones[indiceRes];
			if (tamano.X > util.X || tamano.Y > util.Y) tamano = util;
		
			ventana.Size = tamano;
		
			// 4) Centrar la ventana en el monitor
			Vector2I origen = DisplayServer.ScreenGetUsableRect(pantalla).Position;
			ventana.Position = origen + (util - tamano) / 2;
		}
		
		ActualizarEstadoOpcionesGraficas();
	}

	private void ActualizarEstadoOpcionesGraficas(){
		var opcionesResolucion = GetNodeOrNull<OptionButton>("Ajustes/VBoxContainer/MarginContainer/HBoxContainer/VBoxContainer2/Margin5/ResolucionesDisponibles");
		if (opcionesResolucion != null){
			opcionesResolucion.Disabled = GameManager.Instance.modoPantallaSeleccionado == 2;
		}
	}

	public virtual void ActualizarConfiguracionesGraficas()
	{
		var opcionesModo = GetNodeOrNull<OptionButton>("Ajustes/VBoxContainer/MarginContainer/HBoxContainer/VBoxContainer2/Margin3/ModosDisponibles");
		var opcionesResolucion = GetNodeOrNull<OptionButton>("Ajustes/VBoxContainer/MarginContainer/HBoxContainer/VBoxContainer2/Margin5/ResolucionesDisponibles");
    	
		if (opcionesModo == null || opcionesResolucion == null) return;

		opcionesModo.Select(GameManager.Instance.modoPantallaSeleccionado);
		opcionesResolucion.Select(GameManager.Instance.resolucionSeleccionada);
		ActualizarEstadoOpcionesGraficas(); 
	}
}
