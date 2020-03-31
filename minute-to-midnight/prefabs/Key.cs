using Godot;
using System;

public class Key : Area2D
{
    [Signal] public delegate void _Key_Collected();
    
    public void _on_Key_body_entered(Node body)
    {
        if (body.Name == "Player")
        {
            body.Call("CollectKey");
            EmitSignal(nameof(_Key_Collected));
            
            QueueFree();
        }
    }
}
