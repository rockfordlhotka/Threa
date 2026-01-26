using Csla.Core;
using Csla.Rules;
using ProfanityFilter;

namespace GameMechanics.Player;

/// <summary>
/// CSLA business rule that validates a string property contains no profanity.
/// Uses Profanity.Detector library which handles the Scunthorpe problem (false positives).
/// </summary>
public class NoProfanityRule : BusinessRule
{
    private readonly ProfanityFilter.ProfanityFilter _filter;

    public NoProfanityRule(IPropertyInfo primaryProperty)
        : base(primaryProperty)
    {
        _filter = new ProfanityFilter.ProfanityFilter();
    }

    protected override void Execute(IRuleContext context)
    {
        var value = (string?)context.InputPropertyValues[PrimaryProperty];
        if (!string.IsNullOrWhiteSpace(value))
        {
            if (_filter.ContainsProfanity(value))
            {
                context.AddErrorResult("Display name contains inappropriate content");
            }
        }
    }
}
