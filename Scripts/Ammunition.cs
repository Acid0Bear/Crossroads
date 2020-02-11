using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Cars
{
    public class Ammunition : MonoBehaviour
    {
        TargetFinder targetFinder;
        public static Ammunition ammunition;
        public Button openFire;
        public Scrollbar WeapSelector;
        public Image OverHeat;
        public Image BoostAmout;
        public List<int> AmmoBase = new List<int>();
        public Transform KillsLog;
        public GameObject armor, onfire, electricuted;
        public Image armImg;
        public GameObject RocketProj;
        public int curWeap;
        public bool IsLocked;
        public List<TextMeshProUGUI> Ammo = new List<TextMeshProUGUI>();
        public RawImage [] weaps = new RawImage[3];
        public Image SpecialDelay;
        private void Awake()
        {
            for (int i = 0; i < 4; i++)
                AmmoBase.Add(0);
            curWeap = 1;
            targetFinder = TargetFinder.targetFinder;
            if (ammunition == null)
            {
                ammunition = this;
            }
            else
            {
                Destroy(this);
            }
        }
        public void UseSpecial()
        {
            CarController.Carcontroller.Weap.Special(false);
        }
        public void UseGadget()
        {
            CarController.Carcontroller.Weap.Gadget(AmmoBase);
        }
        public void ActivateBoost()
        {
            var Hp = CarController.Carcontroller.health;
            if (Hp.Boost == null)
                Hp.Boost = StartCoroutine(Hp.BoostRout());
            else
            {
                CarController.Carcontroller.carSpeed = 750;
                StopCoroutine(Hp.Boost);
                Hp.ManageBoost(false);
                Hp.Boost = null;
            }
        }
        public void WeaponSelector(int num)
        {
            if (IsLocked) return;
            Color Tmp;
            for(int i = 0; i < 4; i++)
            {
                if (i == num - 1) continue;
                Tmp = weaps[i].color;
                Tmp.a = 0;
                weaps[i].color = Tmp;
            }
            Tmp = weaps[num - 1].color;
            Tmp.a = 1;
            weaps[num - 1].color = Tmp;
            curWeap = num;
        }
        private void FixedUpdate()
        {
            #region Selector
            if (curWeap == 1)
            {
                WeaponSelector(1);
                var PV = PhotonView.Get(this);
                PV.RPC("SelectWeap", RpcTarget.All, CarController.Carcontroller.gameObject.GetPhotonView().ViewID, 0);
            }
            else if (curWeap == 2)
            {
                WeaponSelector(2);
                if (targetFinder.Player)
                {
                    CarController.Carcontroller.Weap.Weapons[1].transform.LookAt(targetFinder.Player.transform.position);
                    var Rot = CarController.Carcontroller.Weap.Weapons[1].transform.rotation.eulerAngles;
                    CarController.Carcontroller.Weap.Weapons[1].transform.rotation = Quaternion.Euler(Rot.x - 92, Rot.y, Rot.z);
                }
                else
                    CarController.Carcontroller.Weap.Weapons[1].transform.localRotation = Quaternion.Euler(0, 0, 0);
                var PV = PhotonView.Get(this);
                PV.RPC("SelectWeap", RpcTarget.All, CarController.Carcontroller.gameObject.GetPhotonView().ViewID, 1);
            }
            else if (curWeap == 3)
            {
                WeaponSelector(3);
                if (targetFinder.Player)
                {
                    CarController.Carcontroller.Weap.Weapons[2].transform.LookAt(targetFinder.Player.transform.position);
                    var Rot = CarController.Carcontroller.Weap.Weapons[2].transform.rotation.eulerAngles;
                    CarController.Carcontroller.Weap.Weapons[2].transform.rotation = Quaternion.Euler(Rot.x - 90, Rot.y, Rot.z);
                }
                else
                    CarController.Carcontroller.Weap.Weapons[2].transform.localRotation = Quaternion.Euler(0, 0, 0);
                var PV = PhotonView.Get(this);
                PV.RPC("SelectWeap", RpcTarget.All, CarController.Carcontroller.gameObject.GetPhotonView().ViewID, 2);
            }
            else if(curWeap == 4)
            {
                WeaponSelector(4);
                var PV = PhotonView.Get(this);
                PV.RPC("SelectWeap", RpcTarget.All, CarController.Carcontroller.gameObject.GetPhotonView().ViewID, 3);
            }

            #endregion
            #region Timers
            if (CarController.Carcontroller.Weap.MiniGuntime > 0)
                CarController.Carcontroller.Weap.MiniGuntime -= Time.deltaTime;
            if (CarController.Carcontroller.Weap.SecondaryTime > 0)
                CarController.Carcontroller.Weap.SecondaryTime -= Time.deltaTime;
            if (CarController.Carcontroller.Weap.HeavyTime > 0)
                CarController.Carcontroller.Weap.HeavyTime -= Time.deltaTime;
            if (CarController.Carcontroller.Weap.MinesDelay > 0)
                CarController.Carcontroller.Weap.MinesDelay -= Time.deltaTime;
            if (CarController.Carcontroller.Weap.SpecialDelay > 0)
            {
                CarController.Carcontroller.Weap.SpecialDelay -= Time.deltaTime;
                SpecialDelay.fillAmount = 1 - (CarController.Carcontroller.Weap.SpecialDelay / CarController.Carcontroller.Weap.SpecialBaseDelay);
            }
            else
            {
                SpecialDelay.fillAmount = 1;
            }
            #endregion
            #region AmmoDisp
            for (int i = 0; i < 3; i++)
                Ammo[i].text = string.Format("x{0}",AmmoBase[i+1].ToString());
            #endregion
            var rect = openFire.transform as RectTransform;
            if (!Application.isMobilePlatform && Input.GetMouseButton(0))
            {
                Vector2 mouse = rect.InverseTransformPoint(Input.mousePosition);
                if (rect.rect.Contains(mouse) && CarController.Carcontroller.health.isAlive)
                {
                    switch (curWeap)
                    {
                        case 1:
                            CarController.Carcontroller.Weap.Minigun();
                            OverHeat.fillAmount = CarController.Carcontroller.Weap.MinigunOverHeat / CarController.Carcontroller.Weap.TimeToOverHeat;
                            break;
                        case 2: CarController.Carcontroller.Weap.Secondary(AmmoBase); MinigunCooling(); break;
                        case 3: CarController.Carcontroller.Weap.HeavyWeap(AmmoBase); MinigunCooling(); break;
                        case 4: CarController.Carcontroller.Weap.Special(true); MinigunCooling(); break;
                    }
                }
                else
                {
                    CarController.Carcontroller.Weap.MinigunSpeed = 0.1f;
                    MinigunCooling();
                }
            }
            else if (Application.isMobilePlatform && Input.touchCount > 0)
            {
                var Touches = Input.touches;
                Vector2 mouse = new Vector2(0, 0);
                bool InRect = false;
                for (int TouchID = 0; TouchID < Input.touchCount; TouchID++)
                {
                    Vector3 touch_pos = Touches[TouchID].position;
                    mouse = rect.InverseTransformPoint(touch_pos);
                    if (rect.rect.Contains(mouse))
                    {
                        InRect = true;
                        break;
                    }
                    else
                        InRect = false;
                }
                if (InRect && CarController.Carcontroller.health.isAlive && !CarController.Carcontroller.health.IsStunned)
                {
                    switch (curWeap)
                    {
                        case 1:
                            CarController.Carcontroller.Weap.Minigun();
                            OverHeat.fillAmount = CarController.Carcontroller.Weap.MinigunOverHeat / CarController.Carcontroller.Weap.TimeToOverHeat;
                            break;
                        case 2: CarController.Carcontroller.Weap.Secondary(AmmoBase); MinigunCooling(); break;
                        case 3: CarController.Carcontroller.Weap.HeavyWeap(AmmoBase); MinigunCooling(); break;
                        case 4: CarController.Carcontroller.Weap.Special(true); MinigunCooling(); break;
                    }
                }
                else
                {
                    CarController.Carcontroller.Weap.MinigunSpeed = 0.1f;
                    MinigunCooling();
                }
            }
            else
            {
                CarController.Carcontroller.Weap.MinigunSpeed = 0.1f;
                MinigunCooling();
            }
        }

        void MinigunCooling()
        {
            if (CarController.Carcontroller.Weap.MinigunOverHeat > 0)
            {
                OverHeat.fillAmount = CarController.Carcontroller.Weap.MinigunOverHeat / CarController.Carcontroller.Weap.TimeToOverHeat;
                CarController.Carcontroller.Weap.MinigunOverHeat -= Time.deltaTime;
                if (CarController.Carcontroller.Weap.MinigunOverHeat <= 0)
                {
                    CarController.Carcontroller.Weap.OverHeated = false;
                }
            }
        }

        [PunRPC]
        void SelectWeap(int CarID, int WeapID)
        {
            var Car = PhotonView.Find(CarID).gameObject;
            Car.GetComponent<CarController>().Weap.SelectWeapon(WeapID);
        }
    }
}
