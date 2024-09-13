using System.Text;

namespace Nexus.Framework.CodeGeneration.Shared;

public interface IRenderable
{
    void Render(StringBuilder builder);
}

public record Namespace(string Name) : IRenderable
{
    public void Render(StringBuilder builder) => builder.AppendLine($"namespace {Name};").AppendLine();
}

public interface ICodeElementBase : IRenderable
{
    public string Name { get; }
    public string AccessModifier { get; }
}

public record Class(string Name, string AccessModifier, string? BaseClass, List<ICodeElementBase> Content) : ICodeElementBase
{
    public void Render(StringBuilder builder)
    {
        builder.Append($"{AccessModifier} class {Name}");

        if (BaseClass is not null)
            builder.AppendLine($" : {BaseClass}");
        else
            builder.AppendLine();

        builder.AppendLine("{");

        foreach (var element in Content)
            element.Render(builder);

        builder.AppendLine("}");
    }
}

public record Property(string Name, string AccessModifier, string Type) : ICodeElementBase
{
    public void Render(StringBuilder builder) => builder.AppendLine($"{AccessModifier} {Type} {Name} {{ get; set; }}");
}

public record Enum(string Name, string AccessModifier, List<EnumValue> Values, bool HasIndex) : ICodeElementBase
{
    public void Render(StringBuilder builder)
    {
        builder.AppendLine($"{AccessModifier} enum {Name}");
        builder.AppendLine("{");

        foreach (var value in Values)
        {
            if (value.Value.HasValue)
                builder.AppendLine($"{value.Name} = {value.Value},");
            else
                builder.AppendLine($"{value.Name},");
        }

        builder.AppendLine("}");
    }
}

public record EnumValue(string Name, int? Value);

public record Method(string Name, string AccessModifier, string ReturnType, IEnumerable<Parameter> Parameters, List<IExpressionBase> Content) : ICodeElementBase
{
    public void Render(StringBuilder builder)
    {
        builder.Append($"{AccessModifier} {ReturnType} {Name}(");

        foreach (var parameter in Parameters)
        {
            parameter.Render(builder);

            if (parameter != Parameters.Last())
                builder.Append(", ");
        }

        builder.AppendLine(")");
        builder.AppendLine("{");

        foreach (var expression in Content)
            expression.Render(builder);

        builder.AppendLine("}");
    }
}

public record Parameter(string Type, string Name)
{
    public void Render(StringBuilder builder) => builder.Append($"{Type} {Name}");
}

public interface IExpressionBase : IRenderable;

public record VariableDecleration(string Type, string Name) : IExpressionBase
{
    public void Render(StringBuilder builder) => builder.Append($"{Type} {Name}");
}

public record VariableAccess(string Name) : IExpressionBase
{
    public void Render(StringBuilder builder) => builder.Append(Name);
}

public record MemberAccess(IExpressionBase Variable, params IEnumerable<IExpressionBase> Members) : IExpressionBase
{
    public void Render(StringBuilder builder)
    {
        Variable.Render(builder);
        foreach (var member in Members)
        {
            builder.Append(".");
            member.Render(builder);
        }
    }
}

public record ArrayAccess(string Name, int Index)
{
    public void Render(StringBuilder builder) => builder.Append($"{Name}[{Index}]");
}

public record CustomExpression(string Value) : IExpressionBase
{
    public void Render(StringBuilder builder) => builder.Append(Value);
}

public record Assignment(IExpressionBase Left, IExpressionBase Right) : IExpressionBase
{
    public void Render(StringBuilder builder)
    {
        Left.Render(builder);
        builder.Append(" = ");
        Right.Render(builder);

        builder.AppendLine(";");
    }
}

public record MethodCall(string MethodName, params IEnumerable<IExpressionBase> Arguments) : IExpressionBase
{
    public void Render(StringBuilder builder)
    {
        builder.Append($"{MethodName}(");

        foreach (var argument in Arguments)
        {
            argument.Render(builder);

            if (argument != Arguments.Last())
                builder.Append(", ");
        }

        builder.Append(")");
    }
}

public record IfStatement(IExpressionBase Condition, List<IExpressionBase> Content) : IExpressionBase
{
    public void Render(StringBuilder builder)
    {
        builder.Append("if (");
        Condition.Render(builder);
        builder.AppendLine(")");
        builder.AppendLine("{");

        foreach (var expression in Content)
            expression.Render(builder);

        builder.AppendLine("}");
    }
}

public record ForLoop(IExpressionBase Initializer, IExpressionBase Condition, IExpressionBase Increment, List<IExpressionBase> Content) : IExpressionBase
{
    public void Render(StringBuilder builder)
    {
        builder.Append("for (");
        Initializer.Render(builder);
        builder.Append("; ");
        Condition.Render(builder);
        builder.Append("; ");
        Increment.Render(builder);
        builder.AppendLine(")");
        builder.AppendLine("{");

        foreach (var expression in Content)
            expression.Render(builder);

        builder.AppendLine("}");
    }
}

public record ElseStatement(List<IExpressionBase> Content) : IExpressionBase
{
    public void Render(StringBuilder builder)
    {
        builder.AppendLine("else");
        builder.AppendLine("{");

        foreach (var expression in Content)
            expression.Render(builder);

        builder.AppendLine("}");
    }
}

public record Condition(IExpressionBase Left, string Comparer, IExpressionBase Right) : IExpressionBase
{
    public void Render(StringBuilder builder)
    {
        Left.Render(builder);
        builder.Append($" {Comparer} ");
        Right.Render(builder);
    }
}

public record Return(IExpressionBase Value) : IExpressionBase
{
    public void Render(StringBuilder builder)
    {
        builder.Append("return ");
        Value.Render(builder);
        builder.AppendLine(";");
    }
}