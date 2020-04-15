using Godot;
using System;

public class Pop : Popup
{
	public override void _Input(InputEvent e)
	{
		if(e.IsActionPressed("Okay"))
		{
			Visible = false;
			GetNode<ColorRect>("CanvasLayer/ColorRect").Visible = false;
			GetTree().Paused = false;
		}
	}
	
	private void _on_ToolButton_button_down()
	{
		Visible = false;
		GetNode<ColorRect>("CanvasLayer/ColorRect").Visible = false;
		GetTree().Paused = false;
	}

}

