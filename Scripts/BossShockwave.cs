using Godot;

// A low red wave from the boss slam.
// It stays near the ground, so the player can jump over it.
public partial class BossShockwave : Area2D
{
    [Export] public float Speed = 360.0f;
    [Export] public float LifeTime = 1.45f;

    public float Direction = 1.0f;

    private float _age = 0.0f;
    private float _damageCooldown = 0.0f;

    public override void _Ready()
    {
        Monitoring = true;
        Monitorable = true;
        BodyEntered += OnBodyEntered;
        Scale = new Vector2(Direction, 1.0f);
    }

    public override void _Process(double delta)
    {
        float seconds = (float)delta;
        Position += new Vector2(Direction * Speed * seconds, 0);
        _age += seconds;
        _damageCooldown = Mathf.Max(0.0f, _damageCooldown - seconds);

        if (_age >= LifeTime)
        {
            QueueFree();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (Node2D body in GetOverlappingBodies())
        {
            TryDamage(body);
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        TryDamage(body);
    }

    private void TryDamage(Node2D body)
    {
        if (_damageCooldown > 0.0f || body is not Player player)
        {
            return;
        }

        _damageCooldown = 0.4f;

        if (!player.TakeDamage())
        {
            GameManager.Find(this)?.RespawnPlayer();
        }
    }
}
