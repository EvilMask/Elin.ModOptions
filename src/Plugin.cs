using BepInEx;
using System;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;


namespace EvilMask.Elin.ModOptions;

internal static class ModInfo
{
    public const int Major = 0;
    public const int Minor = 23;
    public const int Patch = 74;
    public const int Build = 1;

    public const string Guid = "evilmask.elinplugins.modoptions";
    public const string Name = "Mod Options";
    public const string Version = "0.23.74";

    public const string NameAndVersion = $"{Name} ver.{Version}";
}

[BepInPlugin(ModInfo.Guid, ModInfo.Name, ModInfo.Version)]
internal class Plugin : BaseUnityPlugin
{
    internal static Plugin Instance { get; private set; }

    internal bool Ready { get; private set; } = true;
    internal Dictionary<string, ModOptionController> ManagedMods {  get; private set; } = [];
    internal string Directory => Environment.CurrentDirectory + "/Package/Mod_ModOptions/";

    private void Awake()
    {
        Instance = this;
        try
        {
            var harmony = new Harmony(ModInfo.Guid);
            harmony.PatchAll();
        }
        catch(Exception e)
        {
            Error(e.Message);
            Message("Failed to load Mod Options.");
            Ready = false;
        }
        if (Ready) Message("Mod Options loaded.");
        // Put it upper than dropdown list;
        TooltipManager.Instance.GetComponent<Canvas>().sortingOrder = 30001;
    }

    internal static void Log(object payload)
    {
        Instance!.Logger.LogInfo(payload);
    }

    internal static void Message(object payload)
    {
        Instance!.Logger.LogMessage(payload);
    }

    internal static void Error(object payload)
    {
        Instance!.Logger.LogError(payload);
    }

    internal static void Warn(object payload)
    {
        Instance!.Logger.LogWarning(payload);
    }
    internal static ModOptionController Dict = new(ModInfo.Guid);
}