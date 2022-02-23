using TMPro;
using UnityEngine;

public class Shortcut : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public string shortcut;

    void Start()
    {
        SetShortcut(shortcut);
    }

    public void SetShortcut(string text)
    {
        textMeshPro.SetText(text);
    }
}