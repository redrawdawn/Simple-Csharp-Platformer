using Godot;

// Gives the player a forcefield shield that blocks three hits.
public partial class ShieldPowerup : Area2D
{
    private bool _collected = false;

    public override void _Ready()
    {
        AddToGroup("shield_powerup");
        Monitoring = true;
        Monitorable = true;
        BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_collected)
        {
            return;
        }

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
        GameManager.Find(this)?.UnlockShield();
        SoundManager.Play(this, "res://Audio/powerup.wav");
        Visible = false;
        Monitoring = false;
        Monitorable = false;
    }

    public void ResetPickup()
    {
        _collected = false;
        Visible = true;
        Monitoring = true;
        Monitorable = true;
    }
}
