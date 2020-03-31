using Godot;
using System;

public class Hints : Control
{
    private AnimationPlayer _animationPlayer;

    private bool _hint1Played = false;
    private bool _hint2Played = false;
    private bool _monstHint1Played = false;
    
    public override void _Ready()
    {
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public void _on_HintTrigger1_body_entered(Node body)
    {
        //Play show lighthint1.
        if (!_hint1Played)
        {
            _animationPlayer.Play("ShowHint1");
            _hint1Played = true;
        }
        
    }
    
    public void _on_bazier12__On_Collected(float time)
    {
        //Play show lighthint2
        if (!_hint2Played)
        {
            _animationPlayer.Play("ShowHint2");
            _hint2Played = true;
        }
    }

    public void _on_Ghost2_GhostKilled()
    {
        //Play show MonsterHint
        if (!_monstHint1Played)
        {
            _animationPlayer.Play("ShowMonstHint1");
            _monstHint1Played = true;
        }
    }
}
