using Godot;

// A simple menu for jumping straight to a level while testing.
public partial class LevelSelect : Control
{
    public override void _Ready()
    {
        UpdateButtonText();

        GetNode<Button>("Panel/StartButton").Pressed += () => GoTo("res://Scenes/Start.tscn", resetPowers: true);
        GetNode<Button>("Panel/Level1Button").Pressed += () => GoTo("res://Scenes/Main.tscn");
        GetNode<Button>("Panel/Level2Button").Pressed += () => GoTo("res://Scenes/Level2.tscn", givePowers: true);
        GetNode<Button>("Panel/Level3Button").Pressed += () => GoTo("res://Scenes/Level3.tscn", givePowers: true);
        GetNode<Button>("Panel/Level4Button").Pressed += () => GoTo("res://Scenes/Level4.tscn", givePowers: true);
        GetNode<Button>("Panel/Level5Button").Pressed += () => GoTo("res://Scenes/Level5.tscn", givePowers: true);
    }

    private void GoTo(string scenePath, bool givePowers = false, bool resetPowers = false)
    {
        if (resetPowers)
        {
            GameState.HasFireballs = false;
            GameState.HasGlider = false;
            GameState.HasSpeedBoost = false;
        }

        if (givePowers)
        {
            GameState.HasFireballs = true;
            GameState.HasGlider = true;
            GameState.HasSpeedBoost = true;
        }

        GetTree().ChangeSceneToFile(scenePath);
    }

    private void UpdateButtonText()
    {
        GetNode<Button>("Panel/Level1Button").Text = "Level 1";

        for (int i = 2; i <= 5; i++)
        {
            Button button = GetNode<Button>($"Panel/Level{i}Button");
            bool levelCompleted = GameState.CompletedLevels[i - 1];
            bool previousLevelCompleted = GameState.CompletedLevels[i - 2];
            bool showLock = !levelCompleted && !previousLevelCompleted;
            button.Text = showLock ? $"[LOCK] Level {i}" : $"Level {i}";
        }
    }
}
