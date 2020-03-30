using System;
using Godot;

public enum EnemyAnimationState
{
	Idle,
	Walk,
	Death,
	Attack1,
}

public enum EnemyState
{
	Idle,
	Walking,
	Attacking,
}

public class EnemyBehavior00 : KinematicBody2D
{
	[Export] public float Speed = 50;
	[Export] public float Gravity = 9.8f;
	
	private int _changedirection = -1;
	
	private bool _attacking = false;
	private bool _hitwall = false;
	
	private Vector2 _movement;
	private Vector2 _floor = new Vector2(0,-1);

	private EnemyState _state;
	
	private EnemyAnimationState _animationState;
	private AnimatedSprite _animations;
	private RayCast2D _rc;
	
	public override void _Ready()
	{
		_movement = new Vector2();
		_animations = GetChild<Node>(1).GetChild<AnimatedSprite>(0);
		_rc = GetChild<RayCast2D>(2);
	}

	public override void _Process(float delta)
	{
		UpdatePlayerState();
		UpdateAnimation();
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(float delta)
	{
		

		if (_state == EnemyState.Idle)
		{
			_state = EnemyState.Walking;
		}
		
		if(IsOnWall() || !_rc.IsColliding())
		{
			Speed *= _changedirection;
			_rc.Position *= new Vector2(-1,1);
		}
		
		_movement.x = Speed;
		
		_movement.y = Gravity;
		
		_movement = MoveAndSlide(_movement, _floor);

		if (_movement == Vector2.Zero && _state != EnemyState.Attacking)
		{
			_state = EnemyState.Idle;
		}		
	}
	
	private void UpdatePlayerState()
	{
		switch (_state)
		{
			case EnemyState.Idle:
				_animationState = EnemyAnimationState.Idle;
				break;
			
			case EnemyState.Attacking:
				_animationState = EnemyAnimationState.Attack1;
				break;
			
			case EnemyState.Walking:
				_animationState = EnemyAnimationState.Walk;
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

		switch (_animationState)
		{
			case EnemyAnimationState.Idle:
				_animations.Animation = "idle";
				break;
			
			case EnemyAnimationState.Death:
				_animations.Animation = "death";
				break;
			
			case EnemyAnimationState.Attack1:
				_animations.Animation = "attack_1";
				break;
				
			case EnemyAnimationState.Walk:
				_animations.Animation = "walking";
				break;
		}
	}

	public void _on_Sprite_animation_finished()
	{
		if (_movement == Vector2.Zero && IsOnFloor())
		{
			_state = EnemyState.Idle;
		}
		else if (IsOnFloor())
		{
			_state = EnemyState.Walking;
		}
	}
}
