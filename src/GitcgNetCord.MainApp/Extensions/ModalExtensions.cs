using NetCord;
using NetCord.Services.ComponentInteractions;

namespace GitcgNetCord.MainApp.Extensions;

public static class ModalExtensions
{
    /// <summary>
    /// Extracts all <see cref="TextInput"/> instances from modal components,
    /// supporting both nested <see cref="Label"/> structures and direct <see cref="TextInput"/> components.
    /// </summary>
    public static IEnumerable<TextInput> GetTextInputs(this IEnumerable<IModalComponent> components)
    {
        ArgumentNullException.ThrowIfNull(components);

        foreach (var component in components)
        {
            if (component is Label label && label.Component is TextInput labelTextInput)
            {
                yield return labelTextInput;
            }
            else if (component is TextInput textInput)
            {
                yield return textInput;
            }
        }
    }

    /// <summary>
    /// Extracts all <see cref="TextInput"/> instances from the modal interaction context.
    /// </summary>
    public static IEnumerable<TextInput> GetTextInputs(this ModalInteractionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Components.GetTextInputs();
    }

    /// <summary>
    /// Retrieves the text input value for the specified custom ID from the modal interaction context,
    /// or <c>null</c> if not found.
    /// </summary>
    public static string? GetTextInputValue(this ModalInteractionContext context, string customId)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.GetTextInputs().FirstOrDefault(x => x.CustomId == customId)?.Value;
    }
}
