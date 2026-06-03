using Godot;

// This script sits on Main.tscn and keeps track of simple game-wide state.
public partial class GameManager : Node2D
{
    [Export] public int TotalCoins = 24;
    [Export] public int LevelNumber = 0;
    [Export] public string NextScenePath = "";
    [Export] public string NextLevelButtonText = "Next Level";
    [Export] public string LevelSelectScenePath = "";
    [Export] public string HomeScenePath = "res://Scenes/Start.tscn";
    [Export] public string BackgroundMusicPath = "res://Audio/ambient_music.wav";
    [Export] public string BossMusicPath = "res://Audio/boss_music.wav";
    [Export] public bool ShowReturnHomeButton = true;
    [Export] public bool ShowTotalCoinBank = false;
    [Export] public bool ShowButtonsWhenAllCoinsCollected = false;
    [Export] public bool ResetPowerUpsOnStart = false;
    [Export] public bool ShowGameCompleteConfetti = false;
    [Export] public float BossMusicStartX = -1.0f;

    private Player _player;
    private Label _coinLabel;
    private Label _itemPopupLabel;
    private Label _totalCoinLabel;
    private Button _nextLevelButton;
    private Button _levelSelectButton;
    private Button _returnHomeButton;
    private AudioStreamPlayer _musicPlayer;
    private Vector2 _spawnPosition;
    private int _coins = 0;
    private float _itemPopupTime = 0.0f;
    private bool _showedAllCoinsCelebration = false;
    private bool _reachedPortal = false;
    private bool _isRespawning = false;
    private bool _bossMusicPlaying = false;
    private float _respawnTimer = 0.0f;
    private const float FallRespawnY = 700.0f;
    private PackedScene _allCoinsCelebrationScene = GD.Load<PackedScene>("res://Scenes/AllCoinsCelebration.tscn");
    private PackedScene _gameCompleteConfettiScene = GD.Load<PackedScene>("res://Scenes/GameCompleteConfetti.tscn");

    public static GameManager Find(Node source)
    {
        if (source.GetTree()?.CurrentScene is GameManager currentManager)
        {
            return currentManager;
        }

        return source.GetTree()?.GetFirstNodeInGroup("game_manager") as GameManager;
    }

    public Player GetPlayer()
    {
        return _player;
    }

    public override void _Ready()
    {
        AddToGroup("game_manager");

        // GetNode finds nodes by their scene-tree path.
        _player = GetNode<Player>("Player");
        _coinLabel = GetNode<Label>("UI/CoinLabel");
        _itemPopupLabel = GetNode<Label>("UI/ItemPopup");
        _totalCoinLabel = GetNodeOrNull<Label>("UI/TotalCoinLabel");
        _nextLevelButton = GetNodeOrNull<Button>("UI/NextLevelButton");
        _levelSelectButton = GetNodeOrNull<Button>("UI/LevelSelectButton");
        _returnHomeButton = GetNodeOrNull<Button>("UI/ReturnHomeButton");

        StartBackgroundMusic();

        if (ResetPowerUpsOnStart)
        {
            GameState.HasFireballs = false;
            GameState.HasGlider = false;
            GameState.HasSpeedBoost = false;
            GameState.HasShield = false;
        }

        if (_nextLevelButton != null)
        {
            _nextLevelButton.Visible = false;
            _nextLevelButton.Text = NextLevelButtonText;
            _nextLevelButton.Pressed += OnNextLevelButtonPressed;
        }

        if (_levelSelectButton != null)
        {
            _levelSelectButton.Visible = false;
            _levelSelectButton.Pressed += OnLevelSelectButtonPressed;
        }

        SetupReturnHomeButton();

        // Remember where the player started, so we can respawn there later.
        _spawnPosition = _player.GlobalPosition;
        ApplySavedPowerUps();
        UpdateCoinLabel();
        _itemPopupLabel.Text = "";
        ShowGameCompleteConfettiIfNeeded();
    }

    public override void _Process(double delta)
    {
        // If the player falls below the visible level, bring them back.
        if (_player.GlobalPosition.Y > FallRespawnY)
        {
            RespawnPlayer();
        }

        if (_isRespawning)
        {
            _respawnTimer -= (float)delta;

            if (_respawnTimer <= 0.0f)
            {
                FinishRespawn();
            }
        }

        if (BossMusicStartX >= 0.0f && !_bossMusicPlaying && _player.GlobalPosition.X >= BossMusicStartX && IsBossAlive())
        {
            StartBossMusic();
        }

        if (_itemPopupTime > 0.0f)
        {
            _itemPopupTime -= (float)delta;
            _itemPopupLabel.Modulate = new Color(1, 1, 1, Mathf.Min(_itemPopupTime, 1.0f));
        }
    }

