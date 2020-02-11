using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Cars
{
    public class Joystick : MonoBehaviour
    {
        public GameObject joystick;
        public Vector3 target_pos;
        public Canvas canvas;
        CarController carController;
        public float ScreenScale;
        public bool IsReady = true;
        void Start()
        {
            carController = CarController.Carcontroller;
            joystick.transform.position = this.transform.position;
            ScreenScale = canvas.transform.localScale.x * 100;
        }

        void Update()
        {
            if (Application.isMobilePlatform && Input.touchCount > 0 && IsReady)
            {
                var Touches = Input.touches;
                for (int TouchID = 0; TouchID < Input.touchCount; TouchID++)
                {
                    Vector3 touch_pos = Touches[TouchID].position;
                    if (touch_pos.x < Screen.width / 1.5f)
                    {
                        StickHandler(touch_pos);
                        break;
                    }
                    else
                    {
                        joystick.transform.position = transform.position;
                        carController.horInput = 0;
                        carController.vertInput = 0;
                    }
                }
            }
            else if (!Application.isMobilePlatform && Input.GetMouseButton(0) && IsReady)
            {
                Vector3 touch_pos = Input.mousePosition;
                if (touch_pos.x < Screen.width / 1.5f)
                {
                    StickHandler(touch_pos);
                }
                else
                {
                    joystick.transform.position = transform.position;
                    carController.horInput = 0;
                    carController.vertInput = 0;
                }
            }
            else
            {
                joystick.transform.position = transform.position;
                carController.horInput = 0;
                carController.vertInput = 0;
            }
        }

        void StickHandler(Vector3 touch_pos)
        {
            target_pos = touch_pos - this.transform.position;
            if (target_pos.magnitude < ScreenScale && touch_pos.x < Screen.width / 1.5f)
            {
                joystick.transform.position = touch_pos;
                Vector3 cur = joystick.transform.localPosition;
                target_pos = (cur - transform.localPosition);
                if (target_pos.x / 100 > 0.35f && target_pos.x / 100 < 0.7f)
                    carController.horInput = 0.5f;
                else if (target_pos.x / 100 > 0.7f)
                    carController.horInput = 1;
                else if (target_pos.x / 100 < -0.35f && target_pos.x / 100 > -0.7f)
                    carController.horInput = -0.5f;
                else if (target_pos.x / 100 < -0.7f)
                    carController.horInput = -1;
                else
                    carController.horInput = 0;
                if (target_pos.y / 100 > -0.25)
                    carController.vertInput = 1;
                else if (target_pos.y / 100 < -0.25)
                    carController.vertInput = -1;
            }
            else if (touch_pos.x < Screen.width / 1.5f)
            {
                target_pos.Normalize();
                target_pos = this.transform.position + target_pos * ScreenScale;
                joystick.transform.position = target_pos;
                Vector3 cur = joystick.transform.localPosition;
                target_pos = (cur - transform.localPosition);
                if (target_pos.x / 100 > 0.35f && target_pos.x / 100 < 0.7f)
                    carController.horInput = 0.5f;
                else if (target_pos.x / 100 > 0.7f)
                    carController.horInput = 1;
                else if (target_pos.x / 100 < -0.35f && target_pos.x / 100 > -0.7f)
                    carController.horInput = -0.5f;
                else if (target_pos.x / 100 < -0.7f)
                    carController.horInput = -1;
                else
                    carController.horInput = 0;
                if (target_pos.y / 100 > -0.25)
                    carController.vertInput = 1;
                else if (target_pos.y / 100 < -0.25)
                    carController.vertInput = -1;
            }
        }
    }
}
