
This is a mod for modders. It allows you to configure your mod settings page using a .xml file or through code, eliminating the need to close the game and modify the .cfg file to change the mod settings.

## Quick Start

Here is a quick guide to show you how to use this mod. 

### Dependency

* Add the `.dll` file of this mod to your dependencies list.
![Mod Preview Image](/doc/assets/dependencies.png)
* You should always reference from the mod folder `Elin/Package/Mod_ModOptions` so that you get the updated libraries as the mod updates.
* Remember to put `Mod Options` above your mod in the mod list when you are testing your mod.
![Mod Preview Image](/doc/assets/load_order.png)

### Register

* You should check if `Mod Options` is loaded first.
```c#
internal const string ModOptionsGuid = "evilmask.elinplugins.modoptions";

private void Awake()
{
    var mod_options_loaded
    foreach (var obj in ModManager.ListPluginObject)
    {
        var plugin = obj as BaseUnityPlugin;
        if (plugin.Info.Metadata.GUID == ModInfo.ModOptionsGuid)
        {
            // Mod Options is loaded, you can do
            // registeration now.
            break;
        }
    }
}
```
* If you do this in `Start()` instead of `Awake()`, load order is no longer required.
* Call `ModOptionController.Register` to register your mod, now you have a tab on the mod option page! The second parameter is optional if you want a tooltip for your tab button.
```c#
var controller = ModOptionController.Register(ModInfo.Guid, "mod.tooltip");
```
### Load UI layout
* Now we read our UI layout from an `xml` file and add some translations. You can find the example `xml` [here](/doc/ConfigExample.en.xml).
``` c#
StreamReader sr = new(PathToYourModFolder + "ConfigExample.en.xml");
controller.SetPreBuildWithXml(sr.ReadToEnd());
sr.Close();

controller.SetTranslation(ModInfo.Guid,
    "MyMod(EN)", "何かのMod(JP)", "我的模组(CN)");
controller.SetTranslation("mod.tooltip",
    "This is my mod!", "俺が作ったのだ！", "这是我的模组！");
controller.SetTranslation("exampleText",
    "This text only has an English version!");
```
* Remember to reference [the XML schema](/doc/ConfigLayoutSchema.xsd) when you create your  `xml` layout file. I plan to add the read-translations-from-file feature later.
### Setup UI through code
* When `Mod Options` starts to build your tab, it will trigger `OnBuildUI` event. Listen to the event to make modifications to the layout or just build the UI all through your code.
``` c#
controller.OnBuildUI += builder =>
{
    var button = builder.GetPreBuild<OptButton>("btn01");
    int count = 0;
    button.OnClicked += () =>
    {
        count++;
        button.Text = count.ToString();
    };

    var slider = builder.GetPreBuild<OptSlider>("slider02");
    slider.Title = slider.Value.ToString();
    slider.Step = 10;
    slider.OnValueChanged += v => slider.Title = slider.Value.ToString();
};
```
* You can get any element with the `id` attribute in the `xml` file through `OptionUIBuilder.GetPreBuild<T>(string id)`. Any `OptionUIBuilder` instance or `OptUIElement` instance is only valid when the player is viewing your mod option tab, so don't store them.