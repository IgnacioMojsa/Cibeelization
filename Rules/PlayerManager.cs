using Godot;
using System;

public partial class PlayerManager: Node3D{
    [Export] private Camera3D camera;
    private Vector3 posicionNueva; 
    private AbejaReina ReinaDelJugador = new AbejaReina(); 

    public override void _Process(double delta){
        if (Input.IsActionJustPressed("move")){
            
            if (camera == null)
            {
                camera = GetViewport().GetCamera3D();
            }

            ObtenerPosicionNueva();
            MoverAbeja();
        }
    }

    public void ObtenerPosicionNueva(){
        Vector2 mousePosition = GetViewport().GetMousePosition();
        Vector3 rayOrigin = camera.ProjectRayOrigin(mousePosition);
        Vector3 rayEnd = rayOrigin + camera.ProjectRayNormal(mousePosition) * 1000.0f;

        var spaceState = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);

        var result = spaceState.IntersectRay(query);

        if (result.Count > 0){
            posicionNueva = (Vector3)result["position"];
            GD.Print("Nueva posicion guardada" + posicionNueva);
        }
    }

    public void MoverAbeja(){
        Node3D VisualReina = GetTree().CurrentScene.GetNodeOrNull<Node3D>("AbejaReina"); 

        if (VisualReina != null)
        {
            VisualReina.GlobalPosition = posicionNueva;
        }
    }
}