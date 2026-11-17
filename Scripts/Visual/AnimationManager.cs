using Godot;
using System;
using System.Collections.Generic;

public partial class AnimationManager : Node3D
{
    public bool AnimacionFinalizada = false;
    private readonly Dictionary<AbejaReina, AnimationPlayer> AnimacionesJugadores = new();
    private readonly Dictionary<AnimationPlayer, AnimationPlayer.AnimationFinishedEventHandler> CallbacksActivos = new();

    public void ReproducirConCallback(Node3D unaAbeja, string animacion, Action alTerminar){
        if(unaAbeja == null || !IsInstanceValid(unaAbeja)){
            alTerminar?.Invoke();
            return;
        }

        var abeja = unaAbeja.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        if(abeja == null || !abeja.HasAnimation(animacion)){
            alTerminar?.Invoke();
            return;
        }

        EstablecerAIdle(abeja, animacion, alTerminar);

        abeja.Stop();
        abeja.Play(animacion);
    }

    public void GuardarInstanciasDeJugadores(Dictionary<AbejaReina, Node3D> visualesJugadores){
        AnimacionesJugadores.Clear();
        CallbacksActivos.Clear();

        if(visualesJugadores == null) return;

        foreach(var kvp in visualesJugadores){
            if(kvp.Key == null || kvp.Value == null || !IsInstanceValid(kvp.Value)) continue;
            var animacionJugador = kvp.Value.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
            if(animacionJugador != null){
                AnimacionesJugadores[kvp.Key] = animacionJugador;
            }
        }
    }

    public void CambiarAnimacionDeJugador(AbejaReina unJugador, string animacionNueva){
        if(unJugador == null || !AnimacionesJugadores.TryGetValue(unJugador, out var animacionDeJugador)) return;
        if(animacionDeJugador == null || !IsInstanceValid(animacionDeJugador)) return;
        if(!animacionDeJugador.HasAnimation(animacionNueva)) return;

        animacionDeJugador.SpeedScale = (animacionNueva == "RecibirDanio") ? 2.0f : 1.0f;
        EstablecerAIdle(animacionDeJugador, animacionNueva);

        animacionDeJugador.Stop();
        animacionDeJugador.Play(animacionNueva);
    }

    public void CambiarAnimacion(Node3D unaAbeja, string animacionNueva) {
        if(unaAbeja == null || !IsInstanceValid(unaAbeja)) return;
        var animacionDeAbeja = unaAbeja.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        if(animacionDeAbeja == null || !animacionDeAbeja.HasAnimation(animacionNueva)) return;

        if(animacionNueva != "Idle"){
            AnimacionFinalizada = false;
            EstablecerAIdle(animacionDeAbeja, animacionNueva, () =>
            {
                AnimacionFinalizada = true;
            });
        }

        animacionDeAbeja.Stop();
        animacionDeAbeja.Play(animacionNueva);
    }

    private void EstablecerAIdle(AnimationPlayer animationPlayer, string animacionActual, Action accionExtra = null){
        if(CallbacksActivos.TryGetValue(animationPlayer, out var handlerAnterior)){
            animationPlayer.AnimationFinished -= handlerAnterior;
            CallbacksActivos.Remove(animationPlayer);
        }

        void Handler(StringName nombre){
            if(nombre.ToString() != animacionActual) return;

            if(IsInstanceValid(animationPlayer)){
                animationPlayer.AnimationFinished -= Handler;
                CallbacksActivos.Remove(animationPlayer);

                if(animacionActual == "RecibirDanio"){
                    animationPlayer.SpeedScale = 1.0f;
                }

                if((animacionActual == "Atacar" || animacionActual == "RecibirDanio") && animationPlayer.HasAnimation("Idle")){
                    animationPlayer.Play("Idle");
                }
            }
            accionExtra?.Invoke();
        }
        CallbacksActivos[animationPlayer] = Handler;
        animationPlayer.AnimationFinished += Handler;
    }

    public void ReestablecerVelocidadAnimacion()
    {
        foreach (var animationPlayer in AnimacionesJugadores.Values)
        {
            if (animationPlayer != null && IsInstanceValid(animationPlayer))
            {
                animationPlayer.SpeedScale = 1.0f;
            }
        }
    }
}