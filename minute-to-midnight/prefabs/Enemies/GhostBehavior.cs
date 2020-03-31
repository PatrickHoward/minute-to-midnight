using Godot;
using Array = Godot.Collections.Array;
using System.Collections.Generic;

public enum GhostAnimationState
{
	Idle,
	Walk,
	Death,
	Attack1
}

public enum GhostState
{
	Idle,
	Walking,
	Attacking,
	Dead
}

public class GhostBehavior : KinematicBody2D
{
	[Signal] public delegate void GhostKilled();

	[Export] public float Speed = 50;
	[Export] public float AttackSpeed = 100;
	[Export] public float Gravity = 9.8f;
	[Export] public float Damage = 5f;
	[Export] public int HitsToDestroy = 1;
	[Export] public float PassiveSoundChance = 1.0f;

	private const int ChangeDirection = -1;

	private Vector2 _movement;
	private readonly Vector2 _floor = new Vector2(0, -1);

	private GhostState _state;

	private static Dictionary<string, AudioStreamSample> _soundEffects = new Dictionary<string, AudioStreamSample>
	{ { "passive", ResourceLoader.Load<AudioStreamSample>("res://resources/audio/ghost/passive.wav") },
		{ "attack_1", ResourceLoader.Load<AudioStreamSample>("res://resources/audio/ghost/attack_1.wav") },
		{ "attack_2", ResourceLoader.Load<AudioStreamSample>("res://resources/audio/ghost/attack_2.wav") },
		{ "attack_3", ResourceLoader.Load<AudioStreamSample>("res://resources/audio/ghost/attack_3.wav") }
	};

	private GhostAnimationState _animationState;
	private AnimationPlayer _animationPlayer;

	private AudioStreamPlayer2D _audioStreamPlayer2D;
	private Node2D _display;

	private RayCast2D _groundCheck;
	private RayCast2D _attackCheck;
	private RayCast2D _behindCheck;

	public override void _Ready()
	{
		_movement = new Vector2();
		_display = GetNode<Node2D>("Display");

		_groundCheck = GetNode<RayCast2D>("GroundCheck");

		_attackCheck = GetNode<RayCast2D>("Display/AttackCheck");
		_behindCheck = GetNode<RayCast2D>("Display/BehindCheck");

		_audioStreamPlayer2D = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}

	public override void _Process(float delta)
	{
		if (HitsToDestroy <= 0)
		{
			_state = GhostState.Dead;

			GetNodeOrNull<CollisionShape2D>("AreaShape2D")?.QueueFree();
			GetNodeOrNull<Area2D>("DamageArea")?.QueueFree();
		}

		UpdatePlayerState();
		UpdateAnimation();

		PlayPassiveSoundEffect();
	}

	// Called every frame. 'delta ' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(float delta)
	{
		if (_state == GhostState.Dead)
		{
			return;
		}

		if (_attackCheck.IsColliding()) // If the player is in front of the ghost.
		{
			var body = _attackCheck.GetCollider();
			if (body != null)
			{
				var bodyAsNode = (Node2D)body;
				if (bodyAsNode.Name == "Player")
				{
					_state = GhostState.Attacking;
				}
			}
		}
		else if (_behindCheck.IsColliding()) // If the player is behind the ghost.
		{
			var body = _behindCheck.GetCollider();
			if (body != null)
			{
				var bodyAsNode = (Node2D)body;
				if (bodyAsNode.Name == "Player")
				{
					_display.Scale = new Vector2(-1, 1);
					_state = GhostState.Attacking;

					Speed *= ChangeDirection;
					AttackSpeed *= ChangeDirection;

					_groundCheck.Position *= new Vector2(-1, 1);

				}
			}
		}

		if (IsOnWall() || !_groundCheck.IsColliding())
		{
			Speed *= ChangeDirection;
			AttackSpeed *= ChangeDirection;

			_groundCheck.Position *= new Vector2(-1, 1);
		}

		_movement.x = (_state == GhostState.Attacking) ? AttackSpeed : Speed;

		_movement.y = Gravity;

		_movement = MoveAndSlide(_movement, _floor);

	}

	private void UpdatePlayerState()
	{
		switch (_state)
		{
			case GhostState.Attacking:
				_animationState = GhostAnimationState.Attack1;
				break;

			case GhostState.Walking:
				_animationState = GhostAnimationState.Walk;
				break;

			case GhostState.Dead:
				_animationState = GhostAnimationState.Death;
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
			case GhostAnimationState.Death:
				_animationPlayer.Play("death");
				break;

			case GhostAnimationState.Attack1:
				_animationPlayer.Play("attack_1");
				break;

			case GhostAnimationState.Walk:
				_animationPlayer.Play("walking");
				break;
		}
	}

	public void DealDamageToEnemy()
	{
		--HitsToDestroy;
	}

	private void PlayPassiveSoundEffect()
	{
		if (!_audioStreamPlayer2D.Playing && (_state == GhostState.Walking || _state == GhostState.Idle))
		{
			var rng = new RandomNumberGenerator();
			rng.Randomize();
			if (rng.Randf() < PassiveSoundChance)
			{
				_audioStreamPlayer2D.Stream = _soundEffects["passive"];
				_audioStreamPlayer2D.Playing = true;
			}
		}
	}

	private void PlayRandomSoundEffect(string[] soundEffectNames)
	{
		var rng = new RandomNumberGenerator();
		rng.Randomize();
		var randomSoundEffect = rng.RandiRange(0, soundEffectNames.Length - 1);
		_audioStreamPlayer2D.Stream = _soundEffects[soundEffectNames[randomSoundEffect]];
		_audioStreamPlayer2D.Playing = true;
	}

	public void _on_AnimatedSprite_animation_finished()
	{
		if (_movement == Vector2.Zero && IsOnFloor())
		{
			_state = GhostState.Walking;
		}

		else if (IsOnFloor())
		{
			_state = GhostState.Walking;
		}

		if (_state == GhostState.Attacking)
		{
			_state = GhostState.Walking;
		}

		if (_state == GhostState.Dead || _animationState == GhostAnimationState.Death)
		{
			EmitSignal(nameof(GhostKilled));

			QueueFree();
		}
	}

	public void _on_DamageArea_body_entered(Node body)
	{
		if (body.Name == "Player")
		{
			float[] timeArg = { Damage };
			body.PropagateCall(nameof(Light.RemoveTimeFromTimer), new Array(timeArg));
		}
	}
}
