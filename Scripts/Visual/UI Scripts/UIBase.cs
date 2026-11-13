using Godot;

public partial class UIBase : Control{
	protected AudioSetting UiSound1 => AudioManager.Instance?.GameAudio?.Sound1;
	protected AudioSetting UiSound4 => AudioManager.Instance?.GameAudio?.Sound4;

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
}
