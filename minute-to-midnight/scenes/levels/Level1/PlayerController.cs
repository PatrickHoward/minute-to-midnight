using Godot;

public class PlayerController : KinematicBody2D
{
	[Export] public float JumpHeight = 250;
	[Export] public float Speed = 75;
	[Export] public float Gravity = 9.8f;
	
	private bool _onground = false;
	private Vector2 _movement;
	private Vector2 _floor = new Vector2(0,-1);
	
	public override void _Ready()
	{
		_movement = new Vector2();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(float delta)
	{
		//Left and Right Movement
		if(Input.IsActionPressed("Move-Left"))
		{
			_movement.x = -Speed;
		}
		else if(Input.IsActionPressed("Move-Right"))
		{
			_movement.x = Speed;
		}
		else
		{
			_movement.x = 0;
		}
		
		//Jump Action
		if(Input.IsActionPressed("Jump"))
		{
			if(_onground)
			{
				_movement.y = -JumpHeight;
			}
		}
		
		if(Input.IsActionPressed("Action"))
		{
			//Action button 
		}
		
		//Check if on Floor
		if(IsOnFloor())
		{
			_onground = true;
		}
		else
		{
			_onground = false;
		}

		//Constant weight of gravity pushes down on us all
		_movement.y += Gravity;
		
		_movement = MoveAndSlide(_movement, _floor);
	}

	private void UpdateAnimation()
	{
		
	}
}
