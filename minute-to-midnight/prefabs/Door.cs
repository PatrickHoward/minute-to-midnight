using Godot;
using System;
using System.Collections;

public class Door : StaticBody2D
{
    [Signal] public delegate void PlayerDoesNotHaveKey();
    [Signal] public delegate void PlayerDoesHaveKey();
    
    private AnimationPlayer _animationPlayer;

    public override void _Ready()
    {
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public void _on_DetectionArea_body_entered(Node body)
    {
        if (body.Name == "Player")
        {
            bool key = (bool) body.Get("HasKey");
            if (key)
            {
                _animationPlayer.Play("opendoor");
                EmitSignal(nameof(PlayerDoesHaveKey));
            }
            else
            {
                EmitSignal(nameof(PlayerDoesNotHaveKey));
            }
        }
    }

    public void _on_AnimationPlayer_animation_finished(string anim_name)
    {
        QueueFree();
    }
}
