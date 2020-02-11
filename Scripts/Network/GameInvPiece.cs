using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameInvPiece : MonoBehaviour
{
    public Button Add, Reject;
    public TextMeshProUGUI NickPlace, req;
    public string nickname;
    float timer = 20;
    void Awake()
    {
        //req.text = MainMenu.menu.lang.FriendsSection[1];
    }
    private void Update()
    {
        timer -= (timer>0) ? Time.deltaTime: RemovePiece();
    }
    int RemovePiece()
    {
        Destroy(this.gameObject);
        return 0;
    }
    public void DeselectFunc()
    {
        Add.onClick.AddListener(() => AddF());
        Reject.onClick.AddListener(() => RejectF());
    }

    void AddF()
    {
        PhotonNetwork.JoinRoom(nickname);
        Destroy(this.gameObject);
    }

    private void RejectF()
    {
        Destroy(this.gameObject);
    }
    public void Setter(string Setnickname)
    {
        nickname = Setnickname;
        NickPlace.text = nickname;
        DeselectFunc();
    }
}
