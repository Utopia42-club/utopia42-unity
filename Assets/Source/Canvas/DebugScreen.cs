using UnityEngine;
using UnityEngine.UI;

namespace Source.Canvas
{
    public class DebugScreen : MonoBehaviour
    {
        private Player player;
        private Text text;
        private World world;

        private void Start()
        {
            text = GetComponent<Text>();
            world = GameObject.Find("World").GetComponent<World>();
            player = GameObject.Find("Player").GetComponent<Player>();
        }

        private void Update()
        {
            text.text = player.GetPosition().ToString();
        }
    }
}