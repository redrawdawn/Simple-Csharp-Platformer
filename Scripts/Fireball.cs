using Godot;

// A tiny projectile. After you collect the fire power-up, press F to shoot.
public partial class Fireball : Area2D
{
    [Export] public float Speed = 520.0f;
    public float Direction = 1.0f;

    private float _life = 0.0f;
    private PackedScene _hitEffectScene = GD.Load<PackedScene>("res://Scenes/EnemyHitEffect.tscn");

    public override void _Ready()
    {
        Monitoring = true;
        Monitorable = true;
        BodyEntered += OnBodyEntered;
        AreaEntered += OnAreaEntered;
    }

    public override void _Process(double delta)
    {
        Position += new Vector2(Direction * Speed * (float)delta, 0);
        RotationDegrees += 720.0f * (float)delta;

        _life += (float)delta;
        if (_life > 1.5f)
        {
            QueueFree();
        }

        foreach (Area2D area in GetOverlappingAreas())
        {
            TryHitArea(area);
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        QueueFree();
    }

    private void OnAreaEntered(Area2D area)
    {
        TryHitArea(area);
    }

    private void TryHitArea(Area2D area)
    {
        if (area is BossEnemy boss)
        {
            bool bossWasDamaged = boss.TryTakeHit();

            SoundManager.Play(this, "res://Audio/enemy_hit.wav", bossWasDamaged ? 0.0f : -10.0f);

            Node2D hitEffect = _hitEffectScene.Instantiate<Node2D>();
            hitEffect.GlobalPosition = bossWasDamaged ? boss.GlobalPosition : GlobalPosition;
            hitEffect.Modulate = bossWasDamaged ? Colors.White : new Color(1.0f, 0.15f, 0.08f, 0.65f);
            GetTree().CurrentScene.AddChild(hitEffect);

            QueueFree();
            return;
        }

        if (area is Enemy enemy)
        {
            if (enemy.IsDefeated)
            {
                return;
            }

            SoundManager.Play(this, "res://Audio/enemy_hit.wav");

            Node2D hitEffect = _hitEffectScene.Instantiate<Node2D>();
            hitEffect.GlobalPosition = enemy.GlobalPosition;
            GetTree().CurrentScene.AddChild(hitEffect);

            enemy.Defeat();
            QueueFree();
        }
    }
}
