using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Cars
{
    public class CameraPreventerCollide : MonoBehaviour
    {
        static CameraPreventerCollide CamPrev;
        Vector3 CurPos, InitialPos;
        float z;
        private void Awake()
        {
            if (CamPrev == null)
                CamPrev = this;
            else
                this.enabled = false;
            CurPos = CarController.Carcontroller.Carpos.localPosition;
            InitialPos = CarController.Carcontroller.Carpos.localPosition;
            z = CurPos.z;
        }

        private void Update()
        {
            CarController.Carcontroller.Carpos.localPosition = Vector3.MoveTowards(CarController.Carcontroller.Carpos.localPosition, CurPos, Time.deltaTime*2);
        }
        private void OnTriggerEnter(Collider other)
        {
            CurPos.z = -1.5f;
        }

        private void OnTriggerExit(Collider other)
        {
            CurPos.z = z;
        }
    }
}
