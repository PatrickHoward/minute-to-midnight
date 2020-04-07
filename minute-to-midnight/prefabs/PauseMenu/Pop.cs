using Godot;
using System;

public class Pop : Popup
{

	private void _on_ToolButton_button_down()
	{
		Visible = false;
		GetNode<ColorRect>("CanvasLayer/ColorRect").Visible = false;
		GetTree().Paused = false;
	}

}

