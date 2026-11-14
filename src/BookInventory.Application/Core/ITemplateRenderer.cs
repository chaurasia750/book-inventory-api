namespace BookInventory.Application.Core;

/// <summary>Fills the <c>{{ variables }}</c> of a template text. A port, so the use cases never depend on the template engine.</summary>
public interface ITemplateRenderer
{
    /// <summary>Throws <see cref="TemplateRenderingException"/> when the text is not a valid template.</summary>
    Task<string> RenderAsync(string template, IReadOnlyDictionary<string, object?> model, CancellationToken cancellationToken = default);
}

/// <summary>A template text that could not be parsed or rendered; the message lists the problems, never the variable values.</summary>
public sealed class TemplateRenderingException(string message) : Exception(message);
