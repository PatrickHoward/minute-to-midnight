using Godot;
using System;

public class PowerGems : Area2D
{
	private void _on_PowerGem_body_entered(Node body)
	{
			if (body.Name == "Player")
			{
				body.Call("CollectGem");
				
				QueueFree();
			}
	}
}
