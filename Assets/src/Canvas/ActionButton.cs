using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    private bool pressed;
    private Image image;
    private Color orgColor;
    
    [SerializeField] public Color pressedColor = Color.gray;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        orgColor = image.color;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pressed = true;
        image.color = pressedColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressed = false;
        image.color = orgColor;
    }

    public bool isPressed()
    {
        return pressed;
    }

}