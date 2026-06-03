using Godot;

// The level exit. Touch it to show the next-level and return-home buttons.
public partial class LevelPortal : Area2D
{
    private PackedScene _burstScene = GD.Load<PackedScene>("res://Scenes/PortalBurst.tscn");

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            Node2D burst = _burstScene.Instantiate<Node2D>();
            burst.GlobalPosition = GlobalPosition;
            GetTree().CurrentScene.AddChild(burst);

            GameManager.Find(this)?.ReachPortal();
        }
    }
}
