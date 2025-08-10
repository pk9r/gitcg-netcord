using HoyolabHttpClient.Models.Interfaces;

namespace GitcgNetCord.MainApp.Models;

public record DecodeResult(
    bool IsValid,
    IDeckData Deck = null!
);
