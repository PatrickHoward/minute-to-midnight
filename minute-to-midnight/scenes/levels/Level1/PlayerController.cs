using Godot;
using System;

public class PlayerController : KinematicBody2D
{
	public float JumpHeight = 250;
	public float Speed = 75;
	public float Gravity = 9.8f;
	
	private bool OnGround = false;
	private Vector2 Movement;
	private Vector2 Floor = new Vector2(0,-1);
	
	public override void _Ready()
	{
		Movement = new Vector2();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(float delta)
	{
		//Left and Right Movement
		if(Input.IsActionPressed("Move-Left"))
		{
			Movement.x = -Speed;
		}
		else if(Input.IsActionPressed("Move-Right"))
		{
			Movement.x = Speed;
		}
		else
		{
			Movement.x = 0;
		}
		
		//Jump Action
		if(Input.IsActionPressed("Jump"))
		{
			if(OnGround)
			{
				Movement.y = -JumpHeight;
			}
		}
		
		if(Input.IsActionPressed("Action"))
		{
			//Action button 
		}
		
		//Check if on Floor
		if(IsOnFloor())
		{
			OnGround = true;
		}
		else
		{
			OnGround = false;
		}

		//Constant weight of gravity pushes down on us all
		Movement.y += Gravity;
		
		Movement = MoveAndSlide(Movement, Floor);
	}
}
