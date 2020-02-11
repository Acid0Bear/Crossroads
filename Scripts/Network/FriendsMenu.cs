using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace Cars
{
    public class FriendsMenu : MonoBehaviourPunCallbacks
    {
        public TMP_InputField user_name;
        public static FriendsMenu FrMenu;
        public TextMeshProUGUI errorMsg, NumberOfFReq;
        public RectTransform FriendList;
        public GameObject FriendReqTablePanel, ReqSample, ReqView;
        public GameObject FriendListPanPanel, FriendSample;
        public GameObject GameInvTablePanel, GameInvSample;
        public List<GameObject> Friends = new List<GameObject>();
        public List<string> FriendsIDs = new List<string>();
        public List<GameObject> FrRequestsLIST = new List<GameObject>();
        public List<GameObject> GameInvLIST = new List<GameObject>();
        public List<FriendInfo> Infos = new List<FriendInfo>();
        public TextMeshProUGUI Fr, WannaAdd;
        public string User;
        public bool IsFriendsReceived = false;
        [HideInInspector]
        public bool AwaitingResponse = false;
        [HideInInspector]
        public float Timer = 2f;
        [HideInInspector]
        public int Delay = 10;
        int ReqCounter = 0;

        private void Awake()
        {
            if (FrMenu == null)
            {
                FrMenu = this;
            }
            else
                Destroy(this);
        }
        public void GetFriends()
        {
            PhotonNetwork.WebRpc("ReqAnalyzer", JsonUtility.ToJson(new FriendsHookform { Code = "6", Nickname = MainMenu.menu.nickname, User = "" }));
        }
        public void AnotherUser()
        {
            User = user_name.text.ToString();
        }

        public void FrLang()
        {
            Fr.text = MainMenu.menu.lang.FriendsSection[0];
            WannaAdd.text = MainMenu.menu.lang.FriendsSection[2];
        }

        private void LateUpdate()
        {
            Timer = (Timer <= 0 && PhotonNetwork.IsConnectedAndReady) ? CheckReq() : Timer - Time.deltaTime;
            if (FriendsIDs.Count != 0 && !AwaitingResponse && !PhotonNetwork.InRoom && PhotonNetwork.IsConnectedAndReady)
            {
                var arr = FriendsIDs.ToArray();
                PhotonNetwork.FindFriends(FriendsIDs.ToArray());
                AwaitingResponse = true;
            }
                
            foreach (GameObject piece in Friends)
            {
                var IFR = piece.GetComponent<IFriendPieces>();
                IFR.IfOnline();
            }
            //NumberOfFReq.text = (ReqCounter==0)? "":ReqCounter.ToString() + "*";
            if (FrRequestsLIST.Count >= 1)
            {
                ReqView.SetActive(true);
                OpenReq();
            }
            else
            {
                ReqView.SetActive(false);
                CloseReq();
            }
        }

        public override void OnFriendListUpdate(List<FriendInfo> friendList)
        {
            Infos = friendList;
            AwaitingResponse = false;
        }
        float CheckReq()
        {
            if (!IsFriendsReceived)
            {
                foreach (GameObject Friend in Friends)
                {
                    Destroy(Friend);
                }
                FriendsIDs.Clear();
                Friends.Clear();
                Infos.Clear();
                GetFriends();
                IsFriendsReceived = true;
            }
            if (!MainMenu.menu.AwaitingLeader)
                PhotonNetwork.WebRpc("ReqAnalyzer", JsonUtility.ToJson(new FriendsHookform { Code = "2", Nickname = MainMenu.menu.nickname, User = "" }));
            return Delay;
        }

        public void CreateFriendRequest()
        {
            if (User != "")
            {
                PhotonNetwork.WebRpc("ReqAnalyzer", JsonUtility.ToJson(new FriendsHookform { Code = "0", Nickname = MainMenu.menu.nickname, User = User }));
            }
            else
            {
                errorMsg.text = "Fields cannt be empty";
            }
        }

        public void CreateGameInvite(string ToUser)
        {
            PhotonNetwork.WebRpc("ReqAnalyzer", JsonUtility.ToJson(new FriendsHookform { Code = "1", Nickname = MainMenu.menu.nickname, User = ToUser }));
            var RoomType = new ExitGames.Client.Photon.Hashtable { { "M", "LBBY" } };
            RoomOptions roomops = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 4, CustomRoomProperties = RoomType, PublishUserId = true, CustomRoomPropertiesForLobby = new string[] { "M" } };
            PhotonNetwork.CreateRoom(MainMenu.menu.nickname, roomops);
        }

        public void OpenReq()
        {
            FriendList.sizeDelta = new Vector2(326.9f, 111);
            FriendList.localPosition = new Vector3(-24.7f, -46, 0f);
        }
        public void CloseReq()
        {
            FriendList.sizeDelta = new Vector2(326.9f, 175.6f);
            FriendList.localPosition = new Vector3(-24.7f, -13.71f, 0f);
        }
        public void AddFrd(string Nickname, GameObject piece)
        {
            FrRequestsLIST.Remove(piece);
            PhotonNetwork.WebRpc("ReqAnalyzer", JsonUtility.ToJson(new FriendsHookform { Code = "4", Nickname = MainMenu.menu.nickname, User = Nickname }));
            foreach (GameObject Friend in Friends)
            {
                Destroy(Friend);
            }
            Friends.Clear();
            GetFriends();
            ReqCounter--;
        }

        public void RejectFrd(string Nickname, GameObject piece)
        {
            FrRequestsLIST.Remove(piece);
            PhotonNetwork.WebRpc("ReqAnalyzer", JsonUtility.ToJson(new FriendsHookform { Code = "5", Nickname = MainMenu.menu.nickname, User = Nickname }));
            ReqCounter--;
        }

        public override void OnWebRpcResponse(OperationResponse response)
        {
            if (response.Parameters.ContainsKey(208))
            {
                var Rawdata = (Dictionary<string, object>)response.Parameters[208];
                if (Rawdata.ContainsKey("FriendReq"))
                {
                    string FriendRequests = (string)Rawdata["FriendReq"];
                    FetchReq(FriendRequests);
                }
                if (Rawdata.ContainsKey("GameInv"))
                {
                    string GameInvites = (string)Rawdata["GameInv"];
                    FetchGameInv(GameInvites);
                }
                if (Rawdata.ContainsKey("FriendsList"))
                {
                    string FriendList = (string)Rawdata["FriendsList"];
                    FetchFriends(FriendList);
                }
            }

        }

        public interface IFriendPieces
        {
            void IfOnline();
        }

        void FetchReq(string Req)
        {
            string Temp = "";
            bool IsExists = false;
            for (int i = 0; i < Req.Length; i++)
            {
                if (Req[i] != ';')
                    Temp += Req[i];
                else
                {
                    var Nick = Temp;
                    foreach (GameObject Requests in FrRequestsLIST)
                    {
                        if (Requests.GetComponent<ReqPieceScript>().nickname == Nick)
                        {
                            IsExists = true;
                            break;
                        }
                    }
                    if (!IsExists)
                    {
                        var newPiece = Instantiate(ReqSample, FriendReqTablePanel.transform);
                        FrRequestsLIST.Add(newPiece);
                        newPiece.GetComponent<ReqPieceScript>().Setter(Nick);
                        ReqCounter++;
                    }
                    Temp = "";
                    IsExists = false;
                }
            }
        }
        void FetchGameInv(string Req)
        {
            string Temp = "";
            bool IsExists = false;
            for (int i = 0; i < Req.Length; i++)
            {
                if (Req[i] != ';')
                    Temp += Req[i];
                else
                {
                    var Nick = Temp;
                    foreach (GameObject Requests in GameInvLIST)
                    {
                        if (Requests.GetComponent<GameInvPiece>().nickname == Nick)
                        {
                            IsExists = true;
                            break;
                        }
                    }
                    if (!IsExists)
                    {
                        var newPiece = Instantiate(GameInvSample, GameInvTablePanel.transform);
                        GameInvLIST.Add(newPiece);
                        newPiece.GetComponent<GameInvPiece>().Setter(Nick);
                    }
                    Temp = "";
                    IsExists = false;
                }
            }
        }
        void FetchFriends(string Req)
        {
            string Name = "";
            string ID = "";
            bool Part = false;
            for (int i = 0; i < Req.Length; i++)
            {
                for (int j = i + 10; j < Req.Length; j++)
                {
                    if (Req[j] == ',')
                    {

                        Part = true;
                        j += 8;
                    }
                    if (Req[j] == ';')
                    {
                        i = j - 1;
                        break;
                    }
                    if (!Part)
                        Name += Req[j];
                    else if (Part)
                        ID += Req[j];
                }
                Part = false;
                if (Name != "" && ID != "")
                {
                    var newPiece = Instantiate(FriendSample, FriendListPanPanel.transform);
                    var friend = newPiece.GetComponent<FriendPieceScript>();
                    Friends.Add(newPiece);
                    friend.NickName = Name;
                    friend.Id = "Id" + ID;
                    friend.SetUpListener();
                    FriendsIDs.Add("Id" + ID);
                    friend.Num = FriendsIDs.Count - 1;
                    friend.Name.text = Name;
                }
                Name = "";
                ID = "";
            }
        }
    }
}
[System.Serializable]
class FriendsHookform
{
    public string Code;
    public string Nickname;
    public string User;
}

