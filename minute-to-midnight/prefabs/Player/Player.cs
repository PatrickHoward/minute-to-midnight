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
	Running,
	Jumping,
	Falling,
	Attacking,
	Dead,
	Escaped
}

public class Player : KinematicBody2D
{
	[Export] public float JumpHeight = 250;
	[Export] public float Speed = 75;
	[Export] public float Gravity = 9.8f;
	[Export] public float Pain = 0.06f;
	[Export] public bool DisableDimming = false;

	[Export] public bool HasKey = false;

	private Vector2 _movement;
	private Vector2 _floor = new Vector2(0, -1);
	
	private int _gemcount;

	private PlayerState _state;

	private PlayerAnimationState _animationState;

	private AnimationPlayer _animationPlayer;
	private Sprite _sprite;

	private Node2D _display;
	private Area2D _damageArea;

	private AudioStreamPlayer2D _audioPlayer;

	private float _painDuration = -1f;
	
	private PackedScene _gameOverScreen;
	private PackedScene _youWinScreen;

	public override void _Ready()
	{
		_gameOverScreen = ResourceLoader.Load<PackedScene>("res://scenes/menu/gameover/GameOver.tscn");
		_youWinScreen = ResourceLoader.Load<PackedScene>("res://scenes/menu/youwin/YouWin.tscn");

		_movement = new Vector2();
		_gemcount = 0;

		_display = GetNode<Node2D>("Display");
		_damageArea = GetNode<Area2D>("Display/DamageArea");

		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_sprite = GetNode<Sprite>("Display/Sprite");    
		_animationPlayer.Play("idle");

		_audioPlayer = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
		
		GetNode<Light>("Display/Light").DisableDimming = DisableDimming;
	}

	public override void _Process(float delta)
	{
		// This is garbage, dont do this.
		if (_painDuration > 0)
		{
			_painDuration -= delta;
			_sprite.SelfModulate = Color.Color8(255, 65, 65);
		}
		else
		{
			_sprite.SelfModulate = Color.Color8(255, 255, 255);
		}
		
		if (_state == PlayerState.Dead || _state == PlayerState.Escaped)
		{
			return;
		}

		UpdatePlayerState();
		UpdateAnimation();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(float delta)
	{
		if (_state == PlayerState.Dead || _state == PlayerState.Escaped)
		{
			if (!IsOnFloor())
			{
				ApplyGravity();
			}

			return;
		}

		//Left and Right Movement
		if (Input.IsActionPressed("Move-Left"))
		{
			if (IsOnFloor() && _state != PlayerState.Attacking)
			{
				_state = PlayerState.Running;
			}

			_movement.x = -Speed;
		}
		else if (Input.IsActionPressed("Move-Right"))
		{
			if (IsOnFloor() && _state != PlayerState.Attacking)
			{
				_state = PlayerState.Running;
			}

			_movement.x = Speed;
		}
		else
		{
			_movement.x = 0;
		}

		//Jump Action
		if (Input.IsActionPressed("Jump"))
		{
			if (IsOnFloor() && _animationState != PlayerAnimationState.JumpLoop)
			{
				_state = PlayerState.Jumping;
				_movement.y = -JumpHeight;
			}
		}

		if (Input.IsActionPressed("Action"))
		{
			_state = PlayerState.Attacking;
			//Action button 
		}

		if (IsOnFloor() && _state == PlayerState.Jumping)
		{
			_state = PlayerState.Idle;
		}

		//Constant weight of gravity pushes down on us all
		ApplyGravity();

		if (_movement == Vector2.Zero && _state != PlayerState.Attacking)
		{
			_state = PlayerState.Idle;
		}
		else if (_movement.y > 0)
		{
			_state = PlayerState.Falling;
		}

		if (_animationPlayer.CurrentAnimation == "" && _state == PlayerState.Attacking)
		{
			_state = PlayerState.Idle;
		}

	}

	private void ApplyGravity()
	{
		_movement.y += Gravity;
		_movement = MoveAndSlide(_movement, _floor);
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

			case PlayerState.Running:
				_animationState = PlayerAnimationState.Run;
				break;

			case PlayerState.Jumping:
				if (_animationState != PlayerAnimationState.JumpStart &&
					_animationState != PlayerAnimationState.JumpLoop)
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
			_display.Scale = new Vector2(-1, 1);
		}
		else if (_movement.x > 0)
		{
			_display.Scale = new Vector2(1, 1);
		}

		if (_animationState == PlayerAnimationState.JumpLoop && IsOnFloor())
		{
			_animationState = PlayerAnimationState.Idle;
		}

		switch (_animationState)
		{
			case PlayerAnimationState.Idle:
				_animationPlayer.Play("idle");
				break;

			case PlayerAnimationState.Run:
				_animationPlayer.Play("run");
				break;

			case PlayerAnimationState.Death:
				_animationPlayer.Play("death");
				break;

			case PlayerAnimationState.JumpStart:
				_animationPlayer.Play("jump_start");
				break;

			case PlayerAnimationState.FallLoop:
			case PlayerAnimationState.JumpLoop:
				_animationPlayer.Play("jump_loop");
				break;

			case PlayerAnimationState.Attack1:
				if (_animationPlayer.AssignedAnimation == "attack_1")
				{
					break;
				}
				_animationPlayer.Play("attack_1");
				break;
		}
	}

	public void CollectKey()
	{
		HasKey = true;
	}
	
	public void CollectGem()
	{
		_gemcount++;
		GD.Print("Gem Count: " + _gemcount);
	}

	public void PerformMeleeAttack()
	{
		var bodies = _damageArea.GetOverlappingBodies();

		if (bodies.Count == 0)
		{
			return;
		}

		foreach (var body in bodies)
		{
			var bodyAsNode = (Node2D)body;
			bodyAsNode.Call("DealDamageToEnemy");
		}
	}

	public void _on_Light_LightDimmedByHit()
	{
		_painDuration = Pain;
		var hit = GetNodeOrNull<AudioStreamPlayer2D>("Hit");
		hit.Play();    
	}

	public void _on_Sprite_animation_finished()
	{
		if (_state == PlayerState.Jumping && _animationState == PlayerAnimationState.JumpStart)
		{
			_animationState = PlayerAnimationState.JumpLoop;
		}
	}

	public void _on_Light_extinguished()
	{
		_state = PlayerState.Dead;
		_animationState = PlayerAnimationState.Death;
		UpdatePlayerState();
		UpdateAnimation();

		AddChild(_gameOverScreen.Instance());
	}

	public void _on_Player_Escaped(Node body)
	{
		if (body.Name == "Player")
		{
			_state = PlayerState.Escaped;
			AddChild(_youWinScreen.Instance());
		}
	}
	
}
