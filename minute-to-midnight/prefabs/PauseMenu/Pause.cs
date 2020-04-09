using Godot;
using System;

public class Pause : Control
{
	private bool _pauseState;
	private Sprite _menu;
	private Sprite _opMenu;
	
	public override void _Ready()
	{
		Visible = false;
		_menu = GetNode<Sprite>("Menu/MenuBackground");
		_menu.Visible = false;
		
		_opMenu = GetNode<Sprite>("OptionMenu/OptionBackground");
		_opMenu.Visible = false;
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

	private void _on_Save_button_down()
	{

	}
	
	
	private void _on_Load_button_down()
	{

	}
	
	
	private void _on_Options_button_down()
	{
		_opMenu.Visible = true;
	}
	
	private void _on_Cancel_button_down()
	{
		_opMenu.Visible = false;
	}
	
	private void _on_Apply_button_down()
	{
		_opMenu.Visible = false;
	}
	
	private void _on_Quit_button_down()
	{
		GetTree().Quit();
	}
}
