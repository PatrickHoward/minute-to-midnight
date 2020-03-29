using System;
using Godot;

public enum PlayerAnimationState
{
	Idle,
	Run,
	JumpStart,
	JumpLoop,
	FallLoop,
	Death,
	Attack1,
	Attack2,
	Attack3
}

public enum PlayerState
{
	Idle,
	Runnning,
	Jumping,
	Falling,
	Attacking,
}

public class PlayerController : KinematicBody2D
{
	[Export] public float JumpHeight = 250;
	[Export] public float Speed = 75;
	[Export] public float Gravity = 9.8f;
	
	private bool _attacking = false;
	
	private Vector2 _movement;
	private Vector2 _floor = new Vector2(0,-1);

	private PlayerState _state;
	
	private PlayerAnimationState _animationState;
	private AnimatedSprite _animations;
	
	public override void _Ready()
	{
		_movement = new Vector2();
		_animations = GetChild<AnimatedSprite>(0);
	}

	public override void _Process(float delta)
	{
		UpdatePlayerState();
		UpdateAnimation();
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(float delta)
	{
		//Left and Right Movement
		if(Input.IsActionPressed("Move-Left"))
		{
			if (IsOnFloor() && _state == PlayerState.Idle)
			{
				_state = PlayerState.Runnning;
			}
			
			_movement.x = -Speed;
		}
		else if(Input.IsActionPressed("Move-Right"))
		{
			if (IsOnFloor() && _state == PlayerState.Idle)
			{
				_state = PlayerState.Runnning;
			}
			
			_movement.x = Speed;
		}
		else
		{
			_movement.x = 0;
		}
		
		//Jump Action
		if(Input.IsActionPressed("Jump"))
		{
			if(IsOnFloor() && _animationState != PlayerAnimationState.JumpLoop)
			{
				_state = PlayerState.Jumping;
				_movement.y = -JumpHeight;
			}
		}
		
		if(Input.IsActionPressed("Action"))
		{
			_state = PlayerState.Attacking;
			//Action button 
		}

		//Constant weight of gravity pushes down on us all
		_movement.y += Gravity;
		
		_movement = MoveAndSlide(_movement, _floor);

		if (_movement == Vector2.Zero && _state != PlayerState.Attacking)
		{
			_state = PlayerState.Idle;
		}
		else if (_movement.y > 0)
		{
			_state = PlayerState.Falling;
		}
		
	}
	
	private void UpdatePlayerState()
	{
		switch (_state)
		{
			case PlayerState.Idle:
				_animationState = PlayerAnimationState.Idle;
				break;
			
			case PlayerState.Falling:
				_animationState = PlayerAnimationState.FallLoop;
				break;
			
			case PlayerState.Attacking:
				_animationState = PlayerAnimationState.Attack1;
				break;
			
			case PlayerState.Runnning:
				_animationState = PlayerAnimationState.Run;
				break;

			case PlayerState.Jumping:
				if (_animationState != PlayerAnimationState.JumpStart && _animationState != PlayerAnimationState.JumpLoop)
				{
					_animationState = PlayerAnimationState.JumpStart;
				}
				break;
		}
	}

	private void UpdateAnimation()
	{
		if (_movement.x < 0)
		{
			_animations.FlipH = true;
		}
		else if (_movement.x > 0)
		{
			_animations.FlipH = false;
		}

		if (_animationState == PlayerAnimationState.JumpLoop && IsOnFloor())
		{
			_animationState = PlayerAnimationState.Run;
		}

		switch (_animationState)
		{
			case PlayerAnimationState.Idle:
				_animations.Animation = "idle";
				break;
			
			case PlayerAnimationState.Run:
				_animations.Animation = "run";
				break;
			
			case PlayerAnimationState.Death:
				_animations.Animation = "death";
				break;
			
			case PlayerAnimationState.JumpStart:
				_animations.Animation = "jump_start";
				break;
			
			case PlayerAnimationState.JumpLoop:
				_animations.Animation = "jump_loop";
				break;
			
			case PlayerAnimationState.Attack1:
				_animations.Animation = "attack_1";
				break;
		}
	}

	public void _on_Sprite_animation_finished()
	{
		if (_movement == Vector2.Zero && IsOnFloor())
		{
			_state = PlayerState.Idle;
		}
		else if (IsOnFloor())
		{
			_state = PlayerState.Runnning;
		}

		if (_state == PlayerState.Jumping && _animationState == PlayerAnimationState.JumpStart)
		{
			_animationState = PlayerAnimationState.JumpLoop;
		}
	}
}
