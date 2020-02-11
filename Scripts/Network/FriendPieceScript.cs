using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
namespace Cars
{
    public class FriendPieceScript : MonoBehaviour, FriendsMenu.IFriendPieces
    {
        public string NickName { get; set; }
        public string Id { get; set; }
        public int Num { get; set; }
        public Button Invite;
        public TextMeshProUGUI Name;
        public RawImage status;
        public AudioClip sound;
        public void SetUpListener()
        {
            Invite.onClick.AddListener(() => FriendsMenu.FrMenu.CreateGameInvite(NickName));
            Invite.onClick.AddListener(() => MainMenu.menu.PlaySingle(sound));
        }
        public void IfOnline()
        {
            if (FriendsMenu.FrMenu.Infos.Count != 0 && FriendsMenu.FrMenu.Infos[Num].IsOnline)
            {
                status.color = Color.green;
            }
            else
                status.color = Color.red;
            if (MainMenu.menu.AwaitingLeader && FriendsMenu.FrMenu.Infos.Count != 0 && FriendsMenu.FrMenu.Infos[Num].IsInRoom)
            {
                if (NickName == MainMenu.menu.LeaderName && PhotonNetwork.IsConnectedAndReady)
                {
                    PhotonNetwork.JoinRoom(FriendsMenu.FrMenu.Infos[Num].Room);
                }
            }
        }
    }
}

