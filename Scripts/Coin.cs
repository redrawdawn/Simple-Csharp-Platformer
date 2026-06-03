using Godot;

// The coin is an Area2D. Area2D nodes detect overlaps but do not physically
// block the player.
public partial class Coin : Area2D
{
    private PackedScene _sparkScene = GD.Load<PackedScene>("res://Scenes/CoinSpark.tscn");
    private bool _collected = false;

    public override void _Ready()
    {
        Monitoring = true;
        Monitorable = true;
        BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        // This backup check makes coin pickup more reliable if the player is
        // already touching the coin before the BodyEntered signal fires.
        foreach (Node2D body in GetOverlappingBodies())
        {
            TryCollect(body);
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        TryCollect(body);
    }

    private void TryCollect(Node2D body)
    {
        // Only the player should collect the coin.
        if (_collected || body is not Player)
        {
            return;
        }

        _collected = true;

        // Ask the main scene to increase the coin count.
        GameManager gameManager = GameManager.Find(this);
        gameManager?.AddCoin();
        SoundManager.Play(this, "res://Audio/coin.wav");

        // Make a small sparkle effect at the coin's position.
        Node2D spark = _sparkScene.Instantiate<Node2D>();
        spark.GlobalPosition = GlobalPosition;
        GetTree().CurrentScene.AddChild(spark);

        // Remove this coin from the game.
        QueueFree();
    }
}
