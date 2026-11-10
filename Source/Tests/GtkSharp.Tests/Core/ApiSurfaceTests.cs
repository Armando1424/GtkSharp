using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;

namespace GtkSharp.Tests.Core;

/// <summary>
/// Guards the public API surface of the wrapper assemblies.
///
/// The baseline is generated from the current sources. When a change removes,
/// renames or changes the signature of a public member, this test fails and
/// forces the author to confirm the change. To accept an intentional change:
///
/// <code>UPDATE_API_BASELINE=1 dotnet test --filter ApiSurfaceTests</code>
///
/// and review the resulting diff in <c>PublicAPI/*.txt</c>.
/// </summary>
public sealed class ApiSurfaceTests
{
    private static readonly string ProjectDirectory =
        typeof(ApiSurfaceTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .First(attribute => attribute.Key == "ProjectDirectory")
            .Value!;

    private static readonly bool UpdateBaseline =
        Environment.GetEnvironmentVariable("UPDATE_API_BASELINE") == "1";

    public static TheoryData<string> Assemblies => new()
    {
        "GtkSharp",
        "GLibSharp",
        "GdkSharp",
        "GioSharp",
        "AtkSharp",
        "PangoSharp",
        "CairoSharp",
    };

    [Theory]
    [MemberData(nameof(Assemblies))]
    public void PublicApi_MatchesBaseline(string assemblyName)
    {
        var assembly = LoadAssembly(assemblyName);
        var actual = BuildSurface(assembly);
        var baselinePath = Path.Combine(ProjectDirectory, "PublicAPI", $"{assemblyName}.txt");

        if (UpdateBaseline)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(baselinePath)!);
            File.WriteAllText(baselinePath, actual, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            return;
        }

        Assert.True(
            File.Exists(baselinePath),
            $"Missing API baseline for {assemblyName}. Run with UPDATE_API_BASELINE=1 to create {baselinePath}.");

        var expected = File.ReadAllText(baselinePath).Replace("\r\n", "\n");

        Assert.True(
            expected == actual,
            Diff(assemblyName, expected, actual));
    }

    private static Assembly LoadAssembly(string name)
    {
        // The test assembly already references every wrapper, so this only
        // resolves an assembly that is already loaded.
        return AppDomain.CurrentDomain.GetAssemblies()
                   .FirstOrDefault(assembly => assembly.GetName().Name == name)
               ?? Assembly.Load(name);
    }

    private static string BuildSurface(Assembly assembly)
    {
        var builder = new StringBuilder();

        foreach (var type in assembly.GetExportedTypes().OrderBy(type => type.FullName, StringComparer.Ordinal))
        {
            builder.Append("T:").Append(NormalizeType(type)).Append('\n');

            const BindingFlags flags =
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

            foreach (var constructor in type.GetConstructors(flags).OrderBy(Format, StringComparer.Ordinal))
                builder.Append("C:").Append(Format(constructor)).Append('\n');

            foreach (var method in type.GetMethods(flags)
                         .Where(method => !method.IsSpecialName)
                         .OrderBy(Format, StringComparer.Ordinal))
                builder.Append("M:").Append(Format(method)).Append('\n');

            foreach (var property in type.GetProperties(flags).OrderBy(property => property.Name, StringComparer.Ordinal))
                builder.Append("P:").Append(NormalizeType(property.PropertyType)).Append(' ')
                    .Append(NormalizeType(type)).Append('.').Append(property.Name).Append('\n');

            foreach (var field in type.GetFields(flags).OrderBy(field => field.Name, StringComparer.Ordinal))
                builder.Append("F:").Append(NormalizeType(field.FieldType)).Append(' ')
                    .Append(NormalizeType(type)).Append('.').Append(field.Name).Append('\n');

            foreach (var @event in type.GetEvents(flags).OrderBy(@event => @event.Name, StringComparer.Ordinal))
                builder.Append("E:").Append(NormalizeType(@event.EventHandlerType!)).Append(' ')
                    .Append(NormalizeType(type)).Append('.').Append(@event.Name).Append('\n');
        }

        return builder.ToString();
    }

    private static string Format(MethodBase method)
    {
        var parameters = string.Join(",", method.GetParameters().Select(parameter => NormalizeType(parameter.ParameterType)));
        var returnType = method is MethodInfo info ? NormalizeType(info.ReturnType) + " " : string.Empty;
        return $"{returnType}{NormalizeType(method.DeclaringType!)}.{method.Name}({parameters})";
    }

    /// <summary>
    /// Produces a stable type name. Type.FullName includes the assembly version
    /// for generic arguments, which changes with every build.
    /// </summary>
    private static string NormalizeType(Type type)
    {
        if (type.IsByRef)
            return NormalizeType(type.GetElementType()!) + "&";

        if (type.IsArray)
            return NormalizeType(type.GetElementType()!) + "[]";

        if (type.IsGenericParameter)
            return (type.DeclaringMethod is null ? "!" : "!!") + type.Name;

        if (type.IsGenericType)
        {
            var name = type.GetGenericTypeDefinition().FullName ?? type.Name;
            var tick = name.IndexOf('`');
            if (tick >= 0)
                name = name[..tick];

            var arguments = string.Join(",", type.GetGenericArguments().Select(NormalizeType));
            return $"{name}<{arguments}>";
        }

        return type.FullName ?? type.Name;
    }

    private static string Diff(string assemblyName, string expected, string actual)
    {
        var expectedLines = expected.Split('\n').ToHashSet(StringComparer.Ordinal);
        var actualLines = actual.Split('\n').ToHashSet(StringComparer.Ordinal);

        var removed = expectedLines.Except(actualLines, StringComparer.Ordinal).Take(15).ToArray();
        var added = actualLines.Except(expectedLines, StringComparer.Ordinal).Take(15).ToArray();

        var builder = new StringBuilder();
        builder.AppendLine($"The public API of {assemblyName} changed.");

        if (removed.Length > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Removed or changed (first 15):");
            foreach (var line in removed)
                builder.AppendLine("  - " + line);
        }

        if (added.Length > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Added (first 15):");
            foreach (var line in added)
                builder.AppendLine("  + " + line);
        }

        builder.AppendLine();
        builder.AppendLine("If the change is intentional, regenerate the baseline:");
        builder.AppendLine("  UPDATE_API_BASELINE=1 dotnet test --filter ApiSurfaceTests");

        return builder.ToString();
    }
}
