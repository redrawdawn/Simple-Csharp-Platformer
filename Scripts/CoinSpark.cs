using Godot;

// A very small effect that appears when a coin is collected.
// It grows, fades out, floats upward, then deletes itself.
public partial class CoinSpark : Node2D
{
    private float _age = 0.0f;
    private const float LifeTime = 0.35f;

    public override void _Process(double delta)
    {
        _age += (float)delta;

        float progress = _age / LifeTime;

        Scale = Vector2.One * (1.0f + progress);
        Position += new Vector2(0, -55.0f * (float)delta);
        Modulate = new Color(1, 1, 1, 1.0f - progress);

        if (_age >= LifeTime)
        {
            QueueFree();
        }
    }
}
