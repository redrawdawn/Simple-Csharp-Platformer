using Godot;

// A small patrolling enemy. Touching it respawns the player.
public partial class Enemy : Area2D
{
    [Export] public float Speed = 80.0f;
    [Export] public float PatrolDistance = 130.0f;
    [Export] public bool RespawnWhileBossAlive = false;
    [Export] public float RespawnDelay = 10.0f;

    private Vector2 _startPosition;
    private float _direction = 1.0f;
    private float _touchCooldown = 0.0f;
    private float _respawnTimer = 0.0f;
    private bool _defeated = false;

    public bool IsDefeated => _defeated;

    public override void _Ready()
    {
        Monitoring = true;
        Monitorable = true;
        _startPosition = Position;
        BodyEntered += OnBodyEntered;
    }

    public override void _Process(double delta)
    {
        if (_defeated)
        {
            _respawnTimer -= (float)delta;

            if (_respawnTimer <= 0.0f && IsBossAlive())
            {
                RespawnEnemy();
            }

            return;
        }

        _touchCooldown = Mathf.Max(0.0f, _touchCooldown - (float)delta);
        Position += new Vector2(_direction * Speed * (float)delta, 0);
        RotationDegrees += _direction * 70.0f * (float)delta;

        if (Mathf.Abs(Position.X - _startPosition.X) >= PatrolDistance)
        {
            _direction *= -1.0f;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_defeated)
        {
            return;
        }

        foreach (Node2D body in GetOverlappingBodies())
        {
            TouchPlayer(body);
        }

        Player player = GameManager.Find(this)?.GetPlayer();
        if (player != null && GlobalPosition.DistanceTo(player.GlobalPosition) < 48.0f)
        {
            TouchPlayer(player);
        }
    }

    public void Defeat()
    {
        if (RespawnWhileBossAlive && IsBossAlive())
        {
            _defeated = true;
            _respawnTimer = RespawnDelay;
            Visible = false;
            Monitoring = false;
            Monitorable = false;
            return;
        }

        QueueFree();
    }

    private void RespawnEnemy()
    {
        _defeated = false;
        _touchCooldown = 0.0f;
        Position = _startPosition;
        Visible = true;
        Monitoring = true;
        Monitorable = true;
    }

    private bool IsBossAlive()
    {
        return GetTree().CurrentScene.FindChild("BossEnemy", true, false) is BossEnemy;
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

        _touchCooldown = 0.6f;
        GameManager.Find(this)?.RespawnPlayer();
    }
}
