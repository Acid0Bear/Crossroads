using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSlideRotation : MonoBehaviour
{
    public Transform CameraPivot;
    public RectTransform settings;
    public GameObject ColorCanvas;
    public GameObject LoginPanel;
    public float RotationSpeed;
    float AddRot;
    public int DivideTo;
    void Start()
    {
        
    }
    private void OnMouseDrag()
    {
        if (settings.gameObject.activeSelf || ColorCanvas.activeSelf || LoginPanel.activeSelf)
            return;
        AddRot = Input.GetAxis("Mouse X") * RotationSpeed * Mathf.Deg2Rad;
        AddRot *= -1;
    }

    void Update()
    {
        float CurRot = CameraPivot.localRotation.eulerAngles.y;
        if(AddRot > 0)
        {
            CameraPivot.localRotation = Quaternion.RotateTowards(CameraPivot.localRotation, Quaternion.Euler(0, AddRot + CurRot, 0), AddRot);
            AddRot = (AddRot > 0) ? AddRot - 0.01f : 0;
        }
        else if (AddRot < 0)
        {
            CameraPivot.localRotation = Quaternion.RotateTowards(CameraPivot.localRotation, Quaternion.Euler(0, AddRot + CurRot, 0), -AddRot);
            AddRot = (AddRot < 0) ? AddRot + 0.01f : 0;
        }
    }
}
