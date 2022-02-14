using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotLight : MonoBehaviour
{
    public Light spotLight;

    void Start()
    {
        spotLight.range = 0;
    }

    void Update()
    {
        if (Input.GetButtonDown("Light"))
        {
            spotLight.range = spotLight.range > 0 ? 0 : 20;
        }
    }
}