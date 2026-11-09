using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class AnimationManager : Node3D
{
    public bool AnimacionFinalizada = false;
    private Dictionary<AbejaReina, AnimationPlayer> AnimacionesJugadores = new Dictionary<AbejaReina, AnimationPlayer>();

    public void ReproducirConCallback(Node3D unaAbeja, string animacion, System.Action alTerminar)
    {
        var abeja = unaAbeja.GetNode<AnimationPlayer>("AnimationPlayer");

        if (!abeja.HasAnimation(animacion))
        {
            alTerminar?.Invoke();
            return;
        }

        void Handler(StringName nombre)
        {
            if (nombre.ToString() != animacion) return;

            abeja.AnimationFinished -= Handler;
            alTerminar?.Invoke();
        }

        abeja.AnimationFinished += Handler;
        abeja.Play(animacion);
    }

    public void GuardarInstanciasDeJugadores(Dictionary<AbejaReina, Node3D> visualesJugadores)
    {
        for (int j = 0; j < visualesJugadores.Count(); j++)
        {
            var animacionJugador = visualesJugadores[GameManager.Instance.JugadoresEnPartida[j]].GetNode<AnimationPlayer>("AnimationPlayer");

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

    public void CambiarAnimacion(Node3D unaAbeja, string animacionNueva)
    {
        var animacionDeAbeja = unaAbeja.GetNode<AnimationPlayer>("AnimationPlayer");

        if(!animacionDeAbeja.HasAnimation(animacionNueva)) return;
        
        animacionDeAbeja.Play(animacionNueva);

        if(animacionDeAbeja.CurrentAnimation != "Idle" && !animacionDeAbeja.IsAnimationActive()) AnimacionFinalizada = true;
    }

    public void ReestablecerVelocidadAnimacion()
    {
        foreach (var jugador in GameManager.Instance.JugadoresEnPartida)
        {
            AnimacionesJugadores[jugador].SpeedScale = 1;
        }
    }
}
