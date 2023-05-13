using AutoCrafter.View;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game;
using ClickLib;
using Dalamud.Game.Command;
using ECommons.DalamudServices;
using static ECommons.GenericHelpers;

using AutoCrafter.Manager;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Logging;
using ECommons;

namespace AutoCrafter;
public sealed class AutoCrafter : IDalamudPlugin
{
    public string Name => "AutoCrafter";
    internal static AutoCrafter Instance { get; private set; } = null!;
    internal Configuration config { get; }
    public WindowSystem windowSystem;
    private const string commandName = "/nextmacro";

    private bool isPackage = true;
    private bool isMaterial = true;

    public AutoCrafter(DalamudPluginInterface pluginInterface)
    {
        Instance = this;
        ECommonsMain.Init(pluginInterface, this);
        config = Svc.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Click.Initialize();
        BuffManager.Init();
        AutoCrafterView autoCrafterView = new AutoCrafterView();
        windowSystem = new WindowSystem(Name);
        windowSystem.AddWindow(autoCrafterView);

        Svc.PluginInterface.UiBuilder.Draw += windowSystem.Draw;
        Svc.Framework.Update += Tick;
        Svc.Chat.ChatMessage += Chat_ChatMessage;
        Svc.PluginInterface.UiBuilder.OpenConfigUi += delegate { autoCrafterView.IsOpen = true; };

        Svc.Commands.AddHandler(commandName, new CommandInfo(OnMacroCommandHandler)
        {
            HelpMessage = "Executes the next macro.",
            ShowInHelp = true
        });

    }

    private void OnMacroCommandHandler(string command, string arguments)
    {
        byte num = (byte)config.macro;
        runMacro(++num);
    }

    private unsafe void Tick(Framework framework)
    {

        if (config.isAutomatic)
        {

            if (checkAFK())
            {
                if (NativeManager.TryFindGameWindow(out var hwnd))
                {
                    NativeManager.Keypress.SendKeycode(hwnd, NativeManager.Keypress.Space);
                }
                else
                {
                    PluginLog.Log("Could not find game window");
                }
            }

            if (config.AbortIfNoFoodPot && !BuffManager.CheckConsumables(false))
            {
                if (TryGetAddonByName<AtkUnitBase>("RecipeNote", out var addon) && addon->IsVisible && Svc.Condition[ConditionFlag.Crafting])
                {
                    RecipeNoteManager.CloseCraftingMenu();
                }
                else
                {
                    if (!Svc.Condition[ConditionFlag.Crafting])
                    {
                        BuffManager.CheckConsumables(true);
                    }
                }
                return;
            }

            if (config.Repair && RepairManager.GetMinEquippedPercent() < 10)
            {
                if (TryGetAddonByName<AtkUnitBase>("RecipeNote", out var addon) && addon->IsVisible && Svc.Condition[ConditionFlag.Crafting])
                {
                    RecipeNoteManager.CloseCraftingMenu();
                }
                else
                {
                    if (!Svc.Condition[ConditionFlag.Crafting])
                    {
                        RepairManager.ProcessRepair();
                    }
                }
                return;
            }

            if (config.count != 0 && isPackage && isMaterial)
            {
                if (!TryGetAddonByName<AtkUnitBase>("RecipeNote", out var recipeNote) || !recipeNote->IsVisible)
                {
                    if (!Svc.Condition[ConditionFlag.Crafting])
                    {
                        RecipeNoteManager.OpenCraftingMenu();
                    }
                    return;
                }
                RecipeNoteManager.TickQuestStartProduction();
            }

        }

    }


    private bool checkAFK()
    {
        var onlineStatus = Svc.ClientState.LocalPlayer?.OnlineStatus.GetWithLanguage(Svc.ClientState.ClientLanguage);
        if (onlineStatus != null)
        {
            return onlineStatus.Name.RawString.Contains("离开");
        }
        return false;
    }
    private void Chat_ChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (message.TextValue.EndsWith("从背包里取出了材料。"))
        {
            if (config.count > 0)
            {
                config.count--;
            }
            isPackage = true;
            isMaterial = true;
            var num = (byte)config.macro;
            saveConfig();
            runMacro(num);
        }
        if ("素材不足，无法进行制作作业。".Equals(message.TextValue))
        {
            isMaterial = false;
        }
        if (message.TextValue.EndsWith("的背包已满，无法进行制作作业。"))
        {
            isPackage = false;
        }
    }
    unsafe public void runMacro(byte num)
    {
        RaptureShellModule.Instance->ExecuteMacro((config.shared ? RaptureMacroModule.Instance->Shared : RaptureMacroModule.Instance->Individual)[num]);
    }

    public void saveConfig()
    {
        Svc.PluginInterface.SavePluginConfig(config);
    }

    public void Dispose()
    {
        Svc.PluginInterface.UiBuilder.Draw -= windowSystem.Draw;
        Svc.Framework.Update -= Tick;
        Svc.Chat.ChatMessage -= Chat_ChatMessage;
        Svc.Commands.RemoveHandler(commandName);
        ECommonsMain.Dispose();
    }
}
