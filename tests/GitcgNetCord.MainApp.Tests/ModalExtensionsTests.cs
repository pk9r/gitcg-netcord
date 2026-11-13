using GitcgNetCord.MainApp.Extensions;
using NetCord;
using NetCord.JsonModels;
using Xunit;

namespace GitcgNetCord.MainApp.Tests;

public class ModalExtensionsTests
{
    private static Label CreateLabelWithTextInput(string customId, string value)
    {
        var jsonTextInput = new JsonTextInputComponent
        {
            Type = ComponentType.TextInput,
            CustomId = customId,
            Value = value
        };

        var jsonLabel = new JsonLabelComponent
        {
            Type = ComponentType.Label,
            Component = jsonTextInput
        };

        return new Label(jsonLabel, null);
    }

    [Fact]
    public void GetTextInputs_FromLabelComponents_ReturnsNestedTextInputs()
    {
        // Arrange
        var label1 = CreateLabelWithTextInput("hoyolab-user-id", "12345678");
        var label2 = CreateLabelWithTextInput("token", "v2_CAISDummyToken");
        var components = new IModalComponent[] { label1, label2 };

        // Act
        var textInputs = components.GetTextInputs().ToList();

        // Assert
        Assert.Equal(2, textInputs.Count);
        Assert.Equal("hoyolab-user-id", textInputs[0].CustomId);
        Assert.Equal("12345678", textInputs[0].Value);
        Assert.Equal("token", textInputs[1].CustomId);
        Assert.Equal("v2_CAISDummyToken", textInputs[1].Value);
    }

    [Fact]
    public void GetTextInputs_SingleLabelComponent_ReturnsTextInput()
    {
        // Arrange
        var label = CreateLabelWithTextInput("uid", "800123456");
        var components = new IModalComponent[] { label };

        // Act
        var textInputs = components.GetTextInputs().ToList();

        // Assert
        Assert.Single(textInputs);
        Assert.Equal("uid", textInputs[0].CustomId);
        Assert.Equal("800123456", textInputs[0].Value);
    }

    [Fact]
    public void GetTextInputs_EmptyComponents_ReturnsEmpty()
    {
        // Arrange
        var components = Array.Empty<IModalComponent>();

        // Act
        var textInputs = components.GetTextInputs().ToList();

        // Assert
        Assert.Empty(textInputs);
    }

    [Fact]
    public void GetTextInputs_WhenComponentIsNotTextInput_IgnoresNonTextInput()
    {
        // Arrange - TextDisplay is an IModalComponent that does not contain a TextInput
        var textDisplay = new TextDisplay(new JsonTextDisplayComponent
        {
            Type = ComponentType.TextDisplay,
            Content = "Important Notice"
        });
        var labelWithValid = CreateLabelWithTextInput("valid-id", "valid-value");
        var components = new IModalComponent[] { textDisplay, labelWithValid };

        // Act
        var textInputs = components.GetTextInputs().ToList();

        // Assert
        Assert.Single(textInputs);
        Assert.Equal("valid-id", textInputs[0].CustomId);
        Assert.Equal("valid-value", textInputs[0].Value);
    }

    [Fact]
    public void GetTextInputs_ThrowsOnNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((IEnumerable<IModalComponent>)null!).GetTextInputs().ToList());
    }
}
