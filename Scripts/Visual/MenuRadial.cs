using Godot;
using System;

public partial class MenuRadial : Control
{
    [Export] private GameUI hud; 
    private Control ContenedorDeBotones;
    public bool Activo = false;
    public Vector2 PosicionCentral;
    public float Radio = 110f;
    public float VelocidadAnimacion = 0.25f;

    public override void _Ready()
    {
        ContenedorDeBotones = GetNode<Control>("%ContenedorBotones");

        PosicionCentral = GetViewportRect().Size / 2 - ContenedorDeBotones. GetRect().Size / 2;
        ContenedorDeBotones.Hide();

        foreach (Node nodoHijo in ContenedorDeBotones.GetChildren()) 
        {
            if(nodoHijo is Button boton)
            {
                boton.PivotOffset = boton.Size / 2;
                boton.Position = Vector2.Zero - (boton.Size / 2);
                boton.Scale = Vector2.Zero;

                boton.Pressed += () => SeleccionarOpcion(boton.Name);
            }
        }
    }

    public void EstablecerHUD(GameUI gameUI)
    {
        hud = gameUI;
    }

    public void DesplegarEnPosicion(Vector2 posicionGlobal)
    {
        GlobalPosition = posicionGlobal;

        if (Activo)
        {
            OcultarMenu();
        }
        else
        {
            MostrarMenu();
        }
    }

    private void OnPressed()
    {
        if(Activo)
        {
            OcultarMenu();
        }
        else
        {
            MostrarMenu();
        }
    }

    public void MostrarMenu()
    {
        Activo = true;
        ContenedorDeBotones.Show();
        Show();
        MoveToFront();
        
        int cantidadDeBotones = ContenedorDeBotones.GetChildCount();
        if (cantidadDeBotones == 0) return;

        float espaciado = MathF.Tau / cantidadDeBotones;

        for (int b = 0; b < cantidadDeBotones; b++)
        {
            if(ContenedorDeBotones.GetChild(b) is Button boton)
            {
                float angulo = b * espaciado;
                Vector2 posicionObjetivo = (new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * Radio) - (boton.Size / 2);

                Tween tween = CreateTween().SetParallel(true);
                tween.TweenProperty(boton, "position", posicionObjetivo, VelocidadAnimacion)
                    .SetTrans(Tween.TransitionType.Back)
                    .SetEase(Tween.EaseType.Out);

                tween.TweenProperty(boton, "scale", Vector2.One, VelocidadAnimacion)
                    .SetTrans(Tween.TransitionType.Back)
                    .SetEase(Tween.EaseType.Out);
            }
        }
    }

    public void OcultarMenu()
    {
        int cantidadBotones = ContenedorDeBotones.GetChildCount();

        for (int i = 0; i < cantidadBotones; i++)
        {
            if (ContenedorDeBotones.GetChild(i) is Button boton)
            {
                Vector2 posicionCentroLocal = Vector2.Zero - (boton.Size / 2);
                
                Tween tween = CreateTween().SetParallel(true);
                tween.TweenProperty(boton, "position", posicionCentroLocal, VelocidadAnimacion)
                    .SetTrans(Tween.TransitionType.Linear)
                    .SetEase(Tween.EaseType.In);

                tween.TweenProperty(boton, "scale", Vector2.Zero, VelocidadAnimacion)
                    .SetTrans(Tween.TransitionType.Linear)
                    .SetEase(Tween.EaseType.In);
            }
        }

        GetTree().CreateTimer(VelocidadAnimacion).Timeout += () =>
        {
            ContenedorDeBotones.Hide();
            Activo = false;
        };
    }

    private void SeleccionarOpcion(string nombreBoton)
    {
        switch(nombreBoton)
        {
            case "ControlarButton":
                break;
            case "CurarButton":
                hud.OnCurarPressed();
                break;
            case "InvocarButton":
                hud.InvocarSubdito();
                break;
        }
        
        OcultarMenu();
    }

    public void ActualizarEstadoBoton(bool condicion, string unBoton)
    {
        if (ContenedorDeBotones.HasNode(unBoton))
        {
            var boton = ContenedorDeBotones.GetNode<Button>(unBoton);
            boton.Disabled = !condicion;
        }
    }
}

