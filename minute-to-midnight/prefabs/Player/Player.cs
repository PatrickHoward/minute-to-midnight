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

	[Export] public int AllowedJumps = 1;
	[Export] public int PlayerDamage = 1;

	[Export] public bool DisableDimming = false;
	[Export] public bool HasKey = false;
	[Export] public bool FinalAttack;

	private bool _onground;

	private Vector2 _movement;
	private Vector2 _floor = new Vector2(0, -1);

	private float _painDuration = -1f;
	private int _gemcount;
	private int _jumpcount;
	private int _attackcount;

	private PlayerState _state;
	private PlayerAnimationState _animationState;

	private AnimationPlayer _animationPlayer;
	private Sprite _sprite;
	private Node2D _display;
	private Area2D _damageArea;
	private AudioStreamPlayer2D _audioPlayer;

	private PackedScene _gameOverScreen;
	private PackedScene _youWinScreen;
	
	private Popup _pop;
	private ColorRect _popRect;
	private Label _popLabel;

	public override void _Ready()
	{
		//retrieve the scenes for beating a level and losing
		_gameOverScreen = ResourceLoader.Load<PackedScene>("res://scenes/menu/gameover/GameOver.tscn");
		_youWinScreen = ResourceLoader.Load<PackedScene>("res://scenes/menu/youwin/YouWin.tscn");
			
		_popLabel = GetNode<Label>("Display/Sprite/Ability1/PopUpTip/Popup/Label2");
		_popRect = GetNode<ColorRect>("Display/Sprite/Ability1/PopUpTip/Popup/CanvasLayer/ColorRect");
		_pop = GetNode<Popup>("Display/Sprite/Ability1/PopUpTip/Popup");

		//initialize the vectore for movement and the intial gemcount and jumpcount
		_movement = new Vector2();
		_jumpcount = 0;
		_attackcount = 0;
		_onground = false;
		
		//get player state from saved player states
		AllowedJumps = PlayerData.AllowedJumps;
		PlayerDamage = PlayerData.PlayerDamage;
		_gemcount = PlayerData.GemCount;
		FinalAttack = PlayerData.FinalAttack;
		

		//get some nodes for the player character to be used
		_display = GetNode<Node2D>("Display");
		_damageArea = GetNode<Area2D>("Display/DamageArea");

		//retrieve the animation player and set player's initial state
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		GD.Print(_animationPlayer.GetAnimationList());
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
		//check if the player is on the ground and updates the flag accordingly
		if(IsOnFloor())
		{
			_onground = true;
			_jumpcount = 0;
		}
		else
		{
			_onground = false;
		}

		if (_state == PlayerState.Dead || _state == PlayerState.Escaped)
		{
			if (!_onground)
			{
				ApplyGravity();
			}

			return;
		}

		//Left and Right Movement
		if (Input.IsActionPressed("Move-Left"))
		{
			if (_onground && _state != PlayerState.Attacking)
			{
				_state = PlayerState.Running;
			}

			_movement.x = -Speed;
		}
		else if (Input.IsActionPressed("Move-Right"))
		{
			if (_onground && _state != PlayerState.Attacking)
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
		if (Input.IsActionJustPressed("Jump"))
		{
			if (_animationState != PlayerAnimationState.JumpLoop && _jumpcount < AllowedJumps)
			{
				_jumpcount++;
				_onground = false;
				_state = PlayerState.Jumping;
				_movement.y = -JumpHeight;

			}
		}

		//attack action
		if (Input.IsActionJustPressed("Action"))
		{
			_state = PlayerState.Attacking;
		}

		if (_onground && _state == PlayerState.Jumping)
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

	//function to keep applying gravity to player
	private void ApplyGravity()
	{
		_movement.y += Gravity;
		_movement = MoveAndSlide(_movement, _floor);
	}


	//function to update the player to the correct state for its animation
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
				if(_attackcount == 0)
				{
					_animationState = PlayerAnimationState.Attack1;
					break;
				}
				else if(_attackcount == 1)
				{
					_animationState = PlayerAnimationState.Attack2;
					break;
				}
				else if(_attackcount == 2)
				{
					_animationState = PlayerAnimationState.Attack3;
					break;
				}
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

	//function to update the player's animation
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

		if (_animationState == PlayerAnimationState.JumpLoop && _onground)
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
			case PlayerAnimationState.Attack2:
				if (_animationPlayer.AssignedAnimation == "attack_2")
				{
					break;
				}
				_animationPlayer.Play("attack_2");
				break;
			case PlayerAnimationState.Attack3:
				if (_animationPlayer.AssignedAnimation == "attack_3")
				{
					break;
				}
				_animationPlayer.Play("attack_3");
				break;
		}
	}


	//updates the key flag
	//Called by key class
	public void CollectKey()
	{
		HasKey = true;
	}
	
	//Updates the gem count when a gem is collected
	//Called by the PowerGem class
	public void CollectGem()
	{
		_gemcount++;
		PlayerData.GemCount = _gemcount;
		GD.Print("Gem Count: " + _gemcount);
		AbilityCheck();

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
			bodyAsNode.Call("DealDamageToEnemy", PlayerDamage);
			if(_attackcount == 2)
			{
				bodyAsNode.Call("DealDamageToEnemy", 4);
			}
			else
			{
				bodyAsNode.Call("DealDamageToEnemy", PlayerDamage);
			}
		}
	}

	public void _on_Light_LightDimmedByHit()
	{
		_painDuration = Pain;
		var hit = GetNodeOrNull<AudioStreamPlayer2D>("Hit");
		hit.Play();    
	}
	
	//function to handle the jumping loop animation
	public void _on_Sprite_animation_finished()
	{
		if (_state == PlayerState.Jumping && _animationState == PlayerAnimationState.JumpStart)
		{
			_animationState = PlayerAnimationState.JumpLoop;
		}
	}


	//On death screen
	public void _on_Light_extinguished()
	{
		_state = PlayerState.Dead;
		_animationState = PlayerAnimationState.Death;
		UpdatePlayerState();
		UpdateAnimation();

		AddChild(_gameOverScreen.Instance());
	}

	//Level completed screen
	public void _on_Player_Escaped(Node body)
	{
		if (body.Name == "Player")
		{
			_state = PlayerState.Escaped;
			AddChild(_youWinScreen.Instance());
		}
	}
	
	//Checks gem amounts when a gem is collected and updates player's ability
	private void AbilityCheck()
	{
		if(_gemcount == 2)
		{
			PlayerDamage++;
			PlayerData.PlayerDamage = PlayerDamage;
			GD.Print("Player Damage: " + PlayerDamage);
			
			GetTree().Paused = true;
			_popLabel.Text = "Damage Increased";
			_pop.Visible = true;
			_popRect.Visible = true;
		}
		else if(_gemcount == 4)
		{
			AllowedJumps++;
			PlayerData.AllowedJumps = AllowedJumps;
			GD.Print("Allowed Jumps: " + AllowedJumps);
			
			GetTree().Paused = true;
			_popLabel.Text = "Double Jump Unlocked";
			_pop.Visible = true;
			_popRect.Visible = true;
		}
		else if(_gemcount == 7)
		{
			PlayerDamage++;
			PlayerData.PlayerDamage = PlayerDamage;
			GD.Print("Player Damage: " + PlayerDamage);
			
			GetTree().Paused = true;
			_popLabel.Text = "Damage Increased";
			_pop.Visible = true;
			_popRect.Visible = true;
		}
		else if(_gemcount == 10)
		{
			FinalAttack = true;
			PlayerData.FinalAttack = true;
			GD.Print("Final Attack Is Active");
			
			GetTree().Paused = true;
			_popLabel.Text = "Third Attack Added to Combo";
			_pop.Visible = true;
			_popRect.Visible = true;
		}
	}
	
	private void _on_AnimationPlayer_animation_finished(string anim_name)
	{
		if(anim_name == "attack_1")
		{
			_attackcount++;
			_state = PlayerState.Idle;
		}
		else if (anim_name == "attack_2")
		{
			if(FinalAttack)
			{
				_attackcount++;
			}
			else
			{
				_attackcount = 0;
			}
			_state = PlayerState.Idle;
		}
		else if(anim_name == "attack_3")
		{
			_attackcount = 0;
			_state = PlayerState.Idle;
		}
	}
}
