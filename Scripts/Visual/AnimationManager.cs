using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class AnimationManager : Node3D
{
    private Dictionary<AbejaReina, AnimationPlayer> AnimacionesJugadores = new Dictionary<AbejaReina, AnimationPlayer>();

    public void GuardarInstanciasDeJugadores(List<Node3D> visualesJugadores)
    {
        for (int j = 0; j < visualesJugadores.Count(); j++)
        {
            var animacionJugador = visualesJugadores[j].GetNode<AnimationPlayer>("AnimationPlayer");

            AnimacionesJugadores.Add(GameManager.Instance.JugadoresEnPartida[j], animacionJugador);
        }
    }

    public void CambiarAnimacionDeJugador(AbejaReina unJugador, string animacionNueva)
    {
        var animacionDeJugador = AnimacionesJugadores[unJugador];

        if(animacionNueva == "RecibirDanio")
        {
            animacionDeJugador.SpeedScale = 2;
        }
        
        animacionDeJugador.Play(animacionNueva);
    }

    public void ReestablecerVelocidadAnimacion()
    {
        foreach (var jugador in GameManager.Instance.JugadoresEnPartida)
        {
            AnimacionesJugadores[jugador].SpeedScale = 1;
        }
    }
}
