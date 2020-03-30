using System;
using Godot;
using Array = Godot.Collections.Array;

public enum EnemyAnimationState
{
	Idle,
	Walk,
	Death,
	Attack1
}

public enum EnemyState
{
	Idle,
	Walking,
	Attacking,
	Dead
}

public class EnemyBehavior00 : KinematicBody2D
{
	[Export] public float Speed = 50;
	[Export] public float AttackSpeed = 100;
	[Export] public float Gravity = 9.8f;
	[Export] public float Damage = 5f;
	[Export] public int HitsToDestroy = 1;
	
	private int _changedirection = -1;
	
	private bool _attacking = false;
	private bool _hitwall = false;
	
	private Vector2 _movement;
	private Vector2 _floor = new Vector2(0,-1);

	private EnemyState _state;
	
	private EnemyAnimationState _animationState;
	private AnimatedSprite _animations;
	private Node2D _display;
	
	private RayCast2D _groundCheck;
	private RayCast2D _attackCheck;
	private Area2D _damageArea;
	
	public override void _Ready()
	{
		_movement = new Vector2();
		_animations = GetNode<AnimatedSprite>("Display/AnimatedSprite");

		_display = GetNode<Node2D>("Display");
		
		_groundCheck = GetNode<RayCast2D>("GroundCheck");
		_attackCheck = GetNode<RayCast2D>("Display/AttackCheck");
		_damageArea = GetNode<Area2D>("DamageArea");
	}

	public override void _Process(float delta)
	{
		if (HitsToDestroy <= 0)
		{
			_state = EnemyState.Dead;
		}
		
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

		if (_state == EnemyState.Dead)
		{
			return;
		}
		
		if(IsOnWall() || !_groundCheck.IsColliding())
		{
			Speed *= _changedirection;
			_groundCheck.Position *= new Vector2(-1,1);
		}

		if (_attackCheck.IsColliding())
		{
			var body = _attackCheck.GetCollider().GetClass();
		}
		
		
		_movement.x = _attacking ? AttackSpeed : Speed;
		
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
				_attacking = false;
				break;
			
			case EnemyState.Attacking:
				_animationState = EnemyAnimationState.Attack1;
				_attacking = true;
				break;
			
			case EnemyState.Walking:
				_animationState = EnemyAnimationState.Walk;
				_attacking = false;
				break;
			
			case EnemyState.Dead :
				_animationState = EnemyAnimationState.Death;
				break;
		}
	}

	private void UpdateAnimation()
	{
		if (_movement.x < 0)
		{
			_display.Scale = new Vector2(-1, 1);
		}
		else if (_movement.x > 0)
		{
			_display.Scale = new Vector2(1, 1);
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

	public void DealDamageToEnemy()
	{
		Console.WriteLine("Dealt damage!");
		--HitsToDestroy;
	}

	public void _on_AnimatedSprite_animation_finished()
	{
		if (_movement == Vector2.Zero && IsOnFloor())
		{
			_state = EnemyState.Idle;
		}
		
		else if (IsOnFloor())
		{
			_state = EnemyState.Walking;
		}

		if (_state == EnemyState.Dead || _animationState == EnemyAnimationState.Death)
		{
			QueueFree();
		}
		
	}

	public void _on_DamageArea_body_entered(Node body)
	{
		if (body.Name == "Player")
		{
			float[] timeArg = {Damage};
			body.PropagateCall(nameof(Light.RemoveTimeFromTimer), new Array(timeArg));
		}
	}
}
