using Godot;
using System;

public class MainMenu : Control
{
    private AnimationPlayer _animationPlayer;
    
    public override void _Ready()
    {
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public void Quit()
    {
        GetTree().Quit();
    }

    public void StartGame()
    {
        GetTree().ChangeScene("res://scenes/levels/Level1/Level1.tscn");
    }
    
    public void _on_Start_Game_Trigger_body_entered(Node body)
    {
        if (body.Name == "Player")
        {
            _animationPlayer.Play("Fade_Into_Game");
        }
    }
    
    public void _on_Exit_Game_Trigger_body_entered(Node body)
    {
        if (body.Name == "Player")
        {
            _animationPlayer.Play("Fade_Exit");
        }
    }
}
