using Godot;

// The player is a CharacterBody2D because Godot gives this node helper methods
// for platformer-style movement and collision.
public partial class Player : CharacterBody2D
{
    // Exported values appear in Godot's Inspector, so you can tweak them later
    // without changing code.
    [Export] public float Speed = 220.0f;
    [Export] public float JumpVelocity = -560.0f;
    [Export] public float Gravity = 1200.0f;
    [Export] public float FlipSpeed = 540.0f;
    [Export] public float CheatSpeed = 520.0f;
    [Export] public float CheatFlySpeed = 420.0f;

    public bool HasFireballs = false;
    public bool HasGlider = false;
    public bool HasSpeedBoost = false;
    public bool HasShield = false;
    public bool IsOnIce = false;

    private int _shieldLayers = 0;
    private float _damageCooldown = 0.0f;
    private bool _isFlipping = false;
    private bool _cheatMode = false;
    private int _slashPresses = 0;
    private float _flipDegrees = 0.0f;
    private float _facingDirection = 1.0f;
    private float _shootCooldown = 0.0f;
    private PackedScene _fireballScene = GD.Load<PackedScene>("res://Scenes/Fireball.tscn");
    private Node2D _fireGun;
    private Node2D _gliderFolded;
    private Node2D _gliderOpen;
    private Node2D _hats;
    private Node2D _forceField;
    private PackedScene _shieldBreakEffectScene = GD.Load<PackedScene>("res://Scenes/ShieldBreakEffect.tscn");

    public override void _Ready()
    {
        _fireGun = GetNode<Node2D>("FireGun");
        _gliderFolded = GetNode<Node2D>("GliderFolded");
        _gliderOpen = GetNode<Node2D>("GliderOpen");
        _hats = GetNode<Node2D>("Hats");
        _forceField = GetNode<Node2D>("ForceField");

        _fireGun.Visible = false;
        _gliderFolded.Visible = false;
        _gliderOpen.Visible = false;
        _forceField.Visible = false;
        ApplyEquippedHat();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
        {
            return;
        }

        if (keyEvent.Keycode != Key.Slash && keyEvent.PhysicalKeycode != Key.Slash)
        {
            return;
        }

        _slashPresses += 1;

        if (_slashPresses < 3)
        {
            return;
        }

        _slashPresses = 0;
        _cheatMode = !_cheatMode;
        Velocity = Vector2.Zero;
        RotationDegrees = 0.0f;
        _isFlipping = false;
        _flipDegrees = 0.0f;

        string message = _cheatMode ? "Cheat mode on: Space up, Shift down" : "Cheat mode off";
        GameManager.Find(this)?.ShowCheatMessage(message);
    }

    public void UnlockFireballs()
    {
        HasFireballs = true;
        _fireGun.Visible = true;
    }

    public void UnlockGlider()
    {
        HasGlider = true;
        _gliderFolded.Visible = true;
    }

    public void UnlockSpeedBoost()
    {
        HasSpeedBoost = true;
        Speed = 285.0f;
        JumpVelocity = -610.0f;
    }

    public void UnlockShield()
    {
        HasShield = true;
        _shieldLayers = 3;
        UpdateShieldVisual();
    }

    public void ClearShield()
    {
        HasShield = false;
        _shieldLayers = 0;
        UpdateShieldVisual();
    }

    public bool TakeDamage()
    {
        if (_damageCooldown > 0.0f)
        {
            return true;
        }

        _damageCooldown = 0.6f;

        if (_shieldLayers > 0)
        {
            _shieldLayers--;
            HasShield = _shieldLayers > 0;
            SpawnShieldBreakEffect();
            UpdateShieldVisual();
            return true;
        }

        return false;
    }

    public void ApplyEquippedHat()
    {
        // Hide every hat first, then show only the equipped one.
        foreach (Node child in _hats.GetChildren())
        {
            if (child is Node2D hat)
            {
                hat.Visible = false;
            }
        }

        if (GameState.EquippedHat < 0)
        {
            return;
        }

        Node2D equippedHat = _hats.GetNodeOrNull<Node2D>($"Hat{GameState.EquippedHat + 1}");
        if (equippedHat != null)
        {
            equippedHat.Visible = true;
        }
    }

    public void ResetPlayerState()
    {
        // This is used when the player respawns.
        Velocity = Vector2.Zero;
        RotationDegrees = 0.0f;
        _isFlipping = false;
        _flipDegrees = 0.0f;
    }

