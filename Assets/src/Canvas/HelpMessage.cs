using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace src.Canvas
{
    public class HelpMessage : MonoBehaviour
    {
        public TextMeshProUGUI textMesh;

        void Start()
        {
            GameManager.INSTANCE.stateChange.AddListener(s =>
            {
                switch (s)
                {
                    case (GameManager.State.PLAYING):
                        gameObject.SetActive(true);
                        textMesh.SetText("F2: Open Settings");
                        break;
                    case (GameManager.State.MAP):
                        gameObject.SetActive(true);
                        textMesh.SetText("F2: Open Side Panel");
                        break;
                    default:
                        gameObject.SetActive(false);
                        textMesh.SetText("");
                        break;
                }
            });
        }
    }
}