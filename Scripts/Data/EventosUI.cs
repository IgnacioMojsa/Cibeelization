using System;

public static class EventosUI
{
    public static event Action<string> OnMensajeAMostrar;

    public static void MostrarMensaje(string texto)
    {
        OnMensajeAMostrar?.Invoke(texto);
    }
}
