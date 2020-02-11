using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImgRot : MonoBehaviour
{
    public RectTransform img;
    public float val;
    void Update()
    {
        var rot = img.localRotation.eulerAngles;
        img.Rotate(new Vector3(0, 0, rot.z+1),val);
    }
}
