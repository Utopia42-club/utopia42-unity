using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Source.Canvas
{
    public class ActionButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        [SerializeField] public Color pressedColor = Color.gray;
        private Image image;

        private readonly List<UnityAction> listeners = new();
        private Color orgColor;

        // Start is called before the first frame update
        private void Start()
        {
            image = GetComponent<Image>();
            orgColor = image.color;
        }

        // Update is called once per frame
        private void Update()
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