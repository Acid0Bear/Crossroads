using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
namespace Cars
{
    public class Health : MonoBehaviour
    {
        public int health = 100;
        int armorVal = 1;
        public int killerID, Shooter;
        public List<int> Attackers = new List<int>();
        public List<GameObject> damage = new List<GameObject>();
        public GameObject KillsLogPiece;
        public bool isAlive = true;
        public bool IsStunned = false;
        Coroutine[] coroutines = new Coroutine[9];
        [HideInInspector]
        public Coroutine armor = null, Boost = null;
        public float BoostAmount;
        public float MaxBoostAmount = 20;
        private void Update()
        {
            Shooter = 0;
            Ammunition.ammunition.BoostAmout.fillAmount += (Ammunition.ammunition.BoostAmout.fillAmount < BoostAmount / MaxBoostAmount) ? Time.deltaTime : 0;
        }
        public void ManageHealth(int Amount, int ShooterID)
        {
            int Dmg = Mathf.FloorToInt(Amount / armorVal);
            health += Dmg;
            Shooter = ShooterID;
            if (!Attackers.Contains(ShooterID) && Dmg > 0)
            {
                Attackers.Add(ShooterID);
                if (coroutines[ShooterID] != null)
                    StopCoroutine(coroutines[ShooterID]);
                coroutines[ShooterID] = StartCoroutine(SetTimer(ShooterID));
            }
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("SetOnFire", RpcTarget.All, photonView.ViewID, health);
            if (health <= 0)
            {
                killerID = ShooterID;
                if (gameObject.GetPhotonView().IsMine)
                {
                    TargetFinder.targetFinder.Destroyed.SetActive(true);
                    TargetFinder.targetFinder.ToResp.text = string.Format("Respawn in {0} seconds", 5f);
                    StartCoroutine(RespawnTimer());
                    photonView.RPC("SetKiller", RpcTarget.All, killerID, PhotonNetwork.LocalPlayer.ActorNumber);
                    photonView.RPC("ChangeState", RpcTarget.All, photonView.ViewID, false, 0);
                }

            }
        }
        public void ManageBoost(bool state)
        {
            PhotonView PV = PhotonView.Get(this);
            PV.RPC("VisualNitro", RpcTarget.All, PV.ViewID, state);
        }
        public IEnumerator Armor()
        {
            Ammunition.ammunition.armor.SetActive(true);
            Ammunition.ammunition.armImg.fillAmount = 1;
            float timer = 20f;
            armorVal = 2;
            while (timer != 0)
            {
                Ammunition.ammunition.armImg.fillAmount = timer / 20;
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    armor = null;
                    armorVal = 1;
                    Ammunition.ammunition.armor.SetActive(false);
                    break;
                }
                yield return null;
            }
        }
        public IEnumerator BoostRout()
        {
            PhotonView PV = PhotonView.Get(this);
            PV.RPC("VisualNitro", RpcTarget.All, PV.ViewID, true);
            PV.RPC("NitroAuido", RpcTarget.All, PV.ViewID, 1);
            while (true)
            {
                if (!CarController.Carcontroller.NitroSrc.isPlaying)
                {
                    PV.RPC("NitroAuido", RpcTarget.All, PV.ViewID, 2);
                }
                if (BoostAmount <= 0)
                {
                    CarController.Carcontroller.carSpeed = 750;
                    Boost = null;
                    break;
                }
                CarController.Carcontroller.carSpeed = 1000;
                BoostAmount -= Time.deltaTime;
                Ammunition.ammunition.BoostAmout.fillAmount = BoostAmount / MaxBoostAmount;
                yield return null;
            }
            PV.RPC("VisualNitro", RpcTarget.All, PV.ViewID, false);
        }

        IEnumerator RespawnTimer()
        {
            float timer = 5f;
            while (timer != 0)
            {
                timer -= Time.deltaTime;
                TargetFinder.targetFinder.ToResp.text = string.Format("Respawn in {0} seconds", timer.ToString("0"));
                if (timer <= 0)
                {
                    var resps = GameObject.FindGameObjectsWithTag("Respawn");
                    gameObject.transform.position = resps[Random.Range(0, resps.Length - 1)].transform.position;
                    TargetFinder.targetFinder.Destroyed.SetActive(false);
                    PhotonView photonView = PhotonView.Get(this);
                    photonView.RPC("ChangeState", RpcTarget.All, photonView.ViewID, true, 100);
                    break;
                }
                yield return null;
            }
        }
        IEnumerator SetTimer(int ShooterId)
        {
            float timer = 5f;
            while (timer != 0)
            {
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    Attackers.Remove(ShooterId);
                    coroutines[ShooterId] = null;
                    break;
                }
                if (ShooterId == Shooter)
                    timer = 5f;
                yield return new WaitForFixedUpdate();
            }
        }
        [PunRPC]
        void SetKiller(int Killer, int Victim)
        {
            if (Killer != Victim)
            {
                GameLogic.gameLogic.statHandler.stats[Killer].Kills++;
                GameLogic.gameLogic.kills++;
            }
            foreach (int AttackerID in Attackers)
            {
                if (AttackerID != Killer)
                    GameLogic.gameLogic.statHandler.stats[AttackerID].Assists++;
            }
            GameLogic.gameLogic.statHandler.stats[Victim].Deaths++;
            var Piece = Instantiate(KillsLogPiece, Ammunition.ammunition.KillsLog).GetComponent<KillsLogPiece>();
            Piece.Killer = GameLogic.gameLogic.statHandler.stats[Killer].NickName;
            Piece.Victim = GameLogic.gameLogic.statHandler.stats[Victim].NickName;
            Piece.SetUpUI();
        }

        [PunRPC]
        void ChangeState(int ID, bool state, int value)
        {
            var obj = PhotonView.Find(ID).gameObject.GetComponent<Health>();
            obj.isAlive = state;
            if (state)
            {
                obj.damage[2].SetActive(false);
                obj.damage[1].SetActive(false);
                obj.damage[0].SetActive(false);
            }
            obj.health = value;
        }
        [PunRPC]
        void SetOnFire(int ID, int value)
        {
            var obj = PhotonView.Find(ID).gameObject.GetComponent<Health>();
            if (value <= 75 && value > 50)
            {
                obj.damage[0].SetActive(true);
            }
            else if (value <= 50 && value > 25)
            {
                obj.damage[1].SetActive(true);
                obj.damage[0].SetActive(false);
            }
            else if (value <= 25)
            {
                obj.damage[2].SetActive(true);
                obj.damage[1].SetActive(false);
                obj.damage[0].SetActive(false);
            }
        }
        [PunRPC]
        void VisualNitro(int ID, bool value)
        {
            var obj = PhotonView.Find(ID).gameObject.GetComponent<CarController>();
            if (value)
                obj.MiscsPart.EnableNitro();
            else
                obj.MiscsPart.DisableNitro();
        }
        [PunRPC]
        void NitroAuido(int ID, int state)
        {
            var obj = PhotonView.Find(ID).gameObject.GetComponent<CarController>();
            obj.NitroSrc.volume = (PlayerPrefs.GetFloat("SfxVol")/2);
            if (state == 1)
            {
                obj.NitroSrc.clip = obj.NitroActivation;
                obj.NitroSrc.Play();
            }
            else
            {
                obj.NitroSrc.clip = obj.NitroRunning;
                obj.NitroSrc.Play();
            }
                
        }
    }
}
