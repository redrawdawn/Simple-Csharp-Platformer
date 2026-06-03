using Godot;

// Watches the boss fight. When the boss is defeated, the exit lift starts moving.
public partial class BossExitController : Node
{
    [Export] public NodePath ExitPlatformPath;

    private MovingPlatform _exitPlatform;
    private bool _activated = false;

    public override void _Ready()
    {
        _exitPlatform = GetNode<MovingPlatform>(ExitPlatformPath);
    }

    public override void _Process(double delta)
    {
        if (_activated || IsBossStillAlive(GetTree().CurrentScene))
        {
            return;
        }

        _activated = true;
        _exitPlatform.Activate();
        GameManager.Find(this)?.ShowCheatMessage("Arena clear: exit lift active");
    }

    private bool IsBossStillAlive(Node node)
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is BossEnemy)
            {
                return true;
            }

            if (IsBossStillAlive(child))
            {
                return true;
            }
        }

        return false;
    }
}
