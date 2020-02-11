using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Cars
{
    public class BoostBox : MonoBehaviour
    {
        public GameObject gbox;
        public float timer;
        public bool isActive = false;
        public ParticleSystem Effect;
        public AudioSource Src;
        private void FixedUpdate()
        {
            if (timer > 0)
                timer -= Time.deltaTime;
            if (!isActive && PhotonNetwork.IsMasterClient && timer <= 0)
            {
                PhotonView photonView = PhotonView.Get(this);
                photonView.RPC("BoxActivation", RpcTarget.AllBufferedViaServer, photonView.ViewID, true);
            }
        }
        private void OnTriggerEnter(Collider collision)
        {
            if (collision.gameObject.tag == "Player" && isActive && collision.gameObject.GetPhotonView().IsMine)
            {
                var Health = collision.gameObject.GetComponent<Health>();
                Health.BoostAmount = (Health.BoostAmount + 5 <= Health.MaxBoostAmount) ? Health.BoostAmount + 5 : Health.MaxBoostAmount;
                PhotonView photonView = PhotonView.Get(this);
                photonView.RPC("BoxActivation", RpcTarget.AllBufferedViaServer, photonView.ViewID, false);
            }
        }
        [PunRPC]
        void BoxActivation(int Id, bool status)
        {
            var box = PhotonView.Find(Id).gameObject;
            box.GetComponent<BoostBox>().gbox.SetActive(status);
            box.GetComponent<BoostBox>().isActive = status;
            BoostBox Script = box.GetComponent<BoostBox>();
            Script.Effect.Play();
            Script.Src.volume = PlayerPrefs.GetFloat("SfxVol");
            Script.Src.Play();
            if (!status)
            {
                box.GetComponent<BoostBox>().timer = 30f;
            }
        }
    }
}