    public void AddCoin()
    {
        _coins += 1;
        GameState.TotalCoinsCollected += 1;
        UpdateCoinLabel();

        if (_coins >= TotalCoins && !_showedAllCoinsCelebration)
        {
            _showedAllCoinsCelebration = true;
            ShowAllCoinsCelebration();

            if (ShowButtonsWhenAllCoinsCollected)
            {
                ShowLevelCompleteButtons();
            }
        }
    }

    public void ReachPortal()
    {
        if (_reachedPortal)
        {
            return;
        }

        _reachedPortal = true;
        MarkCurrentLevelComplete();
        SoundManager.Play(this, "res://Audio/portal.wav");
        ShowLevelCompleteButtons();
    }

    public void RespawnPlayer()
    {
        if (_isRespawning)
        {
            return;
        }

        SoundManager.Play(this, "res://Audio/death.wav");

        _isRespawning = true;
        _respawnTimer = 2.0f;
        _player.ClearShield();
        ResetShieldPickups();
        _player.Visible = false;
        _player.SetPhysicsProcess(false);
    }

    public void StartBossMusic()
    {
        if (_bossMusicPlaying)
        {
            return;
        }

        _bossMusicPlaying = true;
        ChangeMusic(BossMusicPath, -7.0f);
    }

    public void StopBossMusic()
    {
        if (!_bossMusicPlaying)
        {
            return;
        }

        _bossMusicPlaying = false;
        ChangeMusic(BackgroundMusicPath, -8.0f);
    }

    private void FinishRespawn()
    {
        _isRespawning = false;

        // Put the player back at the checkpoint and stop any falling motion.
        _player.GlobalPosition = _spawnPosition;
        _player.ResetPlayerState();
        _player.Visible = true;
        _player.SetPhysicsProcess(true);
    }

    public void SetCheckpoint(Vector2 checkpointPosition)
    {
        _spawnPosition = checkpointPosition;
        ShowItemPopup("Checkpoint set");
    }

    public void UnlockFireballs()
    {
        GameState.HasFireballs = true;
        _player.UnlockFireballs();
        ShowItemPopup("Fireball acquired: press F to shoot");
    }

    public void UnlockGlider()
    {
        GameState.HasGlider = true;
        _player.UnlockGlider();
        ShowItemPopup("Glider acquired: hold jump while falling");
    }

    public void UnlockSpeedBoost()
    {
        GameState.HasSpeedBoost = true;
        _player.UnlockSpeedBoost();
        ShowItemPopup("Speed boots acquired: run and jump faster");
    }

    public void UnlockShield()
    {
        _player.UnlockShield();
        ShowItemPopup("Shield acquired: blocks 3 hits");
    }

    public void RefreshCoinDisplay()
    {
        UpdateCoinLabel();
    }

    public void ShowCheatMessage(string message)
    {
        ShowItemPopup(message);
    }

    private void UpdateCoinLabel()
    {
        if (ShowTotalCoinBank)
        {
            _coinLabel.Text = $"◆ {GameState.TotalCoinsCollected}";
        }
        else
        {
            _coinLabel.Text = $"◆ {_coins}/{TotalCoins}";
        }

        if (_totalCoinLabel != null)
        {
            _totalCoinLabel.Text = $"◆ {GameState.TotalCoinsCollected}";
        }
    }

    private void ShowItemPopup(string message)
    {
        _itemPopupLabel.Text = message;
        _itemPopupLabel.Modulate = Colors.White;
        _itemPopupTime = 3.0f;
    }

    private void ShowAllCoinsCelebration()
    {
        SoundManager.Play(this, "res://Audio/level_complete.wav");

        // Show a bigger effect slightly above the player.
        Node2D celebration = _allCoinsCelebrationScene.Instantiate<Node2D>();
        celebration.GlobalPosition = _player.GlobalPosition + new Vector2(0, -70);
        AddChild(celebration);
    }

    private void MarkCurrentLevelComplete()
    {
        if (LevelNumber < 1 || LevelNumber > GameState.CompletedLevels.Length)
        {
            return;
        }

        GameState.CompletedLevels[LevelNumber - 1] = true;
    }

