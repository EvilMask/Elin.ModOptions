# Register

```c#
static ModOptionController Register(string guid,
    string tooptipId = null,
    params object[] configs)
```
* `guid`: This can be any unique string except an empty string or null, but the GUID of your mod is recommended.
* `tooptipId`: A translation ID for your mod's tab button. Keep it null if you don't want it. See also: [Translation]().
* `configs`: Serials of instances that contain your `ConfigEntry` as their field.  See also: [Pre-build UI](/doc/en/PreBuildUI.md#reflection).

You will get a `ModOptionController` instance for your mod, through which you can interact with `Mod Options`, so keep it somewhere.

## In `Awake()`
>[!IMPORTANT]
> If you want to register in `Awake()` for some reason, do it after `ModOptions.dll` is loaded, which requires the player to put `Mod Options` mod above your mod in the mods list.

You can check if `Mod Options` is loaded by following code:

```c#
foreach (var obj in ModManager.ListPluginObject)
{
    var plugin = obj as BaseUnityPlugin;
    if (plugin.Info.Metadata.GUID == "evilmask.elinplugins.modoptions")
    {
        // Mod Options is loaded, you can do
        // registeration now.
        break;
    }
}
```
If you want to check the version of `Mod Options`, use:

```c#
EvilMask.Elin.ModOptions.Version.Current
```

## In `Start()`

All mods are loaded in the mods list and enabled now. <ins>Load order is no longer needed if you do the registration here.</ins> You still need to check if `Mod Options` is loaded though.