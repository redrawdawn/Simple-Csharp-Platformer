using Godot;

// A simple pushable box.
// The player tells the crate to move when they bump into it from the side.
public partial class Crate : CharacterBody2D
{
    [Export] public float Gravity = 1200.0f;
    [Export] public float PushSpeed = 150.0f;
    [Export] public float Friction = 900.0f;

    public void Push(float direction)
    {
        // direction is -1 for left, +1 for right.
        Velocity = new Vector2(direction * PushSpeed, Velocity.Y);
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        if (!IsOnFloor())
        {
            velocity.Y += Gravity * (float)delta;
        }

        // Slowly stop sliding when the player is not pushing the crate.
        velocity.X = Mathf.MoveToward(velocity.X, 0.0f, Friction * (float)delta);

        Velocity = velocity;
        MoveAndSlide();
    }
}
