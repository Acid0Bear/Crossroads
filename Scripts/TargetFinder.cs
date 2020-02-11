using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Cars
{
    [RequireComponent(typeof(RectTransform))]
    public class TargetFinder : MonoBehaviour
    {
        CarController carController;
        public static TargetFinder targetFinder;
        public Camera carCamera;
        public Canvas canvas;
        public TextMeshProUGUI ToResp;
        public float PosDiff = 0.05f;
        public RawImage dot;
        public Slider Hp;
        Vector3 Offset = Vector3.zero;
        public GameObject Player, Destroyed;

        private void Awake()
        {
            if (targetFinder == null)
            {
                targetFinder = this;
            }
            else
            {
                Destroy(this);
            }
        }
        void Start()
        {
            carController = CarController.Carcontroller;
            canvas.worldCamera = carCamera;
            Vector3 pos = carController.Carpos.position;
            carCamera.transform.position = new Vector3(pos.x, pos.y, pos.z);
        }

        void LateUpdate()
        {
            if (carController == null) return;
            carController.RotateCenter();
            Vector3 pos = carController.Carpos.position;
            Hp.value = carController.health.health;
            //carCamera.transform.position = Vector3.MoveTowards(carCamera.transform.position,new Vector3(pos.x, pos.y, pos.z ), carController.GetComponent<Rigidbody>().velocity.normalized.magnitude);
            carCamera.transform.position = pos;
            //carCamera.transform.rotation = Quaternion.Slerp(carCamera.transform.rotation, Quaternion.LookRotation(carController.Camtarget.position - carCamera.transform.position), 1);
            carCamera.transform.LookAt(carController.Camtarget.position);
            if (carController.insideMe.Count != 0 && Ammunition.ammunition.curWeap != 1)
            {
                if (carController.insideMe[0] != null && carController.insideMe[0].GetComponent<Health>().health > 0)
                    Player = carController.insideMe[0];
                else
                    carController.insideMe.Clear();
                if (Player != null && Ammunition.ammunition.curWeap!=0)
                {
                    var newPos = worldToUISpace(canvas, Player.transform.position) + Offset;
                    dot.transform.position = (Vector3.Distance(dot.transform.position, newPos) < PosDiff) ? dot.transform.position : newPos;
                }
                else
                    carController.insideMe.Clear();
            }
            else
            {
                dot.transform.localPosition = new Vector3(0, 65, 0);
                Player = null;
            }

        }



        public Vector3 worldToUISpace(Canvas parentCanvas, Vector3 worldPos)
        {
            //Convert the world for screen point so that it can be used with ScreenPointToLocalPointInRectangle function
            Vector3 screenPos = carCamera.WorldToScreenPoint(worldPos);
            Vector2 movePos;

            //Convert the screenpoint to ui rectangle local point
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, screenPos, parentCanvas.worldCamera, out movePos);
            //Convert the local point to world point
            return parentCanvas.transform.TransformPoint(movePos);
        }
    }
}
