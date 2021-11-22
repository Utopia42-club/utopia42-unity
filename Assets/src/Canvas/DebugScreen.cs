using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
    public static DebugScreen INSTANCE;
    World world;
    public Text text;
    Player player;

    void Start()
    {
        INSTANCE = this;
        text = GetComponent<Text>();
        world = GameObject.Find("World").GetComponent<World>();
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    void Update()
    {
        text.text = player.transform.position.ToString();
    }
}
