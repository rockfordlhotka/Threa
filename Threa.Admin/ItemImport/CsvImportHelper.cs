using System.Globalization;
using CsvHelper;
using Spectre.Console;

namespace Threa.Admin.ItemImport;

/// <summary>
/// Shared helper for reading and writing CSV/TSV files with clear error messages.
/// </summary>
public static class CsvImportHelper
{
    /// <summary>
    /// The field separator for a file: tab for .tsv, comma otherwise.
    /// </summary>
    public static char GetSeparator(string filePath)
        => Path.GetExtension(filePath).Equals(".tsv", StringComparison.OrdinalIgnoreCase) ? '\t' : ',';

    /// <summary>
    /// Reader options for import files: separator from the extension, values trimmed,
    /// invariant culture, and line numbers tracked for error messages.
    /// </summary>
    public static CsvReaderOptions CreateReaderOptions(string filePath, Func<CsvRecordError, bool>? onRecordError = null) => new()
    {
        Separator = GetSeparator(filePath),
        TrimWhitespace = true,
        TrackLineNumbers = true,
        MissingField = CsvMissingFieldHandling.ReturnEmpty,
        FormatProvider = CultureInfo.InvariantCulture,
        OnRecordError = onRecordError
    };

    /// <summary>
    /// Creates (or overwrites) an export file: invariant culture, and fields quoted when
    /// they need it or have whitespace at either end, so values round-trip through import.
    /// </summary>
    public static CsvWriter CreateWriter(string filePath, char separator)
        => CsvWriter.ToFile(filePath, new CsvWriterOptions
        {
            Separator = separator,
            Quoting = CsvQuoting.WhenNeededOrWhitespace,
            FormatProvider = CultureInfo.InvariantCulture
        });

    /// <summary>
    /// Reads a CSV or TSV file and returns the parsed records.
    /// Returns null if there was a parsing error (error is printed to console).
    /// </summary>
    public static List<T>? ReadCsv<T>(string filePath)
    {
        CsvReader? csv = null;
        string? rawRecord = null;
        string? fieldValue = null;
        var options = CreateReaderOptions(filePath, error =>
        {
            // Capture the record while the reader is still on it; returning false rethrows.
            rawRecord = error.RawRecord;
            if (csv != null && error.Exception is CsvFormatException { FieldIndex: >= 0 } fe)
                fieldValue = fe.FieldIndex < csv.FieldCount ? csv.GetString(fe.FieldIndex) : string.Empty;
            return false;
        });

        try
        {
            using (csv = CsvReader.FromFile(filePath, options))
                return csv.GetRecords<T>().ToList();
        }
        catch (CsvFormatException ex)
        {
            PrintCsvError(ex, typeof(T), rawRecord, fieldValue);
            return null;
        }
    }

    private static void PrintCsvError(CsvFormatException ex, Type recordType, string? rawRecord, string? fieldValue)
    {
        // Member is "Type.Property"; the row types name their properties after the columns.
        var columnName = ex.Member?[(ex.Member.LastIndexOf('.') + 1)..];
        var memberType = columnName == null ? null : recordType.GetProperty(columnName)?.PropertyType;

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[red]CSV Parse Error:[/]");

        if (ex.LineNumber > 0)
            AnsiConsole.MarkupLine($"  [yellow]Row:[/] {ex.LineNumber}");

        if (columnName != null)
        {
            var actualValue = fieldValue ?? "(empty)";
            AnsiConsole.MarkupLine($"  [yellow]Column:[/] {Markup.Escape(columnName)} (index {ex.FieldIndex})");
            AnsiConsole.MarkupLine($"  [yellow]Expected:[/] {Markup.Escape(GetFriendlyTypeName(memberType))}");
            AnsiConsole.MarkupLine($"  [yellow]Found:[/] '{Markup.Escape(actualValue)}'");
            AnsiConsole.WriteLine();

            // Provide helpful suggestions based on the type
            var suggestion = GetTypeSuggestion(memberType, fieldValue ?? string.Empty);
            if (!string.IsNullOrEmpty(suggestion))
            {
                AnsiConsole.MarkupLine($"[grey]Suggestion:[/] {Markup.Escape(suggestion)}");
                AnsiConsole.WriteLine();
            }
        }
        else
        {
            AnsiConsole.MarkupLine($"  [yellow]Error:[/] {Markup.Escape(ex.Message)}");
            AnsiConsole.WriteLine();
        }

        // Show the problematic row data if available
        if (rawRecord != null)
        {
            AnsiConsole.MarkupLine("[grey]Raw row data:[/]");
            AnsiConsole.MarkupLine($"  {Markup.Escape(rawRecord.TrimEnd())}");
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
