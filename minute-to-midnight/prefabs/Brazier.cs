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
            var flame = GetNode<Node2D>(new NodePath("Flame"));
            flame.QueueFree();

            float[] timeArg = {time};
            body.PropagateCall(nameof(Light.AddTimeToTimer), new Array(timeArg));

            EmitSignal(nameof(_On_Collected), time);

            used = true;

            var burst = GetNode<Node2D>("Burst").GetChildren();
            foreach (Particles2D particle in burst)
            {
                particle.Emitting = true;
            }
        }
    }
}
