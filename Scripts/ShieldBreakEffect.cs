using Godot;

// A quick blue pop when one shield layer breaks.
public partial class ShieldBreakEffect : Node2D
{
    private float _age = 0.0f;
    private const float LifeTime = 0.45f;

    public override void _Process(double delta)
    {
        _age += (float)delta;
        float progress = _age / LifeTime;

        RotationDegrees += 360.0f * (float)delta;
        Scale = Vector2.One * (1.0f + progress * 1.7f);
        Modulate = new Color(0.7f, 1.0f, 1.0f, 1.0f - progress);

        if (_age >= LifeTime)
        {
            QueueFree();
        }
    }
}
