using Dalamud.Configuration;
using System;

namespace AutoCrafter;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;
    public int macro = 0;
    public bool shared = false;
    public bool isAutomatic = false;
    public bool AbortIfNoFoodPot = false;
    public bool Repair = false;
    public int count = 0;
    public uint food = 0;
    public uint syrup = 0;
}
