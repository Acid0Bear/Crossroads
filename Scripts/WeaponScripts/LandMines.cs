using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Cars
{
    public class LandMines : MonoBehaviour
    {
        ParticleSystem expl;
        GameObject ExplGO;
        public GameObject body;
        public GameObject Explosion;
        public AudioClip ExplSound;
        public AudioSource MySrc;
        bool IsSpawned = false;
        public int OwnerID;
        float Timer;
        private void OnTriggerEnter(Collider e)
        {
            if (!IsSpawned && e.gameObject != this.gameObject && e.gameObject.tag == "Player" && gameObject.GetPhotonView().OwnerActorNr != e.gameObject.GetPhotonView().OwnerActorNr)
            {
                PhotonView PV = PhotonView.Get(this);
                if (PV.IsMine)
                    PV.RPC("BlowCar", RpcTarget.All, OwnerID, e.gameObject.GetPhotonView().ViewID);
                if (!IsSpawned)
                {
                    ExplGO = Instantiate(Explosion, transform);
                    ExplGO.transform.position = transform.position;
                    expl = ExplGO.GetComponent<ParticleSystem>();
                    expl.Play();
                    MySrc.clip = ExplSound;
                    MySrc.Play();
                    GetComponent<MeshRenderer>().enabled = false;
                    PhotonView photonView = PhotonView.Get(this);
                    IsSpawned = true;
                    Timer = 0.5f;
                }
            }
        }

        private void Update()
        {
            if (Timer > 0)
                Timer -= Time.deltaTime;
            if (ExplGO != null && expl.particleCount == 0 && Timer <= 0 && !MySrc.isPlaying)
            {
                Destroy(ExplGO);
                if (PhotonView.Find(this.GetComponent<PhotonView>().ViewID).IsMine)
                    PhotonNetwork.Destroy(body);
            }
        }

        [PunRPC]
        void BlowCar(int OwnerID, int TargetID)
        {
            var Target = PhotonView.Find(TargetID).gameObject.GetComponent<Health>();
            GameLogic.gameLogic.statHandler.stats[OwnerID].DamageDealt += 50;
            Target.GetComponent<Health>().ManageHealth(-50, OwnerID);
        }
    }
}
