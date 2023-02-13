using System.Linq;
using ECommons;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.GeneratedSheets;

namespace AutoCrafter.Manager;

public unsafe class BuffManager
{
    public static (uint Id, string Name)[] Food;
    public static (uint Id, string Name)[] Syrup;

    public static void Init()
    {
        Food = Svc.Data.GetExcelSheet<Item>().Where(x => x.ItemUICategory.Value?.RowId == 46 && IsCraftersAttribute(x)).Select(x => (x.RowId, x.Name.ToString())).ToArray();
        Syrup = Svc.Data.GetExcelSheet<Item>().Where(x => !x.RowId.EqualsAny<uint>(4570) && x.ItemUICategory.Value?.RowId == 44 && IsCraftersAttribute(x)).Select(x => (x.RowId, x.Name.ToString())).ToArray();
    }
    public static bool IsCraftersAttribute(Item x)
    {
        try
        {
            foreach (var z in x.ItemAction.Value.Data)
            {
                if (Svc.Data.GetExcelSheet<ItemFood>().GetRow(z).UnkData1[0].BaseParam.EqualsAny<byte>(11, 70, 71))
                {
                    return true;
                }
            }
        }
        catch { }
        return false;
    }

    public static bool checkFood()
    {
        return Svc.ClientState.LocalPlayer?.StatusList.Any(x => x.StatusId == 48 && x.RemainingTime > 0f) == true;
    }
    public static bool checkSyrup()
    {
        return Svc.ClientState.LocalPlayer?.StatusList.Any(x => x.StatusId == 49 && x.RemainingTime > 0f) == true;
    }

    public unsafe static (uint Id, string Name)[] GetFood(bool hq = false)
    {
        return Food.Where(x => InventoryManager.Instance()->GetInventoryItemCount(x.Id, hq) > 0).ToArray();
    }

    public unsafe static (uint Id, string Name)[] GetSyrup(bool hq = false)
    {
        return Syrup.Where(x => InventoryManager.Instance()->GetInventoryItemCount(x.Id, hq) > 0).ToArray();
    }
    internal static unsafe bool UseItem(uint itemID, bool hq = false)
    {
        if (hq)
        {
            itemID += 1_000_000;
        }

        if (Throttler.Throttle(2000))
        {
            return ActionManager.Instance() is not null && ActionManager.Instance()->UseAction(ActionType.Item, itemID, a4: 65535);
        }
        return false;
    }
    internal static bool CheckConsumables(bool use = true)
    {
        var fooded = checkFood() || Plugin.Instance.config.food == 0;
        if (!fooded)
        {
            if (GetFood(true).Any())
            {
                if (use) UseItem(Plugin.Instance.config.food, true);
                return false;
            }
            else
            {
                fooded = !Plugin.Instance.config.AbortIfNoFoodPot;
            }
        }
        var potted = checkSyrup() || Plugin.Instance.config.syrup == 0;
        if (!potted)
        {
            if (GetSyrup(true).Any())
            {
                if (use) UseItem(Plugin.Instance.config.syrup, true);
                return false;
            }
            else
            {
                potted = !Plugin.Instance.config.AbortIfNoFoodPot;
            }
        }
        var ret = potted && fooded;
        return ret;
    }
}
