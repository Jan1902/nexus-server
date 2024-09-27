namespace Nexus.Framework.CodeGeneration.Shared;

/// <summary>
/// Represents a code template that can be rendered with variable replacements.
/// </summary>
public class CodeTemplate
{
    private readonly string _template;
    private readonly Dictionary<string, string> _replacements = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeTemplate"/> class.
    /// </summary>
    /// <param name="templateName">The name of the template file.</param>
    /// <param name="section">The optional section name within the template file.</param>
    public CodeTemplate(string templateName, string? section = null)
    {
        Stream? stream = null;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!assembly.FullName.StartsWith("Nexus"))
                continue;

            stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Templates.{templateName}.ct");
            if (stream is not null)
                break;
        }

        if (stream is null)
            throw new ArgumentException($"The template file for template '{templateName}' does not exist.");

        var content = new StreamReader(stream).ReadToEnd();
        if (section is not null)
        {
            var start = content.IndexOf($"#{section}", StringComparison.Ordinal);
            if (start == -1)
                throw new ArgumentException($"The section '{section}' does not exist in the code template.");

            start = content.IndexOf('\n', start) + 1;

            var end = content.IndexOf('#', start);
            if (end == -1)
                end = content.Length - 1;
            else
                end -= 1;

            _template = content.Substring(start, end - start + 1).Trim();
        }
        else
        {
            _template = content.Trim();
        }
    }

    /// <summary>
    /// Gets a code template by the template file name and optional section name.
    /// </summary>
    /// <param name="templateFileName">The name of the template file.</param>
    /// <param name="section">The optional section name within the template file.</param>
    /// <returns>The code template.</returns>
    public static CodeTemplate GetTemplate(string templateFileName, string? section = null) => new(templateFileName, section);

    /// <summary>
    /// Sets the value of a variable in the code template.
    /// </summary>
    /// <param name="variable">The name of the variable.</param>
    /// <param name="value">The value to set.</param>
    public CodeTemplate Set(string variable, string value)
    {
        if (!_template.Contains($"{{{{{variable}}}}}"))
            throw new ArgumentException($"The variable '{variable}' does not exist in the code template.");

        _replacements[variable] = value;

        return this;
    }

    /// <summary>
    /// Renders the code template with the variable replacements applied.
    /// </summary>
    /// <returns>The rendered code template.</returns>
    public string Render()
    {
        var result = _template;

        foreach (var replacement in _replacements.ToArray())
            result = result.Replace($"{{{{{replacement.Key}}}}}", replacement.Value);

        if (result.Contains("{{"))
            throw new InvalidOperationException("Not all variables have been replaced in the code template.");

        return result;
    }
}
