using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Cars
{
    public class BulletCollision : MonoBehaviour
    {
        public PhotonView PV;
        private void OnParticleCollision(GameObject other)
        {
            if (PV.IsMine && other.GetComponentInParent<PhotonView>().OwnerActorNr != PhotonNetwork.LocalPlayer.ActorNumber && other.name == "Bullet")
                PV.RPC("DealDmg", RpcTarget.All, PV.ViewID, PhotonRoom.photonroom.FindPlayerNum(other.GetComponentInParent<PhotonView>().Owner.NickName), 1);
            if (PV.IsMine && other.GetComponentInParent<PhotonView>().OwnerActorNr != PhotonNetwork.LocalPlayer.ActorNumber && other.name == "Cannon")
                PV.RPC("DealDmg", RpcTarget.All, PV.ViewID, PhotonRoom.photonroom.FindPlayerNum(other.GetComponentInParent<PhotonView>().Owner.NickName), 10);
        }

        [PunRPC]
        void DealDmg(int TarID, int ActNum, int Dmg)
        {
            var Target = PhotonView.Find(TarID).gameObject;
            if (Target.GetComponent<Health>().health > 0)
            {
                GameLogic.gameLogic.statHandler.stats[ActNum].DamageDealt += Dmg;
                Target.GetComponent<Health>().ManageHealth(-Dmg, ActNum);
            }
        }
    }
}
