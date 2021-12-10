using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotLight : MonoBehaviour
{
    public Light light;

    // Start is called before the first frame update
    void Start()
    {
        light.range = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Light"))
        {
            light.range = light.range > 0 ? 0 : 20;
        }
    }
}