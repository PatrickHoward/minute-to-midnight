using Godot;
using Godot.Collections;

public class Pit : Area2D
{
    public void _on_Pit_body_entered(Node body)
    {
        if (body.Name == "Player")
        {
            float[] timeArg = { 60 };
            body.PropagateCall(nameof(Light.RemoveTimeFromTimer), new Array(timeArg));
        }
    }
}
