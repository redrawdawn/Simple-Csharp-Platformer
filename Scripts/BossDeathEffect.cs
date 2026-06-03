using Godot;

// A larger burst when the boss is defeated.
public partial class BossDeathEffect : Node2D
{
    private float _age = 0.0f;
    private const float LifeTime = 1.1f;

    public override void _Process(double delta)
    {
        _age += (float)delta;
        float progress = _age / LifeTime;

        RotationDegrees += 220.0f * (float)delta;
        Scale = Vector2.One * (1.0f + progress * 2.8f);
        Modulate = new Color(1.0f, 0.45f, 0.25f, 1.0f - progress);

        if (_age >= LifeTime)
        {
            QueueFree();
        }
    }
}
