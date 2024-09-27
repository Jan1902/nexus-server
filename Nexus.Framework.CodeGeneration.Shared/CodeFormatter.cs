using System.Text;

namespace Nexus.Framework.CodeGeneration.Shared;

public static class CodeFormatter
{
    public static string FormatCode(string code)
    {
        var lines = code.Split('\n').Select(s => s.Trim());

        var strBuilder = new StringBuilder();

        int indentCount = 0;
        bool shouldIndent = false;

        foreach (string line in lines)
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

            string blankSpace = string.Empty;
            for (int i = 0; i < indentCount; i++)
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
