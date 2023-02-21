using AutoCrafter.Manager;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using ECommons;
using ECommons.ImGuiMethods;
using ImGuiNET;
using System.Numerics;

namespace AutoCrafter.View;

internal class AutoCrafterView : Window
{
    public AutoCrafterView() : base("AutoCrafterView", ImGuiWindowFlags.NoScrollbar)
    {
        Vector2 minSize = new Vector2(400, 220);
        Vector2 maxiSize = new Vector2(400, 320);
        this.Size = minSize;
        this.SizeCondition = ImGuiCond.FirstUseEver;
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = minSize,
            MaximumSize = maxiSize
        };
    }

    public override void Draw()
    {


        ImGui.BeginChild("", ImGui.GetContentRegionAvail() with { Y = ImGui.GetContentRegionAvail().Y - ImGuiHelpers.GetButtonSize("保存并关闭").Y - 6 });

        if (ImGui.Checkbox("是否自动制作", ref AutoCrafter.Instance.config.isAutomatic))
        {
            AutoCrafter.Instance.saveConfig();

        }
        if (AutoCrafter.Instance.config.isAutomatic)
        {

            if (ImGui.InputInt(":制作次数", ref AutoCrafter.Instance.config.count))
            {
                if (AutoCrafter.Instance.config.count < 0) AutoCrafter.Instance.config.count = -1;
                if (AutoCrafter.Instance.config.count > 140) AutoCrafter.Instance.config.count = 140;
            }
            ImGuiComponents.HelpMarker("剩余次数,-1代表不结束");
        }

        if (ImGui.InputInt(":宏号", ref AutoCrafter.Instance.config.macro))
        {
            if (AutoCrafter.Instance.config.macro < 0) AutoCrafter.Instance.config.macro = 0;
            if (AutoCrafter.Instance.config.macro > 99) AutoCrafter.Instance.config.macro = 99;
        }
        if (ImGui.Checkbox("是否使用公用宏", ref AutoCrafter.Instance.config.shared))
        {
            AutoCrafter.Instance.saveConfig();
        }

        if (ImGui.Checkbox("自动修复", ref AutoCrafter.Instance.config.Repair))
        {
            AutoCrafter.Instance.saveConfig();
        }

        if (ImGui.Checkbox("使用食物或药水", ref AutoCrafter.Instance.config.AbortIfNoFoodPot))
        {
            AutoCrafter.Instance.saveConfig();
        }


        if (AutoCrafter.Instance.config.AbortIfNoFoodPot)
        {
            {
                ImGuiEx.TextV("使用食物:");
                if (ImGui.BeginCombo("##foodBuff", BuffManager.Food.TryGetFirst(x => x.Id == AutoCrafter.Instance.config.food, out var item) ? $" {item.Name}" : $"{(AutoCrafter.Instance.config.food == 0 ? "Disabled" : $" {AutoCrafter.Instance.config.food}")}"))
                {
                    if (ImGui.Selectable("Disable"))
                    {
                        AutoCrafter.Instance.config.food = 0;
                    }
                    foreach (var x in BuffManager.GetFood(true))
                    {
                        if (ImGui.Selectable($" {x.Name}"))
                        {
                            AutoCrafter.Instance.config.food = x.Id;
                        }
                    }
                    ImGui.EndCombo();
                }
            }
            {
                ImGuiEx.TextV("使用药水:");
                if (ImGui.BeginCombo("##syrupBuff", BuffManager.Syrup.TryGetFirst(x => x.Id == AutoCrafter.Instance.config.syrup, out var item) ? $" {item.Name}" : $"{(AutoCrafter.Instance.config.syrup == 0 ? "Disabled" : $" {AutoCrafter.Instance.config.syrup}")}"))
                {

                    if (ImGui.Selectable("Disable"))
                    {
                        AutoCrafter.Instance.config.syrup = 0;
                    }
                    foreach (var x in BuffManager.GetSyrup(true))
                    {
                        if (ImGui.Selectable($" {x.Name}"))
                        {
                            AutoCrafter.Instance.config.syrup = x.Id;
                        }
                    }
                    ImGui.EndCombo();
                }
            }
        }

        ImGui.EndChild();
        ImGui.Separator();
        ImGui.NewLine();
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - ImGuiHelpers.GetButtonSize("保存并关闭").X - 6);

        if (ImGui.Button("保存并关闭"))
        {
            AutoCrafter.Instance.saveConfig();
            IsOpen = false;
            PluginLog.Log("Settings saved.");
        }

        ImGui.End();
    }
}
