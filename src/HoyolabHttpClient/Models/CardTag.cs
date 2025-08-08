namespace HoyolabHttpClient.Models;

public class CardTag
{
    public static CardTag ArcaneLegend { get; } = new CardTag("7");

    private readonly string _value;

    public string Value => _value;

    private CardTag(string value)
    {
        _value = value;
    }

    public override string ToString()
    {
        return Value;
    }
}
