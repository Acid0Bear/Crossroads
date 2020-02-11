using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Cars
{
    public class AmmoBox : MonoBehaviour
    {
        //List<int> ammo = new List<int>();
        public List<Mesh> Meshes = new List<Mesh>();
        public GameObject gbox, GadgetCircle;
        public ParticleSystem Effect;
        public AudioSource Src;
        public float timer;
        int CurAmmo;
        public bool isActive = false;

        private void FixedUpdate()
        {
            if (timer > 0)
                timer -= Time.deltaTime;
            if (!isActive && PhotonNetwork.IsMasterClient && timer <= 0)
            {
                PhotonView photonView = PhotonView.Get(this);
                int AmmoType = Random.Range(1, 4);
                photonView.RPC("BoxActivation", RpcTarget.AllBufferedViaServer, photonView.ViewID, true, AmmoType);
            }
        }
        private void OnTriggerEnter(Collider collision)
        {
            //ammo.Clear();
            if (collision.gameObject.tag == "Player" && isActive && collision.gameObject.GetPhotonView().IsMine)
            {
                /*#region Init
                if (Ammunition.ammunition.AmmoBase[0] < 50)
                    for (int i = 0; i < 5; i++)
                        ammo.Add(1)
                if (Ammunition.ammunition.AmmoBase[1] < 8)
                    for (int i = 0; i < 3; i++)
                        ammo.Add(2);
                if (Ammunition.ammunition.AmmoBase[2] < 4)
                    for (int i = 0; i < 3; i++)
                        ammo.Add(3);
                if (Ammunition.ammunition.AmmoBase[3] < 4)
                    for (int i = 0; i < 3; i++)
                        ammo.Add(4);
                #endregion
                if (ammo.Count > 0)
                {
                    int type = Random.Range(0, ammo.Count - 1);
                    switch (ammo[type])
                    {
                        case 1: Ammunition.ammunition.AmmoBase[0] = 50; break;
                        case 2: Ammunition.ammunition.AmmoBase[1] = 8; break;
                        case 3: Ammunition.ammunition.AmmoBase[2] = 4; break;
                        case 4: Ammunition.ammunition.AmmoBase[3] = 4; break;
                    }
                    PhotonView photonView = PhotonView.Get(this);
                    photonView.RPC("BoxActivation", RpcTarget.All, photonView.ViewID, false);
                    timer = 10f; ammo.Clear();
                }*/
                switch (CurAmmo)
                {
                    case 1:
                        if (Ammunition.ammunition.AmmoBase[1] == 8) return;
                        Ammunition.ammunition.AmmoBase[1] = 8; break;
                    case 2:
                        if (Ammunition.ammunition.AmmoBase[2] == 4) return;
                        Ammunition.ammunition.AmmoBase[2] = 4; break;
                    case 3:
                        if (Ammunition.ammunition.AmmoBase[3] == 4) return;
                        Ammunition.ammunition.AmmoBase[3] = 4; break;
                }
                PhotonView photonView = PhotonView.Get(this);
                photonView.RPC("BoxActivation", RpcTarget.AllBufferedViaServer, photonView.ViewID, false, 1);
                timer = 10f; //ammo.Clear();
            }
        }
        [PunRPC]
        void BoxActivation(int Id, bool status, int AmmoType)
        {
            AmmoBox Script;
            var box = PhotonView.Find(Id).gameObject;
            Script = box.GetComponent<AmmoBox>();
            Script.Effect.Play();
            Script.Src.volume = PlayerPrefs.GetFloat("SfxVol");
            Script.Src.Play();
            GadgetCircle.SetActive(false);
            CurAmmo = AmmoType;
            gbox.GetComponent<MeshFilter>().mesh = Meshes[AmmoType-1];
            if (AmmoType == 1)
            {
                gbox.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                gbox.transform.localPosition = new Vector3(0, -0.4f, 0);
            }
            else if (AmmoType == 2)
            {
                gbox.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                gbox.transform.localPosition = new Vector3(0, 0, 0);
            }
            else if (AmmoType == 3)
            {
                gbox.transform.localScale = new Vector3(0.012f, 0.012f, 0.057f);
                gbox.transform.localPosition = new Vector3(-0.028f, -0.42721f, -0.05f);
                GadgetCircle.SetActive(true);
            }
            Script.gbox.SetActive(status);
            Script.isActive = status;
            if (!status)
            {
                box.GetComponent<AmmoBox>().timer = 10f;
            }
        }
    }
}
