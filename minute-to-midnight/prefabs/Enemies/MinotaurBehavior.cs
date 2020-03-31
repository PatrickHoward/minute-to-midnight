using System;
using Godot;
using Array = Godot.Collections.Array;

public enum MinotaurAnimationState
{
	Idle,
	Walk,
	Death,
	Attack1
}

public enum MinotaurState
{
	Idle,
	Walking,
	Attacking,
	Dead
}

public class MinotaurBehavior : KinematicBody2D
{
	[Export] public float Speed = 10f;
	[Export] public float Gravity = 9.8f;
	[Export] public float Damage = 25f;
	[Export] public int HitsToDestroy = 15;
	[Export] public float IdleTimeout = 30f;
	
	private const int ChangeDirection = -1;

	private float _timeoutDuration;
	
	private Vector2 _movement;
	private readonly Vector2 _floor = new Vector2(0, -1);

	private MinotaurState _state;
	
	private MinotaurAnimationState _animationState;
	private AnimationPlayer _animations;
	private Node2D _display;

	private RayCast2D _groundCheck;
	private RayCast2D _behindCheck;

	private Area2D _damageArea;
	
	public override void _Ready()
	{
		_state = MinotaurState.Walking;

		_timeoutDuration = IdleTimeout;
		
		_movement = new Vector2();
		_animations = GetNode<AnimationPlayer>("AnimationPlayer");

		_display = GetNode<Node2D>("Display");

		_groundCheck = GetNode<RayCast2D>("GroundCheck");
		_behindCheck = GetNode<RayCast2D>("Display/BehindCheck");

		_damageArea = GetNode<Area2D>("Display/DamageArea");
	}

	public override void _Process(float delta)
	{
		if (HitsToDestroy <= 0)
		{
			_state = MinotaurState.Dead;
			
			GetNodeOrNull<CollisionShape2D>("AreaShape2D")?.QueueFree();
			GetNodeOrNull<Area2D>("DamageArea")?.QueueFree();
		}

		UpdatePlayerState();
		UpdateAnimation();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(float delta)
	{
		if (_state == MinotaurState.Dead)
		{
			return;
		}

		if (_state == MinotaurState.Idle)
		{
			_timeoutDuration -= delta;
			if (_timeoutDuration <= 0)
			{
				_state = MinotaurState.Walking;
				_timeoutDuration = IdleTimeout;
			}

			return;
		}

		if (_behindCheck.IsColliding()) // If the player is behind the Minotaur.
		{
			var body = _behindCheck.GetCollider();
			if (body != null)
			{
				var bodyAsNode = (Node2D) body;
				if (bodyAsNode.Name == "Player")
				{
					Console.WriteLine("BOO!");
					
					_display.Scale = new Vector2(-1, 1);
					_state = MinotaurState.Attacking;

					Speed *= ChangeDirection;

					_groundCheck.Position *= new Vector2(-1, 1);

				}
			}
		}

		if (IsOnWall() || !_groundCheck.IsColliding())
		{
			Speed *= ChangeDirection;

			_groundCheck.Position *= new Vector2(-1, 1);

			_state = MinotaurState.Idle;
		}

		_movement.x = Speed;

		_movement.y = Gravity;

		if (_movement == Vector2.Zero)
		{
			_state = MinotaurState.Idle;
		}

		if (_state != MinotaurState.Attacking)
		{
			_movement = MoveAndSlide(_movement, _floor);
		}
	}

	private void UpdatePlayerState()
	{
		switch (_state)
		{
			case MinotaurState.Attacking:
				_animationState = MinotaurAnimationState.Attack1;
				break;
			
			case MinotaurState.Walking:
				_animationState = MinotaurAnimationState.Walk;
				break;
			
			case MinotaurState.Dead:
				_animationState = MinotaurAnimationState.Death;
				break;
			
			case MinotaurState.Idle:
				_animationState = MinotaurAnimationState.Idle;
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
			case MinotaurAnimationState.Death:
				_animations.Play("death");
				break;
			
			case MinotaurAnimationState.Attack1:
				_animations.Play("attack_1");
				break;
				
			case MinotaurAnimationState.Walk:
				_animations.Play("walk");
				break;
			
			case MinotaurAnimationState.Idle:
				_animations.Play("idle");
				break;
		}
	}

	public void DealDamageToEnemy()
	{
		--HitsToDestroy;
	}

	public void AttemptDamage()
	{
		var bodies = _damageArea.GetOverlappingBodies();
        
		if (bodies.Count == 0)
		{
			return;
		}
        
		foreach (var body in bodies)
		{
			var bodyAsNode = (Node2D) body;
			if (bodyAsNode.Name == "Player")
			{
				float[] timeArg = { Damage };
				bodyAsNode.PropagateCall(nameof(Light.RemoveTimeFromTimer), new Array(timeArg));
			}
		}
	}
	
	public void _on_AnimationPlayer_animation_finished(String anim_name)
	{
		if (_state == MinotaurState.Attacking)
		{
			_state = MinotaurState.Idle;
		}

		if (_state == MinotaurState.Dead || _animationState == MinotaurAnimationState.Death)
		{
			QueueFree();
		}
	}

	public void _on_MinotaurDamageArea_body_entered(Node body)
	{
		if (body.Name == "Player")
		{
			_state = MinotaurState.Attacking;
		}
	}

	public void _on_MinotaurDamageArea_body_exited(Node body)
	{
		
	}
}