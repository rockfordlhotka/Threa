using System;
using System.Linq;
using Csla.Core;
using Csla.Rules;

namespace GameMechanics.Skills;

/// <summary>
/// CSLA validation rule that validates attribute specifications.
/// Supports single attributes (e.g., "STR") and compound attributes (e.g., "DEX/ITT").
/// </summary>
public class AttributeValidationRule : BusinessRule
{
    /// <summary>
    /// Valid attribute codes.
    /// </summary>
    public static readonly string[] ValidAttributes = ["STR", "DEX", "END", "INT", "ITT", "WIL", "PHY", "SOC"];

    private readonly bool _required;

    /// <summary>
    /// Creates a new attribute validation rule.
    /// </summary>
    /// <param name="primaryProperty">The property to validate.</param>
    /// <param name="required">Whether the attribute is required (true for primary, false for secondary/tertiary).</param>
    public AttributeValidationRule(IPropertyInfo primaryProperty, bool required = true)
        : base(primaryProperty)
    {
        _required = required;
        InputProperties.Add(primaryProperty);
    }

    protected override void Execute(IRuleContext context)
    {
        var value = context.InputPropertyValues[PrimaryProperty] as string;

        if (string.IsNullOrWhiteSpace(value))
        {
            if (_required)
            {
                context.AddErrorResult($"{PrimaryProperty.FriendlyName} is required.");
            }
            return;
        }

        var parts = value.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            if (_required)
            {
                context.AddErrorResult($"{PrimaryProperty.FriendlyName} is required.");
            }
            return;
        }

        foreach (var part in parts)
        {
            if (!ValidAttributes.Contains(part.ToUpperInvariant()))
            {
                context.AddErrorResult(
                    $"Invalid attribute '{part}' in {PrimaryProperty.FriendlyName}. " +
                    $"Valid attributes: {string.Join(", ", ValidAttributes)}");
                return;
            }
        }
    }

    /// <summary>
    /// Gets the valid attributes message for display.
    /// </summary>
    public static string GetValidAttributesMessage() => string.Join(", ", ValidAttributes);
}
