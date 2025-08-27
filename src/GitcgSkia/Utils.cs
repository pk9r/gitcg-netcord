namespace GitcgSkia;

public static class Utils
{
    public const int NumberRoleCards = 3;
    public const int NumberActionCards = 30;
    
    public const string ArcaneBorderCacheKey = "arcane_border";
    
    public static int CalcRoleCardsWidth(int roleCardWidth, int roleCardSpacing)
        => roleCardWidth * NumberRoleCards + roleCardSpacing * (NumberRoleCards - 1);
}