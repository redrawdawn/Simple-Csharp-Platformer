using Godot;

// New Level 2 mechanic: bounce pads launch the player upward.
public partial class BouncePad : Area2D
{
    [Export] public float BounceVelocity = -760.0f;

    private Node2D _top;
    private float _squishTime = 0.0f;
    private float _bounceCooldown = 0.0f;

    public override void _Ready()
    {
        Monitoring = true;
        Monitorable = true;
        _top = GetNode<Node2D>("Top");
        BodyEntered += OnBodyEntered;
    }

    public override void _Process(double delta)
    {
        _bounceCooldown = Mathf.Max(0.0f, _bounceCooldown - (float)delta);

        if (_squishTime > 0.0f)
        {
            _squishTime -= (float)delta;
            _top.Scale = new Vector2(1.2f, 0.65f);
        }
        else
        {
            _top.Scale = _top.Scale.Lerp(Vector2.One, 12.0f * (float)delta);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (Node2D body in GetOverlappingBodies())
        {
            TryBounce(body);
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        TryBounce(body);
    }

    private void TryBounce(Node2D body)
    {
        if (_bounceCooldown > 0.0f || body is not Player player)
        {
            return;
        }

        _bounceCooldown = 0.2f;
        player.Bounce(BounceVelocity);
        SoundManager.Play(this, "res://Audio/bounce.wav");
        _squishTime = 0.12f;
    }
}
