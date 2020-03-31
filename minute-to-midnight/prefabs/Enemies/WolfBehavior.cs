using Godot;
using Array = Godot.Collections.Array;

public enum WolfAnimationState
{
	Idle,
	Walk,
	Death,
	Attack1
}

public enum WolfState
{
	Idle,
	Walking,
	Attacking,
	Dead
}

public class WolfBehavior : KinematicBody2D
{
	[Signal] public delegate void WolfKilled();
	
	[Export] public float Speed = 50;
	[Export] public float Gravity = 9.8f;
	[Export] public float Damage = 5f;
	[Export] public float Pain = 0.06f;
	[Export] public int HitsToDestroy = 1;

	private const int ChangeDirection = -1;

	private float _painDuration = -1f;
	
	private Vector2 _movement;
	private readonly Vector2 _floor = new Vector2(0, -1);

	private WolfState _state;
	
	private WolfAnimationState _animationState;
	private AnimatedSprite _animations;
	private Node2D _display;

	private RayCast2D _groundCheck;
	private RayCast2D _behindCheck;

	public override void _Ready()
	{
		_movement = new Vector2();
		_animations = GetNode<AnimatedSprite>("Display/AnimatedSprite");

		_display = GetNode<Node2D>("Display");

		_groundCheck = GetNode<RayCast2D>("GroundCheck");
			
		_behindCheck = GetNode<RayCast2D>("Display/BehindCheck");
	}

	public override void _Process(float delta)
	{
		// This is garbage, dont do this.
		if (_painDuration > 0)
		{
			_painDuration -= delta;
			_animations.SelfModulate = Color.Color8(255, 65, 65);
		}
		else
		{
			_animations.SelfModulate = Color.Color8(255, 255, 255);
		}
		
		if (HitsToDestroy <= 0)
		{
			_state = WolfState.Dead;
			
			GetNodeOrNull<CollisionShape2D>("AreaShape2D")?.QueueFree();
			GetNodeOrNull<Area2D>("DamageArea")?.QueueFree();
		}

		UpdatePlayerState();
		UpdateAnimation();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(float delta)
	{
		if (_state == WolfState.Dead)
		{
			return;
		}

		if (_behindCheck.IsColliding()) // If the player is behind the ghost.
		{
			var body = _behindCheck.GetCollider();
			if (body != null)
			{
				var bodyAsNode = (Node2D) body;
				if (bodyAsNode.Name == "Player")
				{
					_display.Scale = new Vector2(-1, 1);

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
			case WolfState.Attacking:
				_animationState = WolfAnimationState.Attack1;
				break;
			
			case WolfState.Walking:
				_animationState = WolfAnimationState.Walk;
				break;
			
			case WolfState.Dead:
				_animationState = WolfAnimationState.Death;
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
			case WolfAnimationState.Death:
				_animations.Animation = "death";
				break;
			
			case WolfAnimationState.Attack1:
				_animations.Animation = "attack_2";
				break;
				
			case WolfAnimationState.Walk:
				_animations.Animation = "walking";
				break;
		}
	}

	public void DealDamageToEnemy()
	{
		--HitsToDestroy;
		_painDuration = Pain;
	}

	public void _on_AnimatedSprite_animation_finished()
	{
		if (_movement == Vector2.Zero && IsOnFloor())
		{
			_state = WolfState.Walking;
		}

		else if (IsOnFloor())
		{
			_state = WolfState.Walking;
		}

		if (_state == WolfState.Attacking)
		{
			_state = WolfState.Walking;
		}

		if (_state == WolfState.Dead || _animationState == WolfAnimationState.Death)
		{
			EmitSignal(nameof(WolfKilled));
			
			QueueFree();
		}
	}

	public void _on_DamageArea_body_entered(Node body)
	{
		_state = WolfState.Attacking;
		
		if (body.Name == "Player")
		{
			float[] timeArg = { Damage };
			body.PropagateCall(nameof(Light.RemoveTimeFromTimer), new Array(timeArg));
		}
	}
}