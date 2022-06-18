using UnityEngine;
using UnityEngine.UI;

namespace Source.Canvas
{
    public class DebugScreen : MonoBehaviour
    {
        World world;
        Text text;
        Player player;

        void Start()
        {
            text = GetComponent<Text>();
            world = GameObject.Find("World").GetComponent<World>();
            player = GameObject.Find("Player").GetComponent<Player>();
        }

        void Update()
        {
            text.text = player.GetPosition().ToString();
        }
    }
}
