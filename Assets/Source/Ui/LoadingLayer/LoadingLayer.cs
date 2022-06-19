using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.LoadingLayer
{
    public class LoadingLayer
    {
        private static int id;
        private static readonly Dictionary<int, Tuple<VisualElement, VisualElement>> loadingLayers = new();

        public static int Show(VisualElement root)
        {
            foreach (var keyValuePair in loadingLayers)
            {
                if (keyValuePair.Value.Item1 == root && keyValuePair.Key != id)
                {
                    loadingLayers.Add(id, keyValuePair.Value);
                    return id++;
                }
            }

            var layer = Resources.Load<VisualTreeAsset>("Ui/LoadingLayer/Loading").CloneTree();
            var s = layer.style;
            s.position = Position.Absolute;
            s.top = s.left = s.right = s.bottom = 0;
            s.flexGrow = 1;
            loadingLayers.Add(id, new Tuple<VisualElement, VisualElement>(root, layer));
            root.Add(layer);
            return id++;
        }

        public static void Hide(int id)
        {
            var (root, layer) = loadingLayers[id];
            loadingLayers.Remove(id);
            if (!loadingLayers.Any(keyValuePair => keyValuePair.Value.Item1 == root && keyValuePair.Key != id))
                root.Remove(layer);
        }
    }
}