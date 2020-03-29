using Godot;

public class Brazier : Node2D
{
    public void _on_Area2D_body_entered(Node body)
    {
        if (body.Name == "Player")
        {
            
        }
        
        GetChild<AnimatedSprite>(0).Animation = "unlit";

        var light = GetNode(new NodePath("Light"));
        light.QueueFree();
    }
}