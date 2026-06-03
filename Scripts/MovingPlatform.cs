using Godot;

// A beginner-friendly moving platform.
// Set TravelOffset in the Inspector:
// - X moves it left/right
// - Y moves it up/down
public partial class MovingPlatform : AnimatableBody2D
{
    [Export] public Vector2 TravelOffset = new Vector2(220, 0);
    [Export] public float MoveSpeed = 1.0f;
    [Export] public bool StartsActive = true;

    private Vector2 _startPosition;
    private float _time = 0.0f;
    private bool _isActive = true;

    public override void _Ready()
    {
        _startPosition = Position;
        _isActive = StartsActive;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_isActive)
        {
            Position = _startPosition;
            return;
        }

        _time += (float)delta * MoveSpeed;

        // This moves smoothly from the start point to the end point and back.
        float moveAmount = (Mathf.Sin(_time) + 1.0f) / 2.0f;
        Position = _startPosition + TravelOffset * moveAmount;
    }

    public void Activate()
    {
        _isActive = true;
    }
}
