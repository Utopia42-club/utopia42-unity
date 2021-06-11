using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    [SerializeField]
    private Text textComponent;

    public void UpdateText(string text)
    {
        this.textComponent.text = text;
    }
}
