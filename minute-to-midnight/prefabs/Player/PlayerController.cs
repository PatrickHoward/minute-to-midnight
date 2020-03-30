using System;
using System.Runtime.InteropServices.WindowsRuntime;
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
    private Vector2 _floor = new Vector2(0, -1);

    private PlayerState _previousState;
    private PlayerState _state;

    private PlayerAnimationState _animationState;

    private AnimationPlayer _animationPlayer;

    private Node2D _display;

    public override void _Ready()
    {
        _movement = new Vector2();

        _display = GetNode<Node2D>("Display");

        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _animationPlayer.Play("idle");
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
        if (Input.IsActionPressed("Move-Left"))
        {
            if (IsOnFloor() && _state != PlayerState.Attacking)
            {
                _state = PlayerState.Runnning;
            }

            _movement.x = -Speed;
        }
        else if (Input.IsActionPressed("Move-Right"))
        {
            if (IsOnFloor() && _state != PlayerState.Attacking)
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

        if (_animationPlayer.CurrentAnimation == "" && _state == PlayerState.Attacking)
        {
            _state = PlayerState.Idle;
        }

    }

    private void UpdatePlayerState()
    {
        switch (_state)
        {
            case PlayerState.Idle:
                _animationState = PlayerAnimationState.Idle;
                Console.WriteLine("Change to Idiling");
                break;

            case PlayerState.Falling:
                _animationState = PlayerAnimationState.FallLoop;
                Console.WriteLine("Change to Falling");
                break;

            case PlayerState.Attacking:
                _animationState = PlayerAnimationState.Attack1;
                Console.WriteLine("Change to attacking");
                break;

            case PlayerState.Runnning:
                _animationState = PlayerAnimationState.Run;
                Console.WriteLine("Change to Running");
                break;

            case PlayerState.Jumping:
                if (_animationState != PlayerAnimationState.JumpStart &&
                    _animationState != PlayerAnimationState.JumpLoop)
                {
                    _animationState = PlayerAnimationState.JumpStart;
                }
                Console.WriteLine("Change to Jumping");
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

    public void _on_Sprite_animation_finished()
    {
        if (_state == PlayerState.Jumping && _animationState == PlayerAnimationState.JumpStart)
        {
            _animationState = PlayerAnimationState.JumpLoop;
        }
    }
}