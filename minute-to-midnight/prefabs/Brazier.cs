using Godot;
using Godot.Collections;

public class Brazier : Node2D
{
    private bool used = false;

    [Export] public float time = 3.0f;

    [Signal] public delegate void _On_Collected(float time);

    public void _on_Area2D_body_entered(Node body)
    {
        if (body.Name == "Player" && !used)
        {
            float[] timeArg = { time };
            body.PropagateCall(nameof(Light.AddTimeToTimer), new Array(timeArg));

            EmitSignal(nameof(_On_Collected), time);

            used = true;

            var animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            animationPlayer.Play("burst");
        }
    }
}
