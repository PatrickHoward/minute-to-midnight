using Godot;

public class Brazier : Node2D
{
    private bool used = false;
    
    public void _on_Area2D_body_entered(Node body)
    {
        if (body.Name == "Player" && !used)
        {
            //SEND TIME EXTEND SIGNAL HERE
            var light = GetNode(new NodePath("Light"));
            light.QueueFree();
        }
    }
}