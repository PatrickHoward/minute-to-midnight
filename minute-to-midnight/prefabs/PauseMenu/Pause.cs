using Godot;
using System;

public class Pause : Control
{
	private bool _pauseState;
	private Sprite _menu;
	
	public override void _Ready()
	{
		Visible = false;
		_menu = GetNode<Sprite>("Menu/MenuBackground");
		_menu.Visible = false;
	}
	
	public override void _Input(InputEvent e)
	{
		if(e.IsActionPressed("Pause"))
		{
			_pauseState = !GetTree().Paused;
			GD.Print("Pause State: " + _pauseState);
			GetTree().Paused = _pauseState;
			Visible = _pauseState;
			_menu.Visible = _pauseState;
		}
	}
}
