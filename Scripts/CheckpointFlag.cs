using Godot;

// Sets the player's respawn point when touched.
public partial class CheckpointFlag : Area2D
{
    private bool _used = false;

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
            TrySetCheckpoint(body);
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        TrySetCheckpoint(body);
    }

    private void TrySetCheckpoint(Node2D body)
    {
        if (_used || body is not Player)
        {
            return;
        }

        _used = true;
        GameManager.Find(this)?.SetCheckpoint(GlobalPosition + new Vector2(0, 40));
        GetNode<Polygon2D>("Flag").Color = new Color(0.35f, 1.0f, 0.45f);
    }
}
