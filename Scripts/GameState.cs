// This tiny static class remembers upgrade state between scenes.
public static class GameState
{
    public static bool HasFireballs = false;
    public static bool HasGlider = false;
    public static bool HasSpeedBoost = false;
    public static bool HasShield = false;
    public static bool CompletedGame = false;
    public static bool ShowedCompletionConfetti = false;
    public static bool[] CompletedLevels = new bool[5];

    // Coins collected across the whole game. These can be spent at home.
    public static int TotalCoinsCollected = 0;

    // There are five hats in the shop. Each slot remembers if that hat was bought.
    public static bool[] BoughtHats = new bool[5];

    // -1 means no hat is equipped. 0 through 4 means one of the shop hats is equipped.
    public static int EquippedHat = -1;
}
