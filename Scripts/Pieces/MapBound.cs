using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Cars
{
    public class MapBound : MonoBehaviour
    {

        private void OnTriggerEnter(Collider e)
        {
            if (e.gameObject != this.gameObject && e.gameObject.tag == "Player")
            {
                PhotonView PV = PhotonView.Get(this);
                PV.RPC("DestroyCar", RpcTarget.All, e.gameObject.GetPhotonView().ViewID);
            }
        }

        [PunRPC]
        void DestroyCar(int TargetID)
        {
            var Target = PhotonView.Find(TargetID).gameObject.GetComponent<Health>();
            Target.GetComponent<Health>().ManageHealth(-100, PhotonRoom.photonroom.FindMyNum(PhotonView.Find(TargetID).Owner.NickName));
        }
    }
}
