using System;
using Godot;

public class LightStrength
{
    private Tuple<float, float> _energyLimits;
    private Tuple<float, float> _scaleLimits;

    private float _energy;
    private float _scale;

    public LightStrength(Tuple<float, float> energyLimits, Tuple<float, float> scaleLimits)
    {
        _energyLimits = energyLimits;
        _scaleLimits = scaleLimits;

        _energy = energyLimits.Item2;
        _scale = scaleLimits.Item2;
    }

    private Vector2 GetScaleAsVector2()
    {
        return new Vector2(Scale, Scale);
    }

    public float Energy
    {
        get { return _energy; }
        set
        {
            _energy = Mathf.Clamp(value, _energyLimits.Item1, _energyLimits.Item2);
        }
    }

    public float Scale
    {
        get { return _scale; }
        set
        {
            _scale = Mathf.Clamp(value, _scaleLimits.Item1, _scaleLimits.Item2);
        }
    }

    public float Flicker { get; set; } = 1.0f;

    public void Apply(Light2D light2DNode)
    {
        light2DNode.Energy = Energy * Flicker;
        light2DNode.SetScale(GetScaleAsVector2());
    }
}

public class Light : KinematicBody2D
{
    private readonly Tuple<float, float> _scaleLimits = Tuple.Create(0.1f, 1.0f);
    private readonly Tuple<float, float> _energyLimits = Tuple.Create(0.0f, 1.0f);
    private readonly Tuple<float, float> _flickerLimits = Tuple.Create(1.0f, 1.1f);
    private readonly float _maxTime = 60;

    [Export]
    public bool Debug = false;

    [Export]
    public int MaxSpeed = 10;

    [Export]
    public float Duration = 60.0f;

    [Export]
    public bool DisableDimming = false;

    [Export]
    public bool Flicker = true;

    [Signal]
    public delegate void extinguished();

    private int _speedMultiplier = 100;

    private float _timeRemaining;

    private Timer _timer;

    private Light2D _lightSource;

    private LightStrength _lightStrength;

    private Vector2 GetInput()
    {
        return new Vector2(
            Input.GetActionStrength("move_light_right") - Input.GetActionStrength("move_light_left"),
            Input.GetActionStrength("move_light_down") - Input.GetActionStrength("move_light_up")
        ).Normalized();
    }

    public override void _Ready()
    {
        _timer = GetNode<Timer>("Timer");
        _lightSource = GetNode<Light2D>("Light2D");
        _timeRemaining = Duration;

        _lightStrength = new LightStrength(_energyLimits, _scaleLimits);
        _lightStrength.Apply(_lightSource);

        if (!DisableDimming || Flicker)
        {
            _timer.Start();
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (Debug)
        {
            var velocity = GetInput() * _speedMultiplier * MaxSpeed * delta;
            Position += velocity;
        }
    }

    public void _on_Timer_timeout()
    {
        if (Flicker)
        {
            var noise = (float)(new Random()).NextDouble() * (_flickerLimits.Item2 - _flickerLimits.Item1) + _flickerLimits.Item1;
            _lightStrength.Flicker = noise;
        }

        if (!DisableDimming)
        {
            GD.Print("DIMMING");
            _timeRemaining = Mathf.Max(_timeRemaining - _timer.WaitTime, 0);
            var percentTimeRemaining = _timeRemaining / Duration;

            _lightStrength.Energy = percentTimeRemaining * (_energyLimits.Item2 - _energyLimits.Item1) + _energyLimits.Item1;
            _lightStrength.Scale = percentTimeRemaining * (_scaleLimits.Item2 - _scaleLimits.Item1) + _scaleLimits.Item1;

            GD.Print(percentTimeRemaining);
        }

        _lightStrength.Apply(_lightSource);

        if (_timeRemaining == 0)
        {
            _timer.Stop();
            EmitSignal(nameof(extinguished));
        }
    }
}
