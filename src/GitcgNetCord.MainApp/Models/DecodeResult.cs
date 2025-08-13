using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using HoyolabHttpClient.Models.Interfaces;
using Microsoft.Extensions.Options;

namespace GitcgNetCord.MainApp.Models;

public record DecodeResult(
    ValidateOptionsResult Validate,
    IDeckData Deck = null!
);