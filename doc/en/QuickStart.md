# Quick Start

Here is a quick guide to show you how to use this mod. 

### Dependency

* Add the `.dll` file of this mod to your dependencies list.

![Dependencies image](/doc/assets/dependencies.png)
* You should always reference from the mod folder `Elin/Package/Mod_ModOptions` so that you get the updated libraries as the mod updates.
* Remember to put `Mod Options` above your mod in the mod list when you are testing your mod.

![Load order image](/doc/assets/load_order.png)

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
        if (plugin.Info.Metadata.GUID == ModOptionsGuid)
        {
            // Mod Options is loaded, you can do
            // registeration now.
            break;
        }
    }
}
```
* If you do this in `Start()` instead of `Awake()`, load order is no longer required.

See also [Register](./Register.md).

### Load UI layout
* Now we read our UI layout from an `XML` file and add some translations. You can find the example `XML` [here](/doc/ConfigExample.en.xml).
``` c#
using (StreamReader sr = new(PathToYourModFolder + "ConfigExample.en.xml"))
    controller.SetPreBuildWithXml(sr.ReadToEnd());

controller.SetTranslation(ModInfo.Guid,
    "MyMod(EN)", "何かのMod(JP)", "我的模组(CN)");
controller.SetTranslation("mod.tooltip",
    "This is my mod!", "俺が作ったのだ！", "这是我的模组！");
controller.SetTranslation("exampleText",
    "This text only has an English version!");
```
* Remember to reference [the XML schema](/doc/ConfigLayoutSchema.xsd) when you create your  `XML` layout file.

### Setup UI through code
* When `Mod Options` starts to build your tab, it will trigger the `OnBuildUI` event. Subscribe to the event to make modifications to the layout or just build the UI all through your code.
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
* You can get any element with the `id` attribute in the `XML` file through `OptionUIBuilder.GetPreBuild<T>(string id)`. Any `OptionUIBuilder` instance or `OptUIElement` instance is only valid when the player is viewing your mod option tab, so don't store them.