    public void Bounce(float bounceVelocity)
    {
        // Bounce pads call this to launch the player upward.
        Velocity = new Vector2(Velocity.X, bounceVelocity);
        _isFlipping = true;
        _flipDegrees = 0.0f;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Velocity is the built-in movement vector for CharacterBody2D.
        Vector2 velocity = Velocity;
        _shootCooldown = Mathf.Max(0.0f, _shootCooldown - (float)delta);
        _damageCooldown = Mathf.Max(0.0f, _damageCooldown - (float)delta);

        if (_cheatMode)
        {
            MoveWithCheats();
            return;
        }

        // Apply gravity when the player is in the air.
        if (!IsOnFloor())
        {
            velocity.Y += Gravity * (float)delta;

            bool isUsingGlider = HasGlider && Input.IsActionPressed("jump") && velocity.Y > 120.0f;

            if (isUsingGlider)
            {
                // The glider slows falling while the jump button is held.
                velocity.Y = 120.0f;
            }

            _gliderOpen.Visible = isUsingGlider;
            _gliderFolded.Visible = HasGlider && !isUsingGlider;
        }
        else
        {
            _gliderOpen.Visible = false;
            _gliderFolded.Visible = HasGlider;
        }

        if (_isFlipping)
        {
            // Rotate until we have made one full 360 degree spin.
            _flipDegrees += FlipSpeed * (float)delta;
            RotationDegrees = _flipDegrees;

            if (_flipDegrees >= 360.0f)
            {
                _isFlipping = false;
                _flipDegrees = 0.0f;
                RotationDegrees = 0.0f;
            }
        }
        else if (IsOnFloor())
        {
            // Stay upright while standing still or running on the ground.
            RotationDegrees = 0.0f;
        }

        // Jump only when standing on the ground.
        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
            SoundManager.Play(this, "res://Audio/jump.wav");

            // Start exactly one frontflip when the jump begins.
            _isFlipping = true;
            _flipDegrees = 0.0f;
        }

        // Input.GetAxis returns:
        // -1 when pressing left, +1 when pressing right, and 0 when neither.
        float direction = Input.GetAxis("move_left", "move_right");
        float currentSpeed = Speed;

        if (IsOnIce && IsOnFloor())
        {
            // Ice makes movement slippery: slower to speed up and slower to stop.
            float targetSpeed = direction * currentSpeed;
            float acceleration = direction == 0.0f ? 90.0f : 260.0f;
            velocity.X = Mathf.MoveToward(velocity.X, targetSpeed, acceleration * (float)delta);
        }
        else
        {
            velocity.X = direction * currentSpeed;
        }

        if (direction != 0.0f)
        {
            _facingDirection = Mathf.Sign(direction);
            _fireGun.Scale = new Vector2(_facingDirection, 1.0f);
        }

        if (HasFireballs && Input.IsActionJustPressed("shoot") && _shootCooldown <= 0.0f)
        {
            ShootFireball();
            _shootCooldown = 0.35f;
        }

        Velocity = velocity;

        // MoveAndSlide moves the player and handles sliding along floors/walls.
        MoveAndSlide();

        // If we bump into a crate from the side, push it.
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            KinematicCollision2D collision = GetSlideCollision(i);

            // Only push when the collision is mostly from the side.
            // This prevents the crate from sliding when the player stands on top.
            bool hitSideOfCrate = Mathf.Abs(collision.GetNormal().X) > 0.7f;

            if (collision.GetCollider() is Crate crate && hitSideOfCrate && Mathf.Abs(velocity.X) > 0.0f)
            {
                crate.Push(Mathf.Sign(velocity.X));
            }
        }
    }

    private void ShootFireball()
    {
        Fireball fireball = _fireballScene.Instantiate<Fireball>();
        fireball.GlobalPosition = GlobalPosition + new Vector2(_facingDirection * 28.0f, -6.0f);
        fireball.Direction = _facingDirection;
        GetTree().CurrentScene.AddChild(fireball);
        SoundManager.Play(this, "res://Audio/fireball.wav", -8.0f);
    }

    private void MoveWithCheats()
    {
        float horizontal = Input.GetAxis("move_left", "move_right");
        float vertical = 0.0f;

        if (Input.IsKeyPressed(Key.Space))
        {
            vertical -= 1.0f;
        }

        if (Input.IsKeyPressed(Key.Shift))
        {
            vertical += 1.0f;
        }

        if (horizontal != 0.0f)
        {
            _facingDirection = Mathf.Sign(horizontal);
            _fireGun.Scale = new Vector2(_facingDirection, 1.0f);
        }

        _gliderOpen.Visible = false;
        _gliderFolded.Visible = HasGlider;
        UpdateShieldVisual();
        RotationDegrees = 0.0f;
        Velocity = new Vector2(horizontal * CheatSpeed, vertical * CheatFlySpeed);
        MoveAndSlide();
    }

    private void UpdateShieldVisual()
    {
        _forceField.Visible = _shieldLayers > 0;

        for (int i = 1; i <= 3; i++)
        {
            Polygon2D layer = _forceField.GetNodeOrNull<Polygon2D>($"ShieldLayer{i}");
            if (layer != null)
            {
                layer.Visible = i <= _shieldLayers;
            }
        }
    }

    private void SpawnShieldBreakEffect()
    {
        SoundManager.Play(this, "res://Audio/shield_break.wav", -4.0f);
        Node2D effect = _shieldBreakEffectScene.Instantiate<Node2D>();
        effect.GlobalPosition = GlobalPosition;
        GetTree().CurrentScene.AddChild(effect);
    }
}
