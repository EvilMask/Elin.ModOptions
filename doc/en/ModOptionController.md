# Mod Option Controller

You can get a `ModOptionController` instance after you register to `Mod Options`. Store it somewhere and interact with `Mod Options` through it.

### Properties

| Name             | Type     | Detail                                   |
| ---------------- | -------- | ---------------------------------------- |
| Name `getter`    | `string` | The translated name of your mod tab.     |
| Tooltip `getter` | `string` | The translated tooltip for your mod tab. |

### Event

| Name      | Type                      | Detail                                         |
| --------- | ------------------------- | ---------------------------------------------- |
| OnBuildUI | `Action<OptionUIBuilder>` | Fires after finish creating your pre-build UI. |

### Methods

```c#
void SetModTooltipId(string tooltipId);
```

Change the translation ID of the tooltip for your mod tab.

| Name      | Type     | Detail                                          |
| --------- | -------- | ----------------------------------------------- |
| tooltipId | `string` | Translation ID of the tooltip for your mod tab. |

```c#
void SetTranslation(string langCode, string id, string trans);
void SetTranslation(string id, string en = null, string jp = null, string cn = null);
```

Add a translation for your mod.

| Name     | Type     | Detail                                                 |
| -------- | -------- | ------------------------------------------------------ |
| langCode | `string` | The locale code for this translation.                  |
| id       | `string` | Translation ID of this translation.                    |
| trans    | `string` | Translated text for `id`.                              |
| en       | `string` | Translated text for `id` with the locale code is `EN`. |
| jp       | `string` | Translated text for `id` with the locale code is `JP`. |
| cn       | `string` | Translated text for `id` with the locale code is `CN`. |

```c#
string Tr(string contentId);
string Tr(string contentId, params string[] args);
```

Get the translation of `contentId`.

| Name      | Type       | Detail                                            |
| --------- | ---------- | ------------------------------------------------- |
| contentId | `string`   | The translation ID.                               |
| args      | `string[]` | Replace `{1}`, `{2}`, ... in the translated text. |

> [!WARNING]
> Currently, `string Tr(string contentId, params string[] args)` will translate `args` automatically. This behavior will be removed in the version.

```c#
void SetPreBuildWithXml(string xml);
```

Set the pre-build UI through `XML`.

| Name | Type     | Detail             |
| ---- | -------- | ------------------ |
| xml  | `string` | The `XML` content. |

```c#
void SetTranslationsFromXslx(string path);
```

Add translations from `XSLX`.

| Name | Type     | Detail                       |
| ---- | -------- | ---------------------------- |
| path | `string` | The path of the `XSLX` file. |

![Translation XSLX](/doc/assets/excel.png)

# OptionUIBuilder

When the `OnBuildUI` event fires, you can create or modify your UI widgets through an `OptionUIBuilder` instance while its valid.

### Properties

| Name           | Type         | Detail                                         |
| -------------- | ------------ | ---------------------------------------------- |
| Valid `getter` | `bool`       | Whether you can interact with the UI elements. |
| Root `getter`  | `OptVLayout` | The root layout of your mod tab.               |

### Method

```c#
T GetPreBuild<T>(string id) where T : OptUIElement;
```

Get a pre-build widget instance.

| Name | Type     | Detail                          |
| ---- | -------- | ------------------------------- |
| id   | `string` | The ID of the pre-build widget. |
