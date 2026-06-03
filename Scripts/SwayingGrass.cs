using Godot;

// A small grass patch that wiggles when the player walks through it.
public partial class SwayingGrass : Area2D
{
    private Node2D _blades;
    private float _swayTime = 0.0f;
    private float _swayStrength = 0.0f;

    public override void _Ready()
    {
        _blades = GetNode<Node2D>("Blades");
        BodyEntered += OnBodyEntered;
    }

    public override void _Process(double delta)
    {
        if (_swayStrength <= 0.0f)
        {
            _blades.RotationDegrees = Mathf.MoveToward(_blades.RotationDegrees, 0.0f, 120.0f * (float)delta);
            return;
        }

        _swayTime += (float)delta * 18.0f;
        _swayStrength = Mathf.MoveToward(_swayStrength, 0.0f, 2.8f * (float)delta);
        _blades.RotationDegrees = Mathf.Sin(_swayTime) * 14.0f * _swayStrength;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            _swayStrength = 1.0f;
        }
    }
}
