using Godot;

// A simple home-area hat shop.
// Walk near the box to open the menu, then click a hat button to buy/equip it.
public partial class HatShop : Area2D
{
    private readonly string[] _hatNames = { "Cap", "Beanie", "Top Hat", "Wizard", "Crown" };
    private readonly int[] _hatCosts = { 15, 15, 20, 22, 40 };
    private readonly int[] _hatVisuals = { 0, 4, 1, 3, 2 };

    private Player _player;
    private GameManager _gameManager;
    private Control _shopMenu;
    private Label _coinBankLabel;
    private Button[] _hatButtons = new Button[5];

    public override void _Ready()
    {
        _gameManager = GameManager.Find(this);
        _player = _gameManager.GetPlayer();
        _shopMenu = _gameManager.GetNode<Control>("UI/HatShopMenu");
        _coinBankLabel = _gameManager.GetNode<Label>("UI/TotalCoinLabel");

        for (int i = 0; i < _hatButtons.Length; i++)
        {
            int hatIndex = i;
            _hatButtons[i] = _gameManager.GetNode<Button>($"UI/HatShopMenu/HatButton{i + 1}");
            _hatButtons[i].Pressed += () => BuyOrEquipHat(hatIndex);
        }

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
        _shopMenu.Visible = false;
        UpdateMenuText();
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            GetNode<Node2D>("ClosedLid").Visible = false;
            GetNode<Node2D>("OpenLid").Visible = true;
            _shopMenu.Visible = true;
            UpdateMenuText();
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is Player)
        {
            GetNode<Node2D>("ClosedLid").Visible = true;
            GetNode<Node2D>("OpenLid").Visible = false;
            _shopMenu.Visible = false;
        }
    }

    private void BuyOrEquipHat(int hatIndex)
    {
        _player = _gameManager.GetPlayer();

        if (!GameState.BoughtHats[hatIndex])
        {
            if (GameState.TotalCoinsCollected < _hatCosts[hatIndex])
            {
                UpdateMenuText();
                return;
            }

            GameState.TotalCoinsCollected -= _hatCosts[hatIndex];
            GameState.BoughtHats[hatIndex] = true;
            SoundManager.Play(this, hatIndex == 4 ? "res://Audio/crown_buy.wav" : "res://Audio/powerup.wav", -5.0f);
        }

        GameState.EquippedHat = _hatVisuals[hatIndex];
        if (_player != null)
        {
            _player.ApplyEquippedHat();
        }

        _gameManager.RefreshCoinDisplay();
        UpdateMenuText();
    }

    private void UpdateMenuText()
    {
        _coinBankLabel.Text = $"◆ {GameState.TotalCoinsCollected}";

        for (int i = 0; i < _hatButtons.Length; i++)
        {
            string buttonText = $"{_hatNames[i]} - ◆ {_hatCosts[i]}";

            if (GameState.BoughtHats[i])
            {
                buttonText = GameState.EquippedHat == _hatVisuals[i] ? $"{_hatNames[i]} equipped" : $"Wear {_hatNames[i]}";
            }

            _hatButtons[i].Text = buttonText;
        }
    }
}
