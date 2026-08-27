using Godot;

public abstract class Celda
{
    // Identificador de la celda
    public int Id { get; protected set; }

    // Posición dentro del GridMap
    public Vector3I PosicionA { get; protected set; }
    public Vector3I PosicionB { get; protected set; }

    // Constructor
    protected Celda(int id, Vector3I posicionA, Vector3I posicionB)
    {
        Id = id;
        PosicionA = posicionA;
        PosicionB = posicionB;
    }
}