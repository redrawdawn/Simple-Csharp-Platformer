using Godot;

// A bigger effect that plays once when every coin has been collected.
public partial class AllCoinsCelebration : Node2D
{
    private float _age = 0.0f;
    private const float LifeTime = 2.2f;

    public override void _Process(double delta)
    {
        _age += (float)delta;
        float progress = _age / LifeTime;

        RotationDegrees += 80.0f * (float)delta;
        Scale = Vector2.One * (1.0f + progress * 1.4f);

        if (progress > 0.55f)
        {
            Modulate = new Color(1, 1, 1, 1.0f - ((progress - 0.55f) / 0.45f));
        }

        if (_age >= LifeTime)
        {
            QueueFree();
        }
    }
}
