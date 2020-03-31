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

	private const int ChangeDirection = -1;

	private Vector2 _movement;
	private readonly Vector2 _floor = new Vector2(0, -1);

	private MinotaurState _state;
	
	private MinotaurAnimationState _animationState;
	private AnimationPlayer _animations;
	private Node2D _display;

	private RayCast2D _groundCheck;
	private RayCast2D _attackCheck;
	private RayCast2D _behindCheck;

	public override void _Ready()
	{
		_movement = new Vector2();
		_animations = GetNode<AnimationPlayer>("AnimationPlayer");

		_display = GetNode<Node2D>("Display");

		_groundCheck = GetNode<RayCast2D>("GroundCheck");
		
		_attackCheck = GetNode<RayCast2D>("Display/AttackCheck");
		_behindCheck = GetNode<RayCast2D>("Display/BehindCheck");
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

		if (_attackCheck.IsColliding()) // If the player is in front of the ghost.
		{
			var body = _attackCheck.GetCollider();
			if (body != null)
			{
				var bodyAsNode = (Node2D) body;
				if (bodyAsNode.Name == "Player")
				{
					_state = MinotaurState.Attacking;
				}
			}
		}
		else if (_behindCheck.IsColliding()) // If the player is behind the ghost.
		{
			var body = _behindCheck.GetCollider();
			if (body != null)
			{
				var bodyAsNode = (Node2D) body;
				if (bodyAsNode.Name == "Player")
				{
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
		}

		_movement.x = Speed;
		
		_movement.y = Gravity;

		_movement = MoveAndSlide(_movement, _floor);

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
				_animations.Play("walking");
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

	public void _on_AnimationPlayer_animation_finished()
	{
		if (_movement == Vector2.Zero && IsOnFloor())
		{
			_state = MinotaurState.Walking;
		}

		else if (IsOnFloor())
		{
			_state = MinotaurState.Walking;
		}

		if (_state == MinotaurState.Attacking)
		{
			_state = MinotaurState.Walking;
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
			float[] timeArg = { Damage };
			body.PropagateCall(nameof(Light.RemoveTimeFromTimer), new Array(timeArg));
		}
	}
}