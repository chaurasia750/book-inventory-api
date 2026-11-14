using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.Application.Core;
using Scriban;
using Scriban.Runtime;

namespace BookInventory.Infrastructure.Core;

/// <summary>
/// Renders the <c>{{ variables }}</c> of a template with Scriban (spec core N3). The templates live in the database
/// and can be edited by an operator, so the engine is kept small on purpose: the model is a plain dictionary, there is
/// no template loader (an <c>include</c> cannot read a file), and a parse error is reported as a
/// <see cref="TemplateRenderingException"/> listing the problems - never the values.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ITemplateRenderer))]
public sealed class ScribanTemplateRenderer : ITemplateRenderer
{
    public async Task<string> RenderAsync(
        string template,
        IReadOnlyDictionary<string, object?> model,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(template))
            return string.Empty;

        var parsed = Template.Parse(template);
        if (parsed.HasErrors)
            throw new TemplateRenderingException(string.Join("; ", parsed.Messages.Select(message => message.Message)));

        var variables = new ScriptObject();
        foreach (var (key, value) in model)
            variables.SetValue(key, value, readOnly: true);

        var context = new TemplateContext { StrictVariables = false, CancellationToken = cancellationToken };
        context.PushGlobal(variables);

        return await parsed.RenderAsync(context);
    }
}
