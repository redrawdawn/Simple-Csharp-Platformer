using Godot;

// Ice floors make the player slide while standing on them.
public partial class IcePatch : Area2D
{
    public override void _Ready()
    {
        Monitoring = true;
        Monitorable = true;
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (Node2D body in GetOverlappingBodies())
        {
            if (body is Player player)
            {
                player.IsOnIce = true;
            }
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player player)
        {
            player.IsOnIce = true;
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is Player player)
        {
            player.IsOnIce = false;
        }
    }
}