    private void ShowLevelCompleteButtons()
    {
        ShowItemPopup(NextLevelButtonText);
        if (_nextLevelButton != null && NextScenePath != "")
        {
            _nextLevelButton.Visible = true;
        }

        if (_levelSelectButton != null && LevelSelectScenePath != "")
        {
            _levelSelectButton.Visible = true;
        }

        if (_returnHomeButton != null && ShowReturnHomeButton && HomeScenePath != "")
        {
            _returnHomeButton.Visible = true;
        }
    }

    private void OnNextLevelButtonPressed()
    {
        if (NextScenePath == "")
        {
            return;
        }

        MarkCurrentLevelComplete();
        GetTree().ChangeSceneToFile(NextScenePath);
    }

    private void OnLevelSelectButtonPressed()
    {
        if (LevelSelectScenePath == "")
        {
            return;
        }

        GetTree().ChangeSceneToFile(LevelSelectScenePath);
    }

    private void OnReturnHomeButtonPressed()
    {
        if (HomeScenePath == "")
        {
            return;
        }

        MarkCurrentLevelComplete();
        GetTree().ChangeSceneToFile(HomeScenePath);
    }

    private void SetupReturnHomeButton()
    {
        if (_returnHomeButton == null)
        {
            _returnHomeButton = new Button();
            _returnHomeButton.Name = "ReturnHomeButton";
            _returnHomeButton.Text = "Return Home";
            _returnHomeButton.OffsetLeft = 382.0f;
            _returnHomeButton.OffsetTop = _levelSelectButton == null ? 292.0f : 364.0f;
            _returnHomeButton.OffsetRight = 578.0f;
            _returnHomeButton.OffsetBottom = _returnHomeButton.OffsetTop + 62.0f;
            GetNode<CanvasLayer>("UI").AddChild(_returnHomeButton);
        }

        _returnHomeButton.Visible = false;
        _returnHomeButton.Pressed += OnReturnHomeButtonPressed;
    }

    private void StartBackgroundMusic()
    {
        AudioStream music = SoundManager.LoadStream(BackgroundMusicPath);

        if (music == null)
        {
            GD.PrintErr($"Could not load music file: {BackgroundMusicPath}");
            return;
        }

        _musicPlayer = new AudioStreamPlayer();
        _musicPlayer.Name = "BackgroundMusicPlayer";
        _musicPlayer.Stream = music;
        _musicPlayer.VolumeDb = -8.0f;
        _musicPlayer.Bus = "Master";
        AddChild(_musicPlayer);

        // Restart the track whenever it ends, making it act like looping music.
        _musicPlayer.Finished += () => _musicPlayer.Play();
        _musicPlayer.Play();
    }

    private void ChangeMusic(string musicPath, float volumeDb)
    {
        if (_musicPlayer == null)
        {
            return;
        }

        AudioStream music = SoundManager.LoadStream(musicPath);
        if (music == null)
        {
            GD.PrintErr($"Could not load music file: {musicPath}");
            return;
        }

        _musicPlayer.Stop();
        _musicPlayer.Stream = music;
        _musicPlayer.VolumeDb = volumeDb;
        _musicPlayer.Play();
    }

    private bool IsBossAlive()
    {
        return GetTree().CurrentScene.FindChild("BossEnemy", true, false) is BossEnemy;
    }

    private void ApplySavedPowerUps()
    {
        if (GameState.HasFireballs)
        {
            _player.UnlockFireballs();
        }

        if (GameState.HasGlider)
        {
            _player.UnlockGlider();
        }

        if (GameState.HasSpeedBoost)
        {
            _player.UnlockSpeedBoost();
        }

    }

    private void ResetShieldPickups()
    {
        foreach (Node node in GetTree().GetNodesInGroup("shield_powerup"))
        {
            if (node is ShieldPowerup shieldPowerup)
            {
                shieldPowerup.ResetPickup();
            }
        }
    }

    private void ShowGameCompleteConfettiIfNeeded()
    {
        if (!ShowGameCompleteConfetti || !GameState.CompletedGame || GameState.ShowedCompletionConfetti)
        {
            return;
        }

        GameState.ShowedCompletionConfetti = true;
        SoundManager.Play(this, "res://Audio/confetti.wav", -4.0f);
        Node2D confetti = _gameCompleteConfettiScene.Instantiate<Node2D>();
        AddChild(confetti);
    }
}
