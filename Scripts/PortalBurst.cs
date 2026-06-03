using Godot;

// A short burst effect that plays when the player touches the level portal.
public partial class PortalBurst : Node2D
{
    private float _age = 0.0f;
    private const float LifeTime = 0.75f;

    public override void _Process(double delta)
    {
        _age += (float)delta;
        float progress = _age / LifeTime;

        Scale = Vector2.One * (1.0f + progress * 2.2f);
        RotationDegrees += 240.0f * (float)delta;
        Modulate = new Color(1, 1, 1, 1.0f - progress);

        if (_age >= LifeTime)
        {
            QueueFree();
        }
    }
}
