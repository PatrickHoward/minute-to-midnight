using Godot;

public class Burst : Node2D
{
    private Godot.Collections.Array _particles;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _particles = GetChildren();
    }

    public void Animate()
    {
        foreach (Particles2D particle in _particles)
        {
            particle.Emitting = true;
        }
    }
}
