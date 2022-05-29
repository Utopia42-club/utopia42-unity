using System.Linq;
using src;
using UnityEngine;
using UnityEngine.UIElements;

public class UiStateAware : MonoBehaviour
{
    [SerializeField] private GameManager.State[] states;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        GameManager.INSTANCE.stateChange.AddListener(s => root.visible = states.Any(state => s == state));
    }
}