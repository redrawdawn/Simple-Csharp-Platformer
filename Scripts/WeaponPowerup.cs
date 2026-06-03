using Godot;

// Gives the player the ability to shoot fireballs.
public partial class WeaponPowerup : Area2D
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
        GameManager.Find(this)?.UnlockFireballs();
        SoundManager.Play(this, "res://Audio/powerup.wav");
        QueueFree();
    }
}
