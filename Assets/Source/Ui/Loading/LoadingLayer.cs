using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Loading
{
    public class LoadingLayer : MonoBehaviour
    {
        private static int lastId;
        private static readonly Dictionary<int, Tuple<VisualElement, VisualElement>> loadingLayers = new();
        private static VisualElement defaultElement;

        private void OnEnable()
        {
            defaultElement = GetComponent<UIDocument>().rootVisualElement;
        }

        public static LoadingController Show()
        {
            return Show(defaultElement);
        }

        public static LoadingController Show(VisualElement root)
        {
            foreach (var keyValuePair in loadingLayers)
            {
                if (keyValuePair.Value.Item1 == root && keyValuePair.Key != lastId)
                {
                    loadingLayers.Add(lastId, keyValuePair.Value);
                    return new LoadingController(lastId++);
                }
            }

            var layer = new LoadingDots();
            // var layer = Utils.Utils.Create("Ui/LoadingLayer/Loading");
            // var s = layer.style;
            // s.position = Position.Absolute;
            // s.top = s.left = s.right = s.bottom = 0;
            // s.flexGrow = 1;
            loadingLayers.Add(lastId, new Tuple<VisualElement, VisualElement>(root, layer));
            root.Add(layer);
            return new LoadingController(lastId++);
        }

        public static void Hide(int id)
        {
            if (!loadingLayers.TryGetValue(id, out var entry))
                return;
            var (root, layer) = entry;
            loadingLayers.Remove(id);
            if (!loadingLayers.Any(keyValuePair => keyValuePair.Value.Item1 == root && keyValuePair.Key != id))
            {
                if (layer.parent != null)
                    root.Remove(layer);
            }
        }
    }
}