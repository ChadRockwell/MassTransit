namespace MassTransit.EventStore;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

internal static class ObjectValidation
{
    public static void ThrowIfNull<T>([NotNull] this T? value, [CallerArgumentExpression("value")] string name = "") where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(name);
        }
    }

    public static void ThrowIfNullOrWhiteSpace([NotNull] this string? value, [CallerArgumentExpression("value")] string name = "")
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{name} can not be NULL or white space.");
        }
    }

    public static void ThrowIfNegative(this int value, [CallerArgumentExpression("value")] string name = "")
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(name, value, $"{name} can not be < 0.");
        }
    }
}

