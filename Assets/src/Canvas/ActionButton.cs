using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace src.Canvas
{
    public class ActionButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        private Image image;
        private Color orgColor;

        [SerializeField] public Color pressedColor = Color.gray;

        private List<UnityAction> listeners = new List<UnityAction>();

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
            image.color = pressedColor;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            image.color = orgColor;
            foreach (var listener in listeners)
                listener.Invoke();
        }

        public bool isPressed()
        {
            return false;
        }

        public void AddListener(UnityAction action)
        {
            listeners.Add(action);
        }

    }
}
