using UnityEngine.UI;

namespace EvilMask.Elin.ModOptions.UI;

public sealed class OptionUIBuilder
{

    public bool Valid { get => !Deleted; }
    public OptVLayout Root { get; set; }

    public T GetPreBuild<T>(string id) where T : OptUIElement
    {
        if (!Valid) return null;
        if (!Controller.PreBuildElements.TryGetValue(id, out var elmt)) return null;
        // if (elmt is not T) return null;
        return elmt as T;
    }

    #region internal
    internal OptionUIBuilder(ContentConfigModOptions modOptions)
    {
        Config = modOptions;
        Root = new()
        {
            Layout = Config.ContentRect.GetComponent<VerticalLayoutGroup>(),
            Element = Config.ContentRect.GetComponent<LayoutElement>(),
            Builder = this
        };
    }
    internal ContentConfigModOptions Config { get; set; }
    internal ModOptionController Controller { get; set; }
    internal bool Deleted { get; set; } = false;
    #endregion
}
