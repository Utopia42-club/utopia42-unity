using TMPro;
using UnityEngine;

public class Shortcut : MonoBehaviour
{
    private TextMeshProUGUI textMeshPro;
    public string shortcut;

    void Start()
    {
        textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
        SetShortcut(shortcut);
    }

    public void SetShortcut(string text)
    {
        textMeshPro.SetText(text);
    }
}