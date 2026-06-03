using Godot;

// A bigger enemy for the Level 5 boss fight.
// It flies above the player, shakes as a warning, slams down,
// sends shockwaves across the ground, then waits briefly while vulnerable.
public partial class BossEnemy : Area2D
{
    [Export] public float PatrolDistance = 760.0f;
    [Export] public float PatrolSpeed = 105.0f;
    [Export] public float FollowSpeed = 150.0f;
    [Export] public float FollowRange = 620.0f;
    [Export] public float DropSpeed = 900.0f;
    [Export] public float RiseSpeed = 360.0f;
    [Export] public float GroundY = 338.0f;
    [Export] public float AttackDelay = 2.0f;

    private enum BossState
    {
        Flying,
        Warning,
        Dropping,
        Landed,
        Rising
    }

    private BossState _state = BossState.Flying;
    private Vector2 _startPosition;
    private Vector2 _warningPosition;
    private float _direction = 1.0f;
    private float _landedTime = 0.0f;
    private float _attackTimer = 1.2f;
    private float _warningTime = 0.0f;
    private float _touchCooldown = 0.0f;
    private bool _canTakeHit = false;
    private bool _wavesSpawned = false;
    private int _health = 6;
    private Polygon2D _invulnerableRing;
    private PackedScene _shockwaveScene = GD.Load<PackedScene>("res://Scenes/BossShockwave.tscn");
    private PackedScene _deathEffectScene = GD.Load<PackedScene>("res://Scenes/BossDeathEffect.tscn");

    public override void _Ready()
    {
        Monitoring = true;
        Monitorable = true;
        _startPosition = Position;
        _invulnerableRing = GetNode<Polygon2D>("Body/InvulnerableRing");
        BodyEntered += OnBodyEntered;
        UpdateHealthDots();
        UpdateInvulnerableRing();
    }

    public override void _Process(double delta)
    {
        float seconds = (float)delta;
        _touchCooldown = Mathf.Max(0.0f, _touchCooldown - seconds);

        Player player = GameManager.Find(this)?.GetPlayer();

        if (_state == BossState.Flying)
        {
            _attackTimer -= seconds;
            FlyNearPlayerOrPatrol(player, seconds);

            if (player != null && _attackTimer <= 0.0f && IsPlayerClose(player))
            {
                _state = BossState.Warning;
                _warningTime = 0.65f;
                _warningPosition = Position;
                SoundManager.Play(this, "res://Audio/boss_shake.wav", -5.0f);
            }
        }
        else if (_state == BossState.Warning)
        {
            _warningTime -= seconds;
            float shake = Mathf.Sin(_warningTime * 90.0f) * 8.0f;
            Position = _warningPosition + new Vector2(shake, 0);

            if (_warningTime <= 0.0f)
            {
                Position = _warningPosition;
                _state = BossState.Dropping;
            }
        }
        else if (_state == BossState.Dropping)
        {
            Position = new Vector2(Position.X, Mathf.MoveToward(Position.Y, GroundY, DropSpeed * seconds));

            if (Mathf.IsEqualApprox(Position.Y, GroundY))
            {
                _state = BossState.Landed;
                _landedTime = 1.8f;
                _canTakeHit = true;
                _wavesSpawned = false;
                SoundManager.Play(this, "res://Audio/boss_slam.wav", -3.0f);
                SpawnShockwaves();
            }
        }
        else if (_state == BossState.Landed)
        {
            _landedTime -= seconds;

            if (_landedTime <= 0.0f)
            {
                _state = BossState.Rising;
                _canTakeHit = false;
            }
        }
        else if (_state == BossState.Rising)
        {
            Position = new Vector2(Position.X, Mathf.MoveToward(Position.Y, _startPosition.Y, RiseSpeed * seconds));

            if (Mathf.IsEqualApprox(Position.Y, _startPosition.Y))
            {
                _state = BossState.Flying;
                _attackTimer = AttackDelay;
            }
        }

        UpdateInvulnerableRing();
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (Node2D body in GetOverlappingBodies())
        {
            TouchPlayer(body);
        }

        Player player = GameManager.Find(this)?.GetPlayer();
        if (player != null && GlobalPosition.DistanceTo(player.GlobalPosition) < 92.0f)
        {
            TouchPlayer(player);
        }
    }

    public bool TryTakeHit()
    {
        if (_state != BossState.Landed || !_canTakeHit || _health <= 0)
        {
            return false;
        }

        _canTakeHit = false;
        _health--;
        UpdateHealthDots();

        if (_health <= 0)
        {
            GameState.CompletedGame = true;
            GameState.ShowedCompletionConfetti = false;
            GameManager.Find(this)?.StopBossMusic();
            Node2D deathEffect = _deathEffectScene.Instantiate<Node2D>();
            deathEffect.GlobalPosition = GlobalPosition;
            GetTree().CurrentScene.AddChild(deathEffect);
            SoundManager.Play(this, "res://Audio/boss_death.wav", -3.0f);
            QueueFree();
        }

        return true;
    }

    private void FlyNearPlayerOrPatrol(Player player, float seconds)
    {
        if (player != null && IsPlayerClose(player))
        {
            GameManager.Find(this)?.StartBossMusic();
            float targetX = Mathf.Clamp(player.GlobalPosition.X, _startPosition.X - PatrolDistance, _startPosition.X + PatrolDistance);
            Position = new Vector2(Mathf.MoveToward(Position.X, targetX, FollowSpeed * seconds), _startPosition.Y);
            return;
        }

        Patrol(seconds);
    }

    private void Patrol(float seconds)
    {
        Position += new Vector2(_direction * PatrolSpeed * seconds, 0);

        if (Mathf.Abs(Position.X - _startPosition.X) >= PatrolDistance)
        {
            _direction *= -1.0f;
        }
    }

    private bool IsPlayerClose(Player player)
    {
        return Mathf.Abs(player.GlobalPosition.X - GlobalPosition.X) <= FollowRange;
    }

    private void SpawnShockwaves()
    {
        if (_wavesSpawned)
        {
            return;
        }

        _wavesSpawned = true;
        SpawnShockwave(-1.0f);
        SpawnShockwave(1.0f);
    }

    private void SpawnShockwave(float direction)
    {
        BossShockwave wave = _shockwaveScene.Instantiate<BossShockwave>();
        wave.Direction = direction;
        wave.GlobalPosition = new Vector2(GlobalPosition.X + direction * 74.0f, GlobalPosition.Y + 74.0f);
        GetTree().CurrentScene.AddChild(wave);
    }

    private void OnBodyEntered(Node2D body)
    {
        TouchPlayer(body);
    }

    private void TouchPlayer(Node2D body)
    {
        if (_touchCooldown > 0.0f || body is not Player player)
        {
            return;
        }

        if (player.TakeDamage())
        {
            _touchCooldown = 0.25f;
            return;
        }

        _touchCooldown = 0.7f;
        GameManager.Find(this)?.RespawnPlayer();
    }

    private void UpdateHealthDots()
    {
        for (int i = 1; i <= 6; i++)
        {
            Polygon2D dot = GetNodeOrNull<Polygon2D>($"Body/Health{i}");
            if (dot != null)
            {
                dot.Visible = i <= _health;
            }
        }
    }

    private void UpdateInvulnerableRing()
    {
        _invulnerableRing.Visible = !_canTakeHit;
        _invulnerableRing.RotationDegrees += 90.0f * (float)GetProcessDeltaTime();
    }
}
