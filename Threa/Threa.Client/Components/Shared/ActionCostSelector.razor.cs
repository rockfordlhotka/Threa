using Microsoft.AspNetCore.Components;

namespace Threa.Client.Components.Shared;

public enum ActionCostType
{
    OneAPOneFat,
    TwoAP
}

public class ActionCostSelectorBase : ComponentBase
{
    [Parameter] public int AP { get; set; }
    [Parameter] public int Fat { get; set; }
    [Parameter] public EventCallback<ActionCostType> OnCostTypeSelected { get; set; }

    protected ActionCostType SelectedCostType { get; set; }

    protected override void OnInitialized()
    {
        // Default to the most common/cheapest option if available
        if (AP >= 1 && Fat >= 1)
        {
            SelectedCostType = ActionCostType.OneAPOneFat;
        }
        else if (AP >= 2)
        {
            SelectedCostType = ActionCostType.TwoAP;
        }
    }

    protected async Task SetCostType(ActionCostType costType)
    {
        SelectedCostType = costType;
        await OnCostTypeSelected.InvokeAsync(SelectedCostType);
    }
}
