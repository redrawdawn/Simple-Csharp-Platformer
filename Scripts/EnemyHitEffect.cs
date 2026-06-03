using Godot;

// A quick pop of sparks when a fireball defeats an enemy.
public partial class EnemyHitEffect : Node2D
{
    private float _age = 0.0f;
    private const float LifeTime = 0.45f;

    public override void _Process(double delta)
    {
        _age += (float)delta;
        float progress = _age / LifeTime;

        RotationDegrees += 540.0f * (float)delta;
        Scale = Vector2.One * (1.0f + progress * 1.8f);
        Modulate = new Color(1, 1, 1, 1.0f - progress);

        if (_age >= LifeTime)
        {
            QueueFree();
        }
    }
}
