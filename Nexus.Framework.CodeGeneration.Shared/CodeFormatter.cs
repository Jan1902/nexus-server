using System.Text;

namespace Nexus.Framework.CodeGeneration.Shared;

public static class CodeFormatter
{
    public static string FormatCode(string code)
    {
        var lines = code.Split('\n').Select(s => s.Trim());

        var strBuilder = new StringBuilder();

        var indentCount = 0;
        var shouldIndent = false;

        foreach (var line in lines)
        {
            if (shouldIndent)
                indentCount++;

            if (line.Trim() == "}")
                indentCount--;

            if (indentCount == 0)
            {
                strBuilder.AppendLine(line);
                shouldIndent = line.Contains("{");

                continue;
            }

            var blankSpace = string.Empty;
            for (var i = 0; i < indentCount; i++)
            {
                blankSpace += "    ";
            }

            if (line.Contains("}") && line.Trim() != "}")
                indentCount--;

            strBuilder.AppendLine(blankSpace + line);
            shouldIndent = line.Contains("{");
        }

        return strBuilder.ToString();
    }
}
