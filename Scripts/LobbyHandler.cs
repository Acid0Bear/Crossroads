using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cars
{
    public class LobbyHandler : MonoBehaviour
    {
        private PhotonView PV;
        public TextMeshProUGUI TimeinMM, Sign;
        public RawImage[] seats;
        public GameObject SeatsHolder, Chat;
        public GameObject MsgPiece;
        public Transform ChatParent;
        float TimeInMM;
        public string Msg { get; set; }
        private void Awake()
        {
            PV = PhotonView.Get(this);
        }
        void Update()
        {
            if (PhotonNetwork.InRoom && PhotonRoom.photonroom.Mode != "LBBY")
            {
                TimeInMM += Time.deltaTime;
                SeatsHolder.SetActive(true);
                Chat.SetActive(true);
                for (int i =0; i<8;i++)
                {
                    if(i<PhotonNetwork.PlayerList.Length)
                        seats[i].color = Color.green;
                    else
                        seats[i].color = Color.red;
                }
                TimeinMM.text = string.Format("{0}:{1}", Mathf.FloorToInt(TimeInMM / 60).ToString("00"), (TimeInMM % 60).ToString("00"));
            }
            else if (PhotonNetwork.InRoom && PhotonRoom.photonroom.Mode == "LBBY")
            {
                Chat.SetActive(true);
            }
            else
            {
                SeatsHolder.SetActive(false);
                Chat.SetActive(false);
                TimeInMM = 0;
            }
        }
        public void SendMsg()
        {
            if (Msg != "")
            {
                Msg = string.Format("{0}: {1}", MainMenu.menu.nickname, Msg);
                PV.RPC("CreateMsg", RpcTarget.All, Msg);
                Msg = "";
            }
        }
        [PunRPC]
        void CreateMsg(string Msg)
        {
            var Txt = Instantiate(MsgPiece, ChatParent).GetComponent<TextMeshProUGUI>();
            Txt.transform.position = new Vector3(102,0,0);
            Txt.text = Msg;
        }
    }
}
