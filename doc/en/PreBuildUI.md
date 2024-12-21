# Pre-build UI

You can tell `Mod Options` how to create your configuration tab through [`XML` file ](#xml-file) or through [reflection](#reflection).

# XML file

You can write your `XML` in different languages. All tags and most attributes have English, Japanese, and Chinese versions.

Remember to reference [the XML schema](/doc/ConfigLayoutSchema.xsd) when you create your `XML` layout file if your text editor supports it.

## Common Element Attributes

Every element has attributes below except `<config>`. All attributes are optional.

| Name                       | Type             | Default | Detail                  |
| -------------------------- | ---------------- | ------- | ----------------------- |
| `id`                       | `string`         | null    | ID [^1] of this widget. |
| `width` / `幅` / `宽度`    | `int` or `float` | null    | Width hint [^2].        |
| `height` / `高さ` / `高度` | `int`            | null    | Height hint [^3].       |

## Elements

### conig / 設定 / 设置

`<config>` is the root of the file, and also represents an `OptVLayout`.

### vlayout / 縦組み / 竖排

`<vlayout>` represents an `OptVLayout`. Its child elements are arranged vertically and their height grows depending on their parent.

A `<vlayout>` with a border and title can represent a configuration group.

![OptVLayout with border image](/doc/assets/group.png)

#### Extra Attributes

| Name                     | Type   | Default | Detail                           |
| ------------------------ | ------ | ------- | -------------------------------- |
| `border` / `枠` / `边框` | `bool` | false   | Whether the layout has a border. |

#### Available Children

| Name                            | Detail                                                                     |
| ------------------------------- | -------------------------------------------------------------------------- |
| `<title>`/`<タイトル>`/`<标题>` | Only works when `border` is `true`. Must be the first child.               |
| `<vlayout>`                     | See [vlayout / 縦組み / 竖排](#vlayout--縦組み--竖排).                     |
| `<hlayout>`                     | See [hlayout / 横組み / 横排](#hlayout--横組み--横排).                     |
| `<text>`                        | See [text / 文字 / 文本](#text--文字--文本).                               |
| `<topic>`                       | See [topic / トピック / 主题](#topic--トピック--主题).                     |
| `<slider>`                      | See [slider / スライダー / 滑动条](#slider--スライダー--滑动条).           |
| `<toggle>`                      | See [toggle / スイッチ / 开关](#toggle--スイッチ--开关).                   |
| `<button>`                      | See [button / ボタン / 按钮](#button--ボタン--按钮).                       |
| `<one_choice>`                  | See [one_choice / 単一選択 / 单选](#one_choice--単一選択--单选).           |
| `<t_group>`                     | See [t_group / 選択肢グループ / 选项组](#t_group--選択肢グループ--选项组). |


### hlayout / 横組み / 横排

`<hlayout>` represents an `OptHLayout`. Its child elements are arranged horizontally and their width is managed by their parent.

#### Available Children

| Name           | Detail                                                                     |
| -------------- | -------------------------------------------------------------------------- |
| `<vlayout>`    | See [vlayout / 縦組み / 竖排](#vlayout--縦組み--竖排).                     |
| `<hlayout>`    | See [hlayout / 横組み / 横排](#hlayout--横組み--横排).                     |
| `<text>`       | See [text / 文字 / 文本](#text--文字--文本).                               |
| `<topic>`      | See [topic / トピック / 主题](#topic--トピック--主题).                     |
| `<slider>`     | See [slider / スライダー / 滑动条](#slider--スライダー--滑动条).           |
| `<toggle>`     | See [toggle / スイッチ / 开关](#toggle--スイッチ--开关).                   |
| `<button>`     | See [button / ボタン / 按钮](#button--ボタン--按钮).                       |
| `<one_choice>` | See [one_choice / 単一選択 / 单选](#one_choice--単一選択--单选).           |
| `<t_group>`    | See [t_group / 選択肢グループ / 选项组](#t_group--選択肢グループ--选项组). |

### text / 文字 / 文本

`<text>` represents an `OptHLabel` — simply plain text.

#### Extra Attributes

| Name                       | Type                 | Default                  | Detail          |
| -------------------------- | -------------------- | ------------------------ | --------------- |
| `size` / `サイズ` / `字号` | [14, 16, 18, 20]     | 16                       | Font size.      |
| `color` / `色` / `颜色`    | #RRGGBB or #RRGGBBAA | null                     | Text color.     |
| `align` / `整列` / `对齐`  | TextAlignment [^4]   | middle / 中央揃え / 居中 | Text alignment. |


### topic / トピック / 主题

`<topic>` represents an `OptTopic`. A special text that shows the configuration topic.

![Topic image](/doc/assets/topic.png)

### slider / スライダー / 滑动条

`<slider>` represents an `OptSlider`. Players can change a value in a specified range. It has two variations.

![Slider image](/doc/assets/slider1.png)

![Slider with buttons image](/doc/assets/slider2.png)

#### Extra Attributes

| Name                            | Type  | Default | Detail                                 |
| ------------------------------- | ----- | ------- | -------------------------------------- |
| `buttons` / `ボタン式` / `按钮` | bool  | false   | Whether the widget has buttons.        |
| `min` / `最小値` / `最小值`     | float | 0       | The minimal value.                     |
| `max` / `最大値` / `最大值`     | float | 1       | The maximum value.                     |
| `value` / `値` / `值`           | float | 0       | The current value.                     |
| `step` / `ステップ` / `步长`    | float | 0.1     | The amount changes on buttons clicked. |

### toggle / スイッチ / 开关

`<toggle>` represents an `OptToggle`.

![Toggle image](/doc/assets/toggle.png)

#### Extra Attributes

| Name                            | Type | Default | Detail                             |
| ------------------------------- | ---- | ------- | ---------------------------------- |
| `checked` / `チェック` / `勾选` | bool | false   | Whether the variation has buttons. |

#### Available Children

| Name               | Detail                                            |
| ------------------ | ------------------------------------------------- |
| `<contentId>` [^5] | Essential. The translation ID of the text.        |
| `<tooltip>`        | Optional. The translation ID of the tooltip text. |

### button / ボタン / 按钮

`<button>` represents an `OptButton`.

![Button image](/doc/assets/button.png)

#### Available Children

| Name               | Detail                                            |
| ------------------ | ------------------------------------------------- |
| `<contentId>` [^5] | Essential. The translation ID of the text.        |
| `<tooltip>`        | Optional. The translation ID of the tooltip text. |

### one_choice / 単一選択 / 单选

`<one_choice>` represents an `OptDropdown`

![Dropdown image](/doc/assets/dropdown.png)

or an `OptLRSelect`.

![ButtonLR image](/doc/assets/buttonLR.png)

#### Extra Attributes

| Name                       | Type               | Default                          | Detail       |
| -------------------------- | ------------------ | -------------------------------- | ------------ |
| `type` / `タイプ` / `样式` | SelectionType [^6] | `dropdown / 引き出し / 下拉菜单` | Widget type. |

#### Available Children

| Name       | Detail                                                                                             |
| ---------- | -------------------------------------------------------------------------------------------------- |
| `<title>`  | Optional. The translation ID of the text.                                                          |
| `<choice>` | Contains at least one. Choices of the widget. See [choice / 選択肢 / 选项](#choice--選択肢--选项). |

#### choice / 選択肢 / 选项

`<choice>` represents a choice of `OptDropdown`, `OptLRSelect` or `OptToggleGroup`.

##### Available Children

| Name               | Detail                                            |
| ------------------ | ------------------------------------------------- |
| `<contentId>` [^5] | Essential. The translation ID of the text.        |
| `<tooltip>`        | Optional. The translation ID of the tooltip text. |

### t_group / 選択肢グループ / 选项组

`<t_group>` represents an `OptToggleGroup`. A combination of multiple `OptToggle`.

![Toggle group image](/doc/assets/choices.png)

#### Extra Attributes

| Name                                  | Type               | Default | Detail           |
| ------------------------------------- | ------------------ | ------- | ---------------- |
| `min_count` / `選択下限` / `选择下限` | Non-negative `int` | 1       | Minimal choices. |
| `max_count` / `選択下限` / `选择下限` | Positive `int`     | 1       | Maximum choices. |

#### Available Children

| Name       | Detail                                                                                             |
| ---------- | -------------------------------------------------------------------------------------------------- |
| `<title>`  | Optional. The translation ID of the text.                                                          |
| `<choice>` | Contains at least one. Choices of the widget. See [choice / 選択肢 / 选项](#choice--選択肢--选项). |

# Reflection

You can place the `ModCfg` attributes on you `ConfigEntry<T>` declarations to tell `Mod Options` how to build a configuration widget for it.

```C#
[ModCfgToggle]
ConfigEntry<bool> is_ready;
[ModCfgSlider(max: 100)]
ConfigEntry<int> int_cfg;
[ModCfgSlider(max: 100, step: 10, buttons: true)]
ConfigEntry<float> float_cfg;

enum TestEnum { Value1, Value2 };

[ModCfgDropdown]
ConfigEntry<TestEnum> enum_cfg;
```

Currently, it doesn't support static fields.

> [!IMPORTANT]
> Don't forget to add instances that contain fields with `ModCfg` attributes in the registration. See [Register](/doc/en/Register.md).

You can set related IDs for every widget by setting the following properties.

| Name     | Type     | Defualt | Detail                                                                               |
| -------- | -------- | ------- | ------------------------------------------------------------------------------------ |
| `Parent` | `string` | null    | The layout ID [^1] which this widget should add to.<br>Add to the root if it's null. |
| `Id`     | `string` | null    | The ID [^1] of this widget.                                                          |

Example:

```C#
[ModCfgSlider(max: 100, Id = "myIntWidget")]
ConfigEntry<int> int_cfg;
```

## ModCfgSlider

Create a slider for a config entry. Supports `int`, `long` and `float`.

### Parameters

| Name      | Type     | Defualt | Detail                                 |
| --------- | -------- | ------- | -------------------------------------- |
| `titleId` | `string` | null    | The translation ID of the title.       |
| `min`     | `float`  | 0       | The minimal value.                     |
| `max`     | `float`  | 1       | The maximum value.                     |
| `step`    | `float`  | 0.1     | The amount changes on buttons clicked. |
| `buttons` | `bool`   | false   | Whether the widget has buttons.        |

## ModCfgToggle

Create a toggle for a config entry. Supports `bool`.

### Parameters

| Name        | Type     | Defualt | Detail                             |
| ----------- | -------- | ------- | ---------------------------------- |
| `titleId`   | `string` | null    | The translation ID of the text.   |
| `tooltipId` | `string` | null    | The translation ID of the tooltip. |

## ModCfgDropdown

Create a dropdown with a label on the left for a config entry. Supports `enum`. The translation ID of each choice is `EnumName.MemberName`.

Example:

```C#
enum TestEnum
{
    Value1, // The translation ID is TestEnum.Value1
    Value2, // The translation ID is TestEnum.Value2
};

[ModCfgDropdown]
ConfigEntry<TestEnum> enum_cfg;
```
### Parameters

| Name        | Type     | Defualt | Detail                             |
| ----------- | -------- | ------- | ---------------------------------- |
| `titleId`   | `string` | null    | The translation ID of the label.   |

[^1]: Non-empty string. You can get a pre-created widget by this id in the `OnBuildUI` callback.
[^2]: Integer value represents `preferedWitdth` and float value represents `flexibleWidth` of the `LayoutElement` component. Works only when the parent of the element is an `OptHLayout`.
[^3]: Positivce integer value, represents `preferedHeight` of the `LayoutElement` component. Works only when the parent of the element is an `OptVLayout`.
[^4]: One of `left / 左揃え / 中央揃え`, `middle / 中央揃え / 居中`, `right / 右揃え / 右对齐`.
[^5]: A string of serials of alphanumeric and underscore separated by dots.
[^6]: One of `dropdown / 引き出し / 下拉菜单`, `lr_select / 左右式 / 左右选择`.
