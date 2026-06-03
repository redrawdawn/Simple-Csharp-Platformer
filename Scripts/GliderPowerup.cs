using Godot;

// Gives the player a simple glider.
// After collecting it, hold jump while falling to float down slowly.
public partial class GliderPowerup : Area2D
{
    private bool _collected = false;

    public override void _Ready()
    {
        Monitoring = true;
        Monitorable = true;
        BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (Node2D body in GetOverlappingBodies())
        {
            TryCollect(body);
        }

        Player player = GameManager.Find(this)?.GetPlayer();
        if (player != null && GlobalPosition.DistanceTo(player.GlobalPosition) < 64.0f)
        {
            TryCollect(player);
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        TryCollect(body);
    }

    private void TryCollect(Node2D body)
    {
        if (_collected || body is not Player)
        {
            return;
        }

        _collected = true;
        GameManager.Find(this)?.UnlockGlider();
        SoundManager.Play(this, "res://Audio/powerup.wav");
        QueueFree();
    }
}
