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

public class Light : Node2D
{
    private Tuple<float, float> _scaleLimits;
    private Tuple<float, float> _energyLimits;
    private readonly Tuple<float, float> _flickerLimits = Tuple.Create(1.0f, 1.1f);
    private readonly Tuple<float, float> _particleInitialVelocityLimits = Tuple.Create(1.0f, 10.0f);
    private readonly Tuple<float, float> _particleScaleLimits = Tuple.Create(0.0f, 3.0f);
    private readonly Tuple<float, float> _particleScaleRandomnessLimits = Tuple.Create(0.0f, 1.0f);
    private readonly Tuple<float, float> _particlesEmissionSphereRadiusLimits = Tuple.Create(1.0f, 3.0f);
    private readonly Tuple<float, float> _particleLinearAccelerationLimits = Tuple.Create(0.0f, 20.0f);
    private const float Maxtime = 60;

    [Export] public float MaxEnergy = 1.0f;
    
    [Export] public float MinEnergy = 0.5f;
    
    [Export] public float MaxScale = 1.0f;
    
    [Export] public float MinScale = 0.1f;

    [Export] public bool Debug = false;

    [Export] public int MaxSpeed = 10;

    [Export] public float Duration = 60.0f;

    [Export] public bool DisableDimming = false;

    [Export] public bool Flicker = true;

    [Signal] public delegate void extinguished();

    private Timer _timer;
    
    private int _speedMultiplier = 100;

    private float _timeRemaining;

    private Light2D _lightSource;

    private LightStrength _lightStrength;

    private Particles2D _particles;

    public static float LinearModel(Tuple<float, float> limits, float percent)
    {
        return percent * (limits.Item2 - limits.Item1) + limits.Item1;
    }

    public override void _Ready()
    {
        _timer = GetNode<Timer>("Timer");
        _lightSource = GetNode<Light2D>("Light2D");
        _particles = GetNode<Particles2D>("Particles2D");
        _timeRemaining = Duration;

        _scaleLimits = Tuple.Create(MinScale, MaxScale);
        _energyLimits = Tuple.Create(MinEnergy, MaxEnergy);

        _lightStrength = new LightStrength(_energyLimits, _scaleLimits);
        _lightStrength.Apply(_lightSource);

        if (!DisableDimming || Flicker)
        {
            _timer.Start();
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
            _timeRemaining = Mathf.Max(_timeRemaining - _timer.WaitTime, 0);
            var percentTimeRemaining = _timeRemaining / Duration;

            _lightStrength.Energy = LinearModel(_energyLimits, percentTimeRemaining);
            _lightStrength.Scale = LinearModel(_scaleLimits, percentTimeRemaining);

            var particlesMaterial = _particles.ProcessMaterial as ParticlesMaterial;
            particlesMaterial.InitialVelocity = LinearModel(_particleInitialVelocityLimits, percentTimeRemaining);
            particlesMaterial.Scale = LinearModel(_particleScaleLimits, percentTimeRemaining);
            particlesMaterial.ScaleRandom = LinearModel(_particleScaleRandomnessLimits, percentTimeRemaining);
            particlesMaterial.EmissionSphereRadius = LinearModel(_particlesEmissionSphereRadiusLimits, percentTimeRemaining);
            particlesMaterial.LinearAccel = LinearModel(_particleLinearAccelerationLimits, percentTimeRemaining);
            _lightStrength.Apply(_lightSource);
            
            if (_timeRemaining <= 0)
            {
                _timer.Stop();

                EmitSignal(nameof(extinguished));
            }
        }
    }

    public void AddTimeToTimer(float time)
    {
        if (!DisableDimming)
        {
            _timeRemaining = Mathf.Min(time + _timeRemaining, Maxtime);
        }
    }

    public void RemoveTimeFromTimer(float time)
    {
        if (!DisableDimming)
        {
            _timeRemaining -= Mathf.Clamp(time, 0, Maxtime);
        }
    }
}
