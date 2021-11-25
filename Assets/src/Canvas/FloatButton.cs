using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace src.Canvas
{
    public class FloatButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        private bool pressed;
        private bool selected;

        private Image image;
        private Color orgColor;

        [SerializeField] public Color pressedColor = Color.gray;
        [SerializeField] public Color selectedColor = Color.green;


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
            selected = !selected;
            image.color = selected ? selectedColor : orgColor;
        }

        public bool isPressed()
        {
            return pressed;
        }

        public bool isSelected()
        {
            return selected;
        }

    }
}
