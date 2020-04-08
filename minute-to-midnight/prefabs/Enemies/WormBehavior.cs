using Godot;
using Array = Godot.Collections.Array;

public enum WormAnimationState
{
	Idle,
	Walk,
	Death,
	Attack1
}

public enum WormState
{
	Idle,
	Walking,
	Attacking,
	Dead
}

public class WormBehavior : KinematicBody2D
{
	[Signal] public delegate void WormKilled();

	[Export] public float Speed = 50;
	[Export] public float Gravity = 9.8f;
	[Export] public float Damage = 1f;
	[Export] public float Pain = 0.06f;
	[Export] public int HitsToDestroy = 1;

	private const int ChangeDirection = -1;

	private Vector2 _movement;
	private readonly Vector2 _floor = new Vector2(0, -1);

	private float _painDuration = -1f;
	
	private WormState _state;

	private WormAnimationState _animationState;
	private AnimatedSprite _animations;
	private Node2D _display;

	private RayCast2D _groundCheck;
	private RayCast2D _behindCheck;

	private AudioStreamPlayer2D _deathSound;

	private bool deathAnimationHasPlayed = false;

	public override void _Ready()
	{
		_movement = new Vector2();
		_animations = GetNode<AnimatedSprite>("Display/AnimatedSprite");

		_display = GetNode<Node2D>("Display");

		_groundCheck = GetNode<RayCast2D>("GroundCheck");

		_behindCheck = GetNode<RayCast2D>("Display/BehindCheck");

		_deathSound = GetNode<AudioStreamPlayer2D>("DeathSound");
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
			_state = WormState.Dead;

			GetNodeOrNull<CollisionShape2D>("CollisionShape2D")?.QueueFree();
			GetNodeOrNull<Area2D>("DamageArea")?.QueueFree();
		}

		UpdatePlayerState();
		UpdateAnimation();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(float delta)
	{
		if (_state == WormState.Dead)
		{
			return;
		}

		if (_behindCheck.IsColliding()) // If the player is behind the ghost.
		{
			var body = _behindCheck.GetCollider();
			if (body != null)
			{
				var bodyAsNode = (Node2D)body;
				if (bodyAsNode.Name == "Player")
				{
					_display.Scale = new Vector2(-1, 1);
					_state = WormState.Attacking;

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
			case WormState.Attacking:
				_animationState = WormAnimationState.Attack1;
				break;

			case WormState.Walking:
				_animationState = WormAnimationState.Walk;
				break;

			case WormState.Dead:
				_animationState = WormAnimationState.Death;
				if (!_deathSound.Playing && !deathAnimationHasPlayed)
				{
					_deathSound.Playing = true;
					deathAnimationHasPlayed = true;
				}
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
			case WormAnimationState.Death:
				_animations.Animation = "death";
				break;

			case WormAnimationState.Attack1:
				_animations.Animation = "attack_1";
				break;

			case WormAnimationState.Walk:
				_animations.Animation = "walking";
				break;
		}
	}

	public void DealDamageToEnemy(int Damage)
	{
		GD.Print("Damage Dealt To Worm: " + Damage);
		HitsToDestroy -= Damage;
		_painDuration = Pain;
	}

	public void _on_AnimatedSprite_animation_finished()
	{
		if (_movement == Vector2.Zero && IsOnFloor())
		{
			_state = WormState.Walking;
		}

		else if (IsOnFloor())
		{
			_state = WormState.Walking;
		}

		if (_state == WormState.Attacking)
		{
			_state = WormState.Walking;
		}

		if (_state == WormState.Dead || _animationState == WormAnimationState.Death)
		{
			EmitSignal(nameof(WormKilled));

			QueueFree();
		}
	}

	public void _on_DamageArea_body_entered(Node body)
	{
		_state = WormState.Attacking;

		if (body.Name == "Player")
		{
			float[] timeArg = { Damage };
			body.PropagateCall(nameof(Light.RemoveTimeFromTimer), new Array(timeArg));
		}
	}
}
