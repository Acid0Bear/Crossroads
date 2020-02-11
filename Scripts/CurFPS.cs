using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurFPS : MonoBehaviour
{
    float deltaTime = 0.0f;
    public TextMeshProUGUI sign;


    // Update is called once per frame
    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        sign.text = text;
    }
}
