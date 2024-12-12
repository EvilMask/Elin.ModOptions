using EvilMask.Elin.ModOptions.Ext;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace EvilMask.Elin.ModOptions.UI
{
    internal sealed class ContentConfigModOptions : ContentConfig
    {
        private static Sprite m_sp_scrol_bar;
        private static Sprite m_sp_scrol_bar_handle;
        private static Sprite m_outline;
        private static RectTransform m_mod_list;
        private static RectTransform m_options_view;
        private static RectTransform m_slider_proto, m_slider_proto2;
        private static RectTransform m_dropdown_proto;
        private static RectTransform m_button_lr_proto;
        private static RectTransform m_input_proto;

        private static bool m_slider_proto2_fixed = false;
        private bool m_should_build_ui = true;
        private string m_current = string.Empty;

        public RectTransform ContentRect => m_options_view;
        public ModOptionController Controlller => m_current.IsEmpty() ? null : Plugin.Instance.ManagedMods[m_current];
        public UIUninstaller Uninstaller { get; private set; } = null;

        public static void Init(Window window)
        {
            var g_content = window.setting.tabs[0].content;
            var bar = g_content.transform
                .GetChild(1) // Scrollview deafult
                .GetChild(2); // Scrollbar Vertical
            var content = g_content.transform
                .GetChild(1) // Scrollview deafult
                .GetChild(0) // Viewport
                .GetChild(0); // Content
            m_sp_scrol_bar = bar.GetComponent<Image>().sprite;
            m_sp_scrol_bar_handle = bar.transform
                .GetChild(0) // Sliding Area
                .GetChild(0) // Handle
                .GetComponent<Image>().sprite;
            m_outline = content
                .GetChild(0) // Horizontal
                .GetChild(0) // Game Object
                .GetChild(0) // Group lang
                .GetChild(0) // BG
                .GetComponent <Image>().sprite;
            m_slider_proto = content
                .GetChild(0) // Horizontal
                .GetChild(1) // Game Object (1)
                .GetChild(0) // Group music
                .GetChild(2).Rect(); // Slider (3)
            m_slider_proto2 = content
                .GetChild(0) // Horizontal
                .GetChild(1) // Game Object (1)
                .GetChild(1) // Group ui brightness
                .GetChild(2).Rect(); // SliderList
            var itemfont = content
                 .GetChild(1) // Group font
                 .GetChild(2); // ItemFont
            m_dropdown_proto = itemfont.GetChild(1).Rect(); // Dropdwon
            m_button_lr_proto = itemfont.GetChild(2).Rect(); // ButtonLR


            var size = window.RectTransform.sizeDelta;
            window.RectTransform.sizeDelta = new Vector2(size.x + 100.0f, size.y);
            FixWindowSizeInput(window.setting.tabs[1].content.transform);
        }

        public void BuildUI()
        {
            if (!m_slider_proto2_fixed) FixSliderProto2();
            if (!m_should_build_ui) return;
            m_should_build_ui = false;
            Plugin.Log("Building ui...");

            var (rect, _) = AddCommonCompoments(gameObject);
            rect.anchoredPosition = new Vector2(410, -281);
            rect.offsetMin = new Vector2(0, -562);
            rect.offsetMax = new Vector2(820, 0);
            rect.localScale = Vector3.one;
            Uninstaller = gameObject.AddComponent<UIUninstaller>();

            AddLayout(gameObject, false, false);
            (var l_rect, m_mod_list, var l_layout) = AddScrollView(gameObject, "Mod List");
            l_layout.minWidth = 200;
            m_mod_list.parent.transform.Rect().SetOffsetToAnchored(5, 56, 5, 25);
            m_mod_list.GetComponent<VerticalLayoutGroup>().padding.left = -5;

            AddBackground(l_rect).SetOffsetToAnchored(2, 36, 2, 5);

            (var _, m_options_view, var  r_layout) = AddScrollView(gameObject, "Options View", false);
            r_layout.preferredWidth = 100000;

            SetupModList();
            SetupOptionsView();
            Plugin.Log("Finished building ui.");
        }

        public void SetupModList()
        {
            m_mod_list.DestroyChildren(true);
            m_mod_list.parent.parent.SetActive(Plugin.Instance.ManagedMods.Count != 0);
            if (Plugin.Instance.ManagedMods.Count == 0) return;

            m_mod_list.parent.SetActive(true);
            var mods = Plugin.Instance.ManagedMods;
            var list = mods.Keys.ToList();
            list.Sort(Comparer<string>.Default);
            foreach (var item in list)
            {
                var name = mods[item].Name;
                var tooltip = mods[item].Tooltip;
                var button = AddToggle(name);
                button.transform.SetParent(m_mod_list);
                button.gameObject.AddComponent<LayoutElement>().preferredHeight = 30;
                button.name = $"Mod {name}";
                button.GetComponentInDirectChildren<Image>().SetActive(false);
                button.tooltip.icon = true;
                button.mainText.text = name;
                if (tooltip != null)
                {
                    button.tooltip.text = tooltip;
                    button.tooltip.enable = true;
                }
                var t_trans = button.mainText.rectTransform;
                t_trans.anchorMin = new Vector2(0, 0.5f);
                t_trans.anchorMax = new Vector2(1, 0.5f);
                t_trans.SetOffsetToAnchored(10, 0, 0, 0);
                button.onClick.AddListener(delegate ()
                {
                    m_current = item;
                    SetupOptionsView();
                });
            }
        }

        public void SetupOptionsView()
        {
            if ((Controlller?.CleanersForCfg.Count ?? 0) > 0)
            {
                foreach (var cleaner in Controlller.CleanersForCfg) cleaner();
                Controlller.CleanersForCfg.Clear();
            }
            m_options_view.DestroyChildren(true);
            Controlller?.CreateBuilder(this, RootLayout(m_options_view));
            Controlller?.PreBuildElements.Clear();
            if (Controlller?.PreBuildRoot != null)
                PreBuildLayoutChilds(Controlller.Builder.Root, Controlller.PreBuildRoot.Childs);
            if (Controlller?.PreBuildInfosFromReflection.Count > 0)
                PreBuildUIFromReflections(Controlller.Builder.Root);
            Controlller?.BuildUI();
            if (m_current == string.Empty || m_options_view.transform.childCount == 0)
            {
                CreateText(m_options_view, ModInfo.NameAndVersion, size: 20);
                if (Plugin.Instance.ManagedMods.Count == 0) 
                    CreateText(m_options_view, UILang.NoManagedMod.tr(), size: 20);
                m_options_view.pivot = new Vector2(0.5f, 0.5f);
                Build();
                return;
            }
            else m_options_view.pivot = new Vector2(0.5f, 1);
            // Don't know why but the scrollview will go wrong without this:
            m_options_view.SetOffsetToAnchored(0, 0, 0, 0);
        }

        private OptVLayout RootLayout(RectTransform parent)
        {
            OptVLayout elmt = new()
            {
                Layout = parent.GetComponent<VerticalLayoutGroup>(),
            };
            elmt.Element = elmt.Layout.GetComponent<LayoutElement>();
            return elmt;
        }

        private void PreBuildUIFromReflections(OptLayout parent)
        {
            Func<string, string> tr = Controlller.Tr;
            foreach(var item in Controlller.PreBuildInfosFromReflection)
            {
                if (item.Parent != null && !Controlller.PreBuildElements.TryGetValue(item.Parent, out var elmt))
                {
                    if (elmt is not OptLayout layout)
                        Plugin.Warn($"{item.Parent} is not an OptLayout. Add to root ...");
                    else parent = layout;
                }
                var parentIsVLayout = parent is OptVLayout;
                switch (item.Info)
                {
                    case SliderInfo slider:
                        {
                            var e = parent.AddSlider($"{tr(slider.TitleId)} ({item.GetValue()})", slider.Buttons, slider.Min, slider.Max, slider.Value);
                            e.Step = slider.Step;
                            e.OnValueChanged += _ => e.Title = $"{tr(slider.TitleId)} ({item.GetValue()})";
                            CheckIdAndStorePreBuild(e, slider, parentIsVLayout);
                            Controlller.CleanersForCfg.Add(item.Connector(e));
                        }
                        break;
                    case SelectionsInfo dropdown:
                        {
                            var p = parent.AddHLayout();
                            p.Base.childScaleWidth = true;
                            var t = p.AddText(tr(dropdown.TitleId), TextAnchor.MiddleLeft, 14);
                            t.Element.Rect().pivot = new Vector2(0, .5f);
                            t.PrefferedWidth = .5f;
                            var e = p.AddDropdown(dropdown.Selections.Select(v=>tr(v.Item1)).ToList(), dropdown.Current);
                            e.PrefferedWidth = .5f;
                            CheckIdAndStorePreBuild(e, dropdown, false);
                            Controlller.CleanersForCfg.Add(item.Connector(e));
                        }
                        break;
                    case ToggleInfo toggle:
                        {
                            var e = parent.AddToggle(tr(toggle.ContentId), toggle.Checked, tooltip: tr(toggle.TooltipId));
                            if (toggle.Id != null && !Controlller.PreBuildElements.ContainsKey(toggle.Id))
                                Controlller.PreBuildElements.Add(toggle.Id, e);
                            Controlller.CleanersForCfg.Add(item.Connector(e));
                        }
                        break;
                }
            }
        }

        private void PreBuildLayoutChilds(OptLayout parent, List<PreBuildInfo> childs)
        {
            Func<string, string> tr = Controlller.Tr;
            var parentIsVLayout = parent is OptVLayout;
            foreach (var child in childs)
            {
                switch(child)
                {
                    case VLayoutInfo v:
                        {
                            var elmt = v.HasBorder ? parent.AddVLayoutWithBorder(tr(v.TitleId), v.Height) : parent.AddVLayout(v.Height);
                            // Plugin.Log("v");
                            CheckIdAndStorePreBuild(elmt, v, parentIsVLayout);
                            PreBuildLayoutChilds(elmt, v.Childs);
                            break;
                        }
                    case HLayoutInfo h:
                        {
                            var elmt = parent.AddHLayout(h.Width);
                            // Plugin.Log("h");
                            CheckIdAndStorePreBuild(elmt, h, parentIsVLayout);
                            PreBuildLayoutChilds(elmt, h.Childs);
                            break;
                        }
                    case TopicInfo topic:
                        {
                            // Plugin.Log("topic");
                            var elmt = parent.AddTopic(tr(topic.ContentId));
                            CheckIdAndStorePreBuild(elmt, topic, parentIsVLayout);
                            break;
                        }
                    case ButtonInfo button:
                        {
                            // Plugin.Log("button");
                            var elmt = parent.AddButton(tr(button.ContentId), tr(button.TooltipId));
                            CheckIdAndStorePreBuild(elmt, button, parentIsVLayout);
                            break;
                        }
                    case ToggleGroupInfo t_group:
                        {
                            // Plugin.Log("t_group");
                            (string, string, bool) ToTri((string, string) ids, int index)
                                => (tr(ids.Item1), tr(ids.Item2), t_group.Current.Contains(index));
                            var selections = t_group.Selections.Select(ToTri).ToArray();
                            var elmt = parent.AddToggleGroup(tr(t_group.TitleId), t_group.MinSelect, t_group.MaxSelect, items: selections);
                            CheckIdAndStorePreBuild(elmt, t_group, parentIsVLayout);
                            break;
                        }
                    case ToggleInfo toggle:
                        {
                            // Plugin.Log("toggle");
                            var elmt = parent.AddToggle(tr(toggle.ContentId), toggle.Checked, tooltip: tr(toggle.TooltipId));
                            CheckIdAndStorePreBuild(elmt, toggle, parentIsVLayout);
                            break;
                        }
                    case SelectionsInfo selects:
                        {
                            // Plugin.Log("selects");
                            OptUIElement elmt = selects.IsDropdown
                                ? parent.AddDropdown(selects.Selections.Select(v => (tr(v.Item1), tr(v.Item2))).ToList(), selects.Current)
                                : parent.AddLRSelect(selects.Selections.Select(v => (tr(v.Item1), tr(v.Item2))).ToList(), selects.Current);
                            CheckIdAndStorePreBuild(elmt, selects, parentIsVLayout);
                            break;
                        }
                    case InputInfo input:
                        {
                            // Plugin.Log("input");
                            OptInput elmt = parent.AddInput(input.Value, input.Placeholder, input.Limit, input.Validation);
                            CheckIdAndStorePreBuild(elmt, input, parentIsVLayout);
                            break;
                        }
                    case SliderInfo slider:
                        {
                            // Plugin.Log("slider");
                            OptSlider elmt = parent.AddSlider(tr(slider.TitleId), slider.Buttons, slider.Min, slider.Max, slider.Value);
                            CheckIdAndStorePreBuild(elmt, slider, parentIsVLayout);
                            break;
                        }
                    case TextInfo text:
                        {
                            // Plugin.Log("text");
                            OptLabel elmt = parent.AddText(tr(text.ContentId), text.Anchor, text.Size ?? 16, text.Color);
                            CheckIdAndStorePreBuild(elmt, text, parentIsVLayout);
                            break;
                        }
                }
            }
        }

        private void CheckIdAndStorePreBuild(OptUIElement elmt, PreBuildInfo info, bool parentIsVLayout)
        {
            if (info.Id != null && !info.Id.IsEmpty() && !Controlller.PreBuildElements.ContainsKey(info.Id))
                Controlller.PreBuildElements.Add(info.Id, elmt);
            if (parentIsVLayout)
                elmt.PrefferedHeight = info.Height ?? -1;
            else if (info.Width != null)
                elmt.PrefferedWidth = info.Width.Value;
        }

        // Fix the position shift in the [Graphics] tab after expend the config window.
        private static void FixWindowSizeInput(Transform content)
        {
            var group = content.GetChild(1) // Scrollview default
                .GetChild(0) // Viewport
                .GetChild(0) // Content
                .GetChild(0) // Horizontal (1)
                .GetChild(1) // Vertical (1)
                .GetChild(0) // Group screen
                .GetChild(5);// Game Object
            m_input_proto = group.GetChild(0).Rect();  // Input Field, save it as proto.
            m_input_proto.SetOffsetToAnchored(-141, -3.8f, 81, -26.2f);
            group.GetChild(1).Rect().SetOffsetToAnchored(-56.2f, -3.8f, -3.8f, -26.2f); // Input Field (1)
            group.GetChild(5).Rect().SetOffsetToAnchored(9.7f, -9, -99.7f, -31); // ButtonMain
        }
        private void FixSliderProto2()
        {
            m_slider_proto2 = Instantiate(m_slider_proto2);
            m_slider_proto2.transform.SetParent(transform);
            m_slider_proto2.gameObject.SetActive(false);
            var slider = m_slider_proto2.transform.GetChild(4).GetComponent<UISlider>();
            slider.textInfo = null;
            slider.textMain = null;
            var t1 = m_slider_proto2.GetChild(2); // unused UIText
            var t2 = m_slider_proto2.GetChild(3); // unused UIText (1)
            DestroyImmediate(t1.gameObject);
            DestroyImmediate(t2.gameObject);
        }

        internal UIToggleGroup CreateToggleGroup(RectTransform parent, string title, int min = 1, int max = 1, int size = 16, params (string, string, bool)[] items)
        {
            if (items == null) return null;
            var (rect, elmt) = CreateElement(parent, "ToggleGroup");
            var group = rect.gameObject.AddComponent<UIToggleGroup>();
            var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            group.Min = min.Clamp(0, items.Length);
            group.Max = max.Clamp(min, items.Length);
            if (title != null) CreateTopic(rect, title);
            if (items == null) return group;
            for (int i = 0; i < items.Length; i++)
            {
                var idx = i;
                var item = items[i];
                var toggle = CreateToggle(rect, item.Item1, false, size, item.Item2);
                toggle.onClick.AddListener(() =>
                {
                    group.SetToggle(idx, toggle.isChecked);
                    // group.RebuildLayout(true);
                });
                group.AddToggle(toggle);
            }
            for (int i = 0; i < items.Length; i++)
                if (items[i].Item3) group.SetToggle(i, items[i].Item3, true);
            return group;
        }

        internal UIButton CreateButton(RectTransform parent, string title, string tooltip = null)
        {
            var button = AddButton(title, () => { });
            button.transform.SetParent(parent);
            button.gameObject.AddComponent<LayoutElement>().minHeight = 52;
            if (tooltip != null)
            {
                button.tooltip.enable = true;
                button.tooltip.text = tooltip;
                button.tooltip.icon = true;
            }
            return button;
        }

        internal VerticalLayoutGroup CreateGroup(RectTransform parent, string title, int? height = null)
        {
            var (rect, elmt) = CreateElement(parent, "Group");
            rect.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding.top = 10;
            layout.padding.bottom = 10;
            layout.padding.left = 10;
            layout.padding.right = 10;
            AddBackground(rect).SetOffsetToAnchored(-8, -8, -8, -8);
            if (title != null) CreateTopic(rect, title);
            if (height != null) elmt.minHeight = height.Value;
            return layout;
        }

        internal UIItem CreateTopic(RectTransform parent, string title)
        {
            var topic = AddTopic(title);
            topic.transform.SetParent(parent);
            topic.gameObject.AddComponent<LayoutElement>().minHeight = 20;
            topic.text1.fontSize = 14;
            topic.text1.alignment = TextAnchor.MiddleLeft;
            return topic;
        }

        internal UIItem CreateText(RectTransform parent, string text, TextAnchor align = TextAnchor.MiddleCenter, int size = 16, Color? color = null)
        {
            var ui_text = AddText(text);
            ui_text.text1.transform.SetParent(parent);
            ui_text.Rect().pivot = new Vector2(0.5f, 1);
            ui_text.text1.alignment = align;
            ui_text.text1.fontSize = size;
            if (color != null) ui_text.text1.color = color.Value;
            return ui_text;
        }

        internal UIButton CreateToggle(RectTransform parent, string text, bool isChecked = false, int size = 16, string tooltip = null)
        {
            var ui_toggle = AddToggle(text, isChecked);
            ui_toggle.Rect().SetParent(parent);
            ui_toggle.Rect().pivot = new Vector2(0, 1);
            ui_toggle.gameObject.AddComponent<LayoutElement>().minHeight = size * 2;
            // ui_toggle.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            ui_toggle.mainText.fontSize = size;
            if (tooltip != null)
            {
                ui_toggle.tooltip.enable = true;
                ui_toggle.tooltip.text = tooltip;
            }
            return ui_toggle;
        }

        internal UISlider CreateSlider(RectTransform parent, string title, bool sideButtons = false)
        {
            var s_rect = Instantiate(sideButtons ? m_slider_proto2 : m_slider_proto);
            var slider = s_rect.transform.GetChild(sideButtons ? 2 : 0).GetComponent<UISlider>();
            s_rect.SetParent(parent);
            s_rect.SetActive(true);
            s_rect.gameObject.AddComponent<LayoutElement>().minHeight = 50;
            // s_rect.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            slider.textMain = slider.GetComponentInChildren<UIText>();
            slider.textMain.text = title;
            return slider;
        }

        internal void SetSliderButtonActions(UISlider slider, Action<float> action)
        {
            var right = slider.transform.parent.GetChild(0).GetComponent<UIButton>();
            var left = slider.transform.parent.GetChild(1).GetComponent<UIButton>();
            left.onClick = new();
            right.onClick = new();
            left.onClick.SetListener(() => action(-1));
            right.onClick.SetListener(() => action(1));
            // left.onClick.AddListener(() => action(-1));
            // right.onClick.AddListener(() => action(1));
        }

        internal UIDropdown CreateDropdown(RectTransform parent, List<(string, string)> values, int current = 0)
        {
            var d_rect = Instantiate(m_dropdown_proto);
            var drop = d_rect.transform.GetComponent<UIDropdown>();
            d_rect.SetParent(parent);
            d_rect.gameObject.AddComponent<LayoutElement>().minHeight = 40;
            // d_rect.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            drop.ClearOptions();
            drop.AddOptions(values.Select(v => v.Item1).ToList());
            drop.value = current;
            var helper = drop.gameObject.AddComponent<SelectionsTooltipData>();
            helper.Tooltips = values.Select(v => v.Item2).ToList();
            var template = d_rect.transform.GetChild(2); // Template
            // The item view order is higher than tooltip by default, so adjust it.
            template.gameObject.AddComponent<Canvas>().sortingOrder =
                TooltipManager.Instance.GetComponent<Canvas>().sortingOrder - 1;
            template.GetChild(0).GetChild(0).GetChild(0) // Viewport/Content/Item
                .gameObject.AddComponent<SelectionsItemTooltipHelper>();
            return drop;
        }

        internal UIButtonLR CreateButtonLR(RectTransform parent, List<(string, string)> values, int current = 0, Action<int> onSelect = null)
        {
            var b_rect = Instantiate(m_button_lr_proto);
            var button = b_rect.transform.GetComponent<UIButtonLR>();
            b_rect.SetParent(parent);
            b_rect.gameObject.AddComponent<LayoutElement>().minHeight = 40;
            button.SetOptions(current, values.Select(v => v.Item1).ToList(), onSelect == null ? onSelect : _ => { });
            button.left.onClick.RemoveAllListeners();
            button.mainText.text = Controlller.Tr(button.options[button.index]);
            var helper = button.transform.GetChild(0).gameObject.AddComponent<ButtonLRTooltipData>();
            helper.Tooltips = values.Select(v => v.Item2).ToList();
            button.left.onClick.AddListener(delegate ()
            {
                button.SelectOption(button.index - 1);
                button.mainText.text = Controlller.Tr(button.options[button.index]);
            });
            button.right.onClick.RemoveAllListeners();
            button.right.onClick.AddListener(delegate ()
            {
                button.SelectOption(button.index + 1);
                button.mainText.text = Controlller.Tr(button.options[button.index]);
            });
            return button;
        }

        internal InputField CreateInput(RectTransform parent, string value = null, string placeholder = null, int lengthLimit = 0, InputField.CharacterValidation validation = InputField.CharacterValidation.None)
        {
            var i_rect = Instantiate(m_input_proto);
            var input = i_rect.transform.GetComponent<InputField>();
            i_rect.SetParent(parent);
            var elmt = i_rect.gameObject.AddComponent<LayoutElement>();
            elmt.minWidth = 100;
            DestroyImmediate(i_rect.GetChild(0).gameObject); // Don't need that, the InputField will create one.
            input.characterLimit = lengthLimit;
            input.characterValidation = validation;
            input.contentType = InputField.ContentType.Standard;
            input.text = value ?? string.Empty;
            input.placeholder.GetComponent<Text>().text = placeholder ?? string.Empty;
            return input;
        }
        internal HorizontalLayoutGroup CreateHLayout(RectTransform parent, PixelOrPercentage? width = null)
        {
            var (rect, elmt) = CreateElement(parent, "HLayout");
            var layout = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 5;
            // rect.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            if (width == null) return layout;
            if (width.Value.IsPixel)
                elmt.preferredWidth = width.Value.Pixel;
            else
                elmt.flexibleWidth = width.Value.Percentage;
            return layout;
        }
        internal VerticalLayoutGroup CreateVLayout(RectTransform parent, int? height = null)
        {
            var (rect, elmt) = CreateElement(parent, "VLayout");
            rect.pivot = new Vector2(0.5f, 1);
            var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            rect.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            if (height != null)  elmt.flexibleWidth = height.Value;
            return layout;
        }

        private (RectTransform, LayoutElement) CreateElement(RectTransform parent, string name)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent);
            var rect = obj.AddComponent<RectTransform>();
            rect.localScale = Vector3.one;
            var element = obj.AddComponent<LayoutElement>();
            return (rect, element);
        }
        #region MO tab build
        private RectTransform AddBackground(RectTransform parent)
        {
            var bg = new GameObject("BG");
            var (bg_trans, bg_layout) = AddCommonCompoments(bg);
            bg_trans.SetParent(parent);
            bg_trans.SetAsFirstSibling();
            bg_layout.ignoreLayout = true;
            bg_trans.anchorMin = Vector2.zero;
            bg_trans.anchorMax = Vector2.one;
            bg_trans.localScale = Vector3.one;
            var img = bg.AddComponent<Image>();
            img.sprite = m_outline;
            img.type = Image.Type.Sliced;
            return bg_trans;
        }
        private (RectTransform, LayoutElement) AddCommonCompoments(GameObject obj)
        {
            return (obj.AddComponent<RectTransform>(), obj.AddComponent<LayoutElement>());
        }
        private Image AddImageRenderer(GameObject obj)
        {
            obj.AddComponent<CanvasRenderer>();
            return obj.AddComponent<Image>();
        }
        private (Scrollbar, Scrollbar) AddScrollBar(GameObject parent, bool scrollBarInside)
        {
            var h_obj = new GameObject("Scrollbar Horizontal");
            var v_obj = new GameObject("Scrollbar Vertical");

            h_obj.AddComponent<RectTransform>().SetParent(parent.transform);
            AddImageRenderer(h_obj);
            var h_bar = h_obj.AddComponent<Scrollbar>();
            var h_img = AddHandle(h_obj, scrollBarInside);
            h_bar.handleRect = h_img.transform as RectTransform;
            h_bar.image = h_img;
            h_bar.value = 1;

            var v_rect = v_obj.AddComponent<RectTransform>();
            v_rect.SetParent(parent.transform);
            var v_back = AddImageRenderer(v_obj);
            v_back.sprite = m_sp_scrol_bar;
            v_back.type = Image.Type.Sliced;
            var v_img = AddHandle(v_obj, scrollBarInside);
            var v_bar = v_obj.AddComponent<Scrollbar>();

            v_rect.pivot = Vector2.one;
            v_rect.anchorMin = new Vector2(1, 0);
            v_rect.anchorMax = Vector2.one;
            if (scrollBarInside)
                v_rect.SetOffsetToAnchored(-25, 50, 15, 20);
            else
                v_rect.SetOffsetToAnchored(3, 26, -23, 0);
            v_rect.localScale = Vector3.one;
            v_bar.direction = Scrollbar.Direction.BottomToTop;
            v_bar.handleRect = v_img.transform as RectTransform;
            v_bar.image = v_img;
            v_img.sprite = m_sp_scrol_bar_handle;
            v_img.type = Image.Type.Sliced;
            v_bar.value = 1;

            return (h_bar, v_bar);
        }
        private (RectTransform, RectTransform) AddViewport(GameObject parent, bool scrollBarInside)
        {
            var view = new GameObject("Viewport");
            var v_rect = view.AddComponent<RectTransform>();
            v_rect.SetParent(parent.transform);
            v_rect.anchorMin = Vector2.zero;
            v_rect.anchorMax = Vector2.one;
            v_rect.localScale = Vector3.one;
            if (scrollBarInside)
                v_rect.SetOffsetToAnchored(5, 56, 5, 25);
            else
                v_rect.SetOffsetToAnchored(10, 33, 10, 5);

            AddImageRenderer(view);
            view.AddComponent<Mask>().showMaskGraphic = false;

            return (v_rect, AddContent(view, scrollBarInside));
        }
        private Image AddHandle(GameObject parent, bool scrollBarInside)
        {
            var obj = new GameObject("Sliding Area");
            var sl_rect = obj.AddComponent<RectTransform>();
            sl_rect.SetParent(parent.transform);
            sl_rect.anchorMin = Vector2.zero;
            sl_rect.anchorMax = Vector2.one;
            sl_rect.localScale = Vector3.one;
            if (scrollBarInside)
                sl_rect.SetOffsetToAnchored(-5, -5, -5, -5);
            else
                sl_rect.SetOffsetToAnchored(0, 0, 0, 0);

            var h_obj = new GameObject("Handle");
            var h_rect = h_obj.AddComponent<RectTransform>();
            h_rect.SetParent(obj.transform);
            h_rect.anchorMin = Vector2.zero;
            h_rect.anchorMax = Vector2.one;
            h_rect.localScale = Vector3.one;
            h_rect.SetOffsetToAnchored(0, 0, 0, 0);

            return AddImageRenderer(h_obj);
        }
        private (RectTransform, RectTransform, LayoutElement) AddScrollView(GameObject parent, string name, bool scrollBarInside = true)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent.transform);

            var (sv_rect, sv_layout) = AddCommonCompoments(obj);
            sv_rect.localScale = Vector3.one;

            obj.AddComponent<UIContent>();
            var s_view = obj.AddComponent<UIScrollView>();
            var (viewport, content) = AddViewport(obj, scrollBarInside);
            s_view.content = content;
            s_view.horizontal = false;
            s_view.normalizedPosition = Vector2.one;
            s_view.viewport = viewport;
            s_view.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            s_view.movementType = ScrollRect.MovementType.Clamped;
            var (h_bar, v_bar) = AddScrollBar(obj, scrollBarInside);
            s_view.horizontalScrollbar = h_bar;
            s_view.verticalScrollbar = v_bar;

            return (sv_rect, content, sv_layout);
        }
        private RectTransform AddContent(GameObject parent, bool scrollBarInside)
        {
            var c = new GameObject("Content");
            var trans = c.AddComponent<RectTransform>();
            trans.SetParent(parent.transform);
            trans.pivot = new Vector2(0.5f, 1);
            trans.anchorMin = new Vector2(0, 1);
            trans.anchorMax = Vector2.one;
            trans.localScale = Vector3.one;
            var (_, layout) = AddLayout(c);
            if (scrollBarInside)
                trans.SetOffsetToAnchored(0, 0, 20, 0);
            else
            {
                trans.SetOffsetToAnchored(0, 0, 0, 0);
                layout.padding.top = 11;
                layout.padding.bottom = -20;
            }
            return trans;
        }
        private (ContentSizeFitter, LayoutGroup) AddLayout(GameObject obj, bool vertival = true, bool addFitter = true)
        {
            if (!obj.TryGetComponent<RectTransform>(out var _))
                obj.AddComponent<RectTransform>();
            LayoutGroup layout = vertival ? obj.AddComponent<VerticalLayoutGroup>() : obj.AddComponent<HorizontalLayoutGroup>();
            if (!addFitter) return (null, layout);
            var fitter = obj.AddComponent<ContentSizeFitter>();
            if (vertival) fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            else fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            return (fitter, layout);
        }
        #endregion
    }
}
