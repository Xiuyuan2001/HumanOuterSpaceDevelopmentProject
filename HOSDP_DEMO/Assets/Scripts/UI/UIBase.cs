using UnityEngine;
using System.Collections.Generic;

public abstract class UIBase : MonoBehaviour
{
    internal void SetVisible(bool visible) => gameObject.SetActive(visible);
}
