using System;
using ClickLib;
using ClickLib.Exceptions;
using Dalamud.Logging;
using ECommons.Automation;
using FFXIVClientStructs.FFXIV.Client.UI;
using static ECommons.GenericHelpers;

namespace AutoCrafter.Manager;
unsafe class RecipeNoteManager
{

    public static void TickQuestStartProduction()
    {
        try
        {
            Click.SendClick("synthesize");
        }
        catch (ClickNotFoundError)
        {
            PluginLog.Log("Click not found");
        }
        catch (NullReferenceException) { }
    }

    public static void OpenCraftingMenu()
    {
        if (!TryGetAddonByName<AddonRecipeNote>("RecipeNote", out var addon))
        {
            if (Throttler.Throttle(1000))
            {
                new Chat().SendMessage("/clog");
            }
        }
    }

    public static void CloseCraftingMenu()
    {
        if (TryGetAddonByName<AddonRecipeNote>("RecipeNote", out var addon) && addon->AtkUnitBase.IsVisible)
        {
            if (Throttler.Throttle(1000))
            {
                new Chat().SendMessage("/clog");
            }
        }
    }
}