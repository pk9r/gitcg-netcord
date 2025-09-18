using GitcgNetCord.MainApp.Plugins;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace GitcgNetCord.MainApp.Models;

public class DeckEditorContext
{
    [JsonPropertyName("deck_model")]
    [Description("The current deck model.")]
    public DeckModel DeckModel { get; set; } = new();
}
