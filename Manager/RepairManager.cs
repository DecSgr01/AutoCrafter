using ClickLib.Clicks;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using System;
using static ECommons.GenericHelpers;

namespace AutoCrafter.Manager;

internal unsafe class RepairManager
{
    internal static void Repair()
    {
        if (TryGetAddonByName<AddonRepair>("Repair", out var addon) && addon->AtkUnitBase.IsVisible && addon->RepairAllButton->IsEnabled && Throttler.Throttle(500))
        {
            new ClickRepair((IntPtr)addon).RepairAll();
        }
    }

    internal static void ConfirmYesNo()
    {
        if (TryGetAddonByName<AddonRepair>("Repair", out var r) &&
            r->AtkUnitBase.IsVisible && TryGetAddonByName<AddonSelectYesno>("SelectYesno", out var addon) &&
            addon->AtkUnitBase.IsVisible &&
            addon->YesButton->IsEnabled &&
            addon->AtkUnitBase.UldManager.NodeList[15]->IsVisible &&
            Throttler.Throttle(500))
        {
            new ClickSelectYesNo((IntPtr)addon).Yes();
        }
    }

    internal static int GetMinEquippedPercent()
    {
        var ret = int.MaxValue;
        var equipment = InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems);
        for (var i = 0; i < equipment->Size; i++)
        {
            var item = equipment->GetInventorySlot(i);
            if (item->Condition < ret) ret = item->Condition;
        }
        return (ret / 300);
    }

    internal static bool ProcessRepair()
    {
        if (TryGetAddonByName<AddonRepair>("Repair", out var r) && r->AtkUnitBase.IsVisible)
        {
            ConfirmYesNo();
            Repair();
        }
        else
        {
            if (Throttler.Throttle(500))
            {
                ActionManager.Instance()->UseAction(ActionType.General, 6);
            }
        }
        return false;
    }
}
