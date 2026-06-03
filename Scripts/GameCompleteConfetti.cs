using Godot;

// Falling confetti for the home area after the boss level is completed.
public partial class GameCompleteConfetti : Node2D
{
    private const int PieceCount = 90;
    private const float LifeTime = 13.0f;

    private readonly Polygon2D[] _pieces = new Polygon2D[PieceCount];
    private readonly Vector2[] _velocities = new Vector2[PieceCount];
    private readonly float[] _spinSpeeds = new float[PieceCount];
    private float _age = 0.0f;

    public override void _Ready()
    {
        Color[] colors =
        {
            new Color(1.0f, 0.18f, 0.18f, 1.0f),
            new Color(1.0f, 0.85f, 0.1f, 1.0f),
            new Color(0.18f, 0.8f, 1.0f, 1.0f),
            new Color(0.35f, 1.0f, 0.35f, 1.0f),
            new Color(0.9f, 0.35f, 1.0f, 1.0f)
        };

        for (int i = 0; i < PieceCount; i++)
        {
            Polygon2D piece = new Polygon2D();
            float width = (float)GD.RandRange(6.0, 12.0);
            float height = (float)GD.RandRange(10.0, 18.0);
            piece.Polygon = new Vector2[]
            {
                new Vector2(-width, -height),
                new Vector2(width, -height),
                new Vector2(width, height),
                new Vector2(-width, height)
            };

            piece.Color = colors[i % colors.Length];
            piece.Position = new Vector2((float)GD.RandRange(-80.0, 1120.0), (float)GD.RandRange(-380.0, -20.0));
            AddChild(piece);

            _pieces[i] = piece;
            _velocities[i] = new Vector2((float)GD.RandRange(-45.0, 45.0), (float)GD.RandRange(95.0, 190.0));
            _spinSpeeds[i] = (float)GD.RandRange(-260.0, 260.0);
        }
    }

    public override void _Process(double delta)
    {
        float seconds = (float)delta;
        _age += seconds;

        for (int i = 0; i < PieceCount; i++)
        {
            Polygon2D piece = _pieces[i];
            piece.Position += _velocities[i] * seconds;
            piece.RotationDegrees += _spinSpeeds[i] * seconds;
        }

        if (_age >= LifeTime)
        {
            QueueFree();
        }
    }
}
