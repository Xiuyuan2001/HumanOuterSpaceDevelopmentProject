using UnityEngine;

public sealed class UIManager : MonoBehaviour
{
    public void Show(UIBase view) => view.SetVisible(true);

    public void Hide(UIBase view) => view.SetVisible(false);
}
