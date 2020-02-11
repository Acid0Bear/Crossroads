using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Cars
{
    public class ReqPieceScript : MonoBehaviour
    {
        public Button Add, Reject;
        public TextMeshProUGUI NickPlace, req;
        public string nickname;
        void Awake()
        {
            //req.text = MainMenu.menu.lang.FriendsSection[1];
        }
        public void DeselectFunc()
        {
            Add.onClick.AddListener(() => AddF());
            Reject.onClick.AddListener(() => RejectF());
        }

        void AddF()
        {
            FriendsMenu.FrMenu.AddFrd(nickname, gameObject);
            Destroy(this.gameObject);
        }

        private void RejectF()
        {
            FriendsMenu.FrMenu.RejectFrd(nickname, gameObject);
            Destroy(this.gameObject);
        }
        public void Setter(string Setnickname)
        {
            nickname = Setnickname;
            NickPlace.text = nickname;
            DeselectFunc();
        }

    }
}
