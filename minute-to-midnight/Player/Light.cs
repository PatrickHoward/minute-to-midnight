using System;
using Godot;

public class Light : KinematicBody2D
{
    [Export]
    public bool debug = false;

    [Export]
    public int MaxSpeed = 10;

    private int _speedMultiplier = 100;

    private Vector2 GetInput()
    {
        return new Vector2(
            Input.GetActionStrength("move_light_right") - Input.GetActionStrength("move_light_left"),
            Input.GetActionStrength("move_light_down") - Input.GetActionStrength("move_light_up")
        ).Normalized();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (debug)
        {
            var velocity = GetInput() * _speedMultiplier * MaxSpeed * delta;
            Position += velocity;
        }
    }
}
