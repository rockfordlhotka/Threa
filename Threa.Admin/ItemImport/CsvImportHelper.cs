using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Spectre.Console;

namespace Threa.Admin.ItemImport;

/// <summary>
/// Shared helper for reading CSV/TSV files with clear error messages.
/// </summary>
public static class CsvImportHelper
{
    /// <summary>
    /// Reads a CSV or TSV file and returns the parsed records.
    /// Returns null if there was a parsing error (error is printed to console).
    /// </summary>
    public static List<T>? ReadCsv<T, TMap>(string filePath) where TMap : ClassMap<T>
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        var delimiter = ext == ".tsv" ? "\t" : ",";

        try
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = delimiter,
                HeaderValidated = null,
                MissingFieldFound = null,
                TrimOptions = TrimOptions.Trim
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<TMap>();
            return csv.GetRecords<T>().ToList();
        }
        catch (TypeConverterException ex)
        {
            PrintTypeConversionError(ex);
            return null;
        }
        catch (ReaderException ex) when (ex.InnerException is TypeConverterException tce)
        {
            PrintTypeConversionError(tce);
            return null;
        }
        catch (CsvHelperException ex)
        {
            PrintGenericCsvError(ex);
            return null;
        }
    }

    private static void PrintTypeConversionError(TypeConverterException ex)
    {
        var context = ex.Context;
        var row = context?.Parser?.RawRow ?? 0;
        var columnName = ex.MemberMapData?.Member?.Name ?? "Unknown";
        var columnIndex = ex.MemberMapData?.Index ?? -1;
        var expectedType = GetFriendlyTypeName(ex.MemberMapData?.Member?.MemberType());
        var actualValue = ex.Text ?? "(empty)";

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[red]CSV Parse Error:[/]");
        AnsiConsole.MarkupLine($"  [yellow]Row:[/] {row}");
        AnsiConsole.MarkupLine($"  [yellow]Column:[/] {columnName} (index {columnIndex})");
        AnsiConsole.MarkupLine($"  [yellow]Expected:[/] {expectedType}");
        AnsiConsole.MarkupLine($"  [yellow]Found:[/] '{Markup.Escape(actualValue)}'");
        AnsiConsole.WriteLine();

        // Provide helpful suggestions based on the type
        var suggestion = GetTypeSuggestion(ex.MemberMapData?.Member?.MemberType(), actualValue);
        if (!string.IsNullOrEmpty(suggestion))
        {
            AnsiConsole.MarkupLine($"[grey]Suggestion:[/] {suggestion}");
            AnsiConsole.WriteLine();
        }

        // Show the problematic row data if available
        if (context?.Parser?.RawRecord != null)
        {
            AnsiConsole.MarkupLine("[grey]Raw row data:[/]");
            AnsiConsole.MarkupLine($"  {Markup.Escape(context.Parser.RawRecord.TrimEnd())}");
            AnsiConsole.WriteLine();
        }
    }

    private static void PrintGenericCsvError(CsvHelperException ex)
    {
        var context = ex.Context;
        var row = context?.Parser?.RawRow ?? 0;

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[red]CSV Parse Error:[/]");

        if (row > 0)
            AnsiConsole.MarkupLine($"  [yellow]Row:[/] {row}");

        // Try to extract useful info from the message
        var message = ex.Message;

        // Remove the verbose state dump that CsvHelper adds
        var stateIndex = message.IndexOf("IReader state:", StringComparison.Ordinal);
        if (stateIndex > 0)
            message = message.Substring(0, stateIndex).Trim();

        AnsiConsole.MarkupLine($"  [yellow]Error:[/] {Markup.Escape(message)}");
        AnsiConsole.WriteLine();

        // Show the problematic row data if available
        if (context?.Parser?.RawRecord != null)
        {
            AnsiConsole.MarkupLine("[grey]Raw row data:[/]");
            AnsiConsole.MarkupLine($"  {Markup.Escape(context.Parser.RawRecord.TrimEnd())}");
            AnsiConsole.WriteLine();
        }
    }

    private static string GetFriendlyTypeName(Type? type)
    {
        if (type == null) return "Unknown";

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType.Name switch
        {
            "Int32" => "integer number",
            "Int64" => "integer number",
            "Decimal" => "decimal number",
            "Double" => "decimal number",
            "Single" => "decimal number",
            "Boolean" => "true/false",
            "String" => "text",
            _ when underlyingType.IsEnum => $"one of: {string.Join(", ", Enum.GetNames(underlyingType))}",
            _ => underlyingType.Name
        };
    }

    private static string? GetTypeSuggestion(Type? type, string actualValue)
    {
        if (type == null) return null;

        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        var isEmpty = string.IsNullOrWhiteSpace(actualValue);

        if (underlyingType == typeof(int) || underlyingType == typeof(long))
        {
            if (isEmpty)
                return "Use 0 instead of leaving the cell empty for numeric columns.";
            if (actualValue.Contains('.') || actualValue.Contains(','))
                return "This column expects a whole number without decimals.";
            return "Enter a valid whole number (e.g., 0, 1, 100).";
        }

        if (underlyingType == typeof(decimal) || underlyingType == typeof(double) || underlyingType == typeof(float))
        {
            if (isEmpty)
                return "Use 0 instead of leaving the cell empty for numeric columns.";
            return "Enter a valid number (e.g., 0, 1.5, 100).";
        }

        if (underlyingType == typeof(bool))
        {
            if (isEmpty)
                return "Use 'false' instead of leaving the cell empty for true/false columns.";
            return "Enter 'true' or 'false'.";
        }

        if (underlyingType.IsEnum)
        {
            var validValues = string.Join(", ", Enum.GetNames(underlyingType));
            if (isEmpty)
                return $"This column cannot be empty. Valid values: {validValues}";
            return $"'{actualValue}' is not a valid value. Use one of: {validValues}";
        }

        return null;
    }
}
