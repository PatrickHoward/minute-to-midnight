using Godot;

public class Light : KinematicBody2D
{
    private readonly float _maxScale = 1.0f;
    private readonly float _minScale = 0.1f;
    private readonly float _maxEnergy = 2.0f;
    private readonly float _minEnergy = 0.0f;
    private readonly float _maxTime = 60;

    [Export]
    public bool debug = false;

    [Export]
    public int MaxSpeed = 10;

    [Export]
    public float Duration = 60.0f;

    [Export]
    public bool DisableDimming = false;

    private int _speedMultiplier = 100;

    private float _timeRemaining;

    private Timer _timer;

    private Light2D _lightSource;

    private Vector2 GetInput()
    {
        return new Vector2(
            Input.GetActionStrength("move_light_right") - Input.GetActionStrength("move_light_left"),
            Input.GetActionStrength("move_light_down") - Input.GetActionStrength("move_light_up")
        ).Normalized();
    }

    private void setStrength()
    {
        var percentTimeRemaining = _timeRemaining / Duration;

        var energy = percentTimeRemaining * (_maxEnergy - _minEnergy) + _minEnergy;
        _lightSource.Energy = percentTimeRemaining = energy;

        var scale = percentTimeRemaining * (_maxScale - _minScale) + _minScale;
        _lightSource.SetScale(new Vector2(scale, scale));
    }

    public override void _Ready()
    {
        _timer = GetNode<Timer>("Timer");
        _lightSource = GetNode<Light2D>("Light2D");
        _timeRemaining = Duration;

        if (!DisableDimming)
        {
            _timer.Start();
        }

        _lightSource.Energy = _maxEnergy;
        _lightSource.SetScale(new Vector2(_maxScale, _maxScale));
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (debug)
        {
            var velocity = GetInput() * _speedMultiplier * MaxSpeed * delta;
            Position += velocity;
        }
    }

    public void _on_Timer_timeout()
    {
        _timeRemaining = Mathf.Max(_timeRemaining - _timer.WaitTime, 0);
        setStrength();
        if (_timeRemaining == 0)
        {
            _timer.Stop();
        }
    }
}
