using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using ExitGames.Client.Photon;
using Photon.Pun.UtilityScripts;

namespace Cars
{
    public class PhotonRoom : MonoBehaviourPunCallbacks, IInRoomCallbacks
    {
        public static PhotonRoom photonroom;
        private PhotonView PV;

        public bool isgameloaded, IsSpawned;
        public int currentscene;
        public List<int> playersOnLine = new List<int>();
        public Player[] photonplayers;
        public int playersinroom;
        public int myNumberInRoom;
        public TextMeshProUGUI playerName, rank;
        private bool readytocount;
        private bool readytoStart, IsGameStarted;
        public string[] PatternsCodes = new string[9];
        public Color[] colors = new Color[9];
        private Player leaver;
        public float staringtime;
        private float lessthanMaxPlayers, timer;
        private float atMaxPlayers;
        private float timeToStart, TimeToCleanRoom;
        private RoomInfo rInfo;
        public string Mode;
        public List<string> UserIds = new List<string>();

        private void Awake()
        {

            if (photonroom == null)
            {
                photonroom = this;
            }
            else
            {
                if (photonroom != this)
                {
                    Destroy(photonroom.gameObject);
                    photonroom = this;
                }

            }
            //DontDestroyOnLoad(gameObject);
            PV = PhotonView.Get(this);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            PhotonNetwork.AddCallbackTarget(this);
            SceneManager.sceneLoaded += OnSceneFinishedLoading;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            PhotonNetwork.RemoveCallbackTarget(this);
            SceneManager.sceneLoaded -= OnSceneFinishedLoading;
        }

        void Start()
        {
            readytocount = false;
            readytoStart = false;
            lessthanMaxPlayers = staringtime;
            atMaxPlayers = 6;
            timeToStart = staringtime;
        }

        public override void OnJoinedRoom()
        {
            UserIds.Clear();
            PhotonNetwork.AutomaticallySyncScene = true;
            rInfo = PhotonNetwork.CurrentRoom;
            Mode = (string)rInfo.CustomProperties["M"];
            if (Mode == "LBBY")
            {
                for (int i = 0; i < PhotonNetwork.PlayerList.Length - 1; i++)
                {
                    MainMenu.menu.lobbyImgs[i].SetActive(true);
                    MainMenu.menu.lobbySeats[i].SetActive(false);
                }
                return;
            }
            Debug.Log("Player is joined the room");
            photonplayers = PhotonNetwork.PlayerList;
            playersinroom = photonplayers.Length;
            myNumberInRoom = playersinroom;
            var PD = PlayerP.PlayerPresets.GetParsed();
            PV.RPC("SendPatternCode", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, PlayerP.PlayerPresets.PresetCode, PD.color.r , PD.color.g, PD.color.b);
            if (MainMenu.menu.delaystart)
            {
                Debug.Log("Displayer players in room out of max players possible (" + playersinroom + ":" + MainMenu.menu.Maxplayers + ")");
                if (playersinroom > 1)
                {
                    readytocount = true;
                }
                if (playersinroom == MainMenu.menu.Maxplayers)
                {
                    readytoStart = true;
                    if (!PhotonNetwork.IsMasterClient)
                        return;
                    PhotonNetwork.CurrentRoom.IsOpen = false;


                }
            }
            else
            {
                StartGame();
            }
        }

        public override void OnLeftRoom()
        {

        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (Mode == "LBBY")
            {
                if (PhotonNetwork.IsMasterClient)
                    UserIds.Remove(otherPlayer.UserId);
                for (int i = 0; i < 3; i++)
                {
                    MainMenu.menu.lobbyImgs[i].SetActive(false);
                    MainMenu.menu.lobbySeats[i].SetActive(true);
                }
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    MainMenu.menu.lobbyImgs[i].SetActive(true);
                    MainMenu.menu.lobbySeats[i].SetActive(false);
                }
                return;
            }
            photonplayers = PhotonNetwork.PlayerList;
            playersinroom = photonplayers.Length;
            if (playersOnLine.Contains(otherPlayer.ActorNumber))
                playersOnLine.Remove(otherPlayer.ActorNumber);
            GameLogic.gameLogic.statHandler.stats[FindPlayerNum(otherPlayer.NickName)] = new StatHandler();
            if (PhotonNetwork.IsMasterClient)
            {   
                if(PhotonView.Find((otherPlayer.ActorNumber * 1000) + 1))
                    PhotonNetwork.Destroy(PhotonView.Find((otherPlayer.ActorNumber * 1000) + 1).gameObject);
                if(playersinroom < MainMenu.menu.Maxplayers - 1)
                    PhotonNetwork.CurrentRoom.IsOpen = true;
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (Mode == "LBBY")
            {
                if (PhotonNetwork.IsMasterClient)
                    UserIds.Add(newPlayer.UserId);
                for (int i = 0; i < PhotonNetwork.PlayerList.Length - 1; i++)
                {
                    MainMenu.menu.lobbyImgs[i].SetActive(true);
                    MainMenu.menu.lobbySeats[i].SetActive(false);
                }
                return;
            }
            var PD = PlayerP.PlayerPresets.GetParsed();
            PV.RPC("SendPatternCode", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, PlayerP.PlayerPresets.PresetCode, PD.color.r , PD.color.g, PD.color.b);
            Debug.Log("Player is entered room");
            photonplayers = PhotonNetwork.PlayerList;
            playersinroom = photonplayers.Length;
            GameLogic.gameLogic.statHandler.stats[FindFreeSpace()].NickName = newPlayer.NickName;
            if (IsGameStarted && PhotonNetwork.IsMasterClient)
            {
                PV.RPC("ENABLE", RpcTarget.All, GameLogic.gameLogic.GameTimer);
                PV.RPC("SyncStats", RpcTarget.Others, JsonUtility.ToJson(GameLogic.gameLogic.statHandler), GameLogic.gameLogic.kills);
            }
            if (MainMenu.menu.delaystart)
            {
                if (playersinroom == MainMenu.menu.Maxplayers)
                {
                    readytocount = true;
                    readytoStart = true;
                    if (!PhotonNetwork.IsMasterClient)
                        return;
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                }
            }
            else
            {
                if (playersinroom == MainMenu.menu.Maxplayers)
                {
                    if (!PhotonNetwork.IsMasterClient)
                        return;
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                }
            }
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            Debug.Log("MasterClient switched.");
        }
        int CleanRoomFromReserved()
        {
            PhotonNetwork.CurrentRoom.ClearExpectedUsers();
            return 15;
        }
        void Update()
        {
            if (MainMenu.menu.delaystart)
            {
                if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
                    TimeToCleanRoom = (TimeToCleanRoom > 0) ? TimeToCleanRoom - Time.deltaTime : CleanRoomFromReserved();
                if (playersinroom == 1)
                {
                    restarttimer();
                }
                if (!isgameloaded)
                {
                    if (readytoStart)
                    {
                        atMaxPlayers -= Time.deltaTime;
                        lessthanMaxPlayers = atMaxPlayers;
                        timeToStart = atMaxPlayers;
                    }
                    else if (readytocount)
                    {
                        lessthanMaxPlayers -= Time.deltaTime;
                        timeToStart = lessthanMaxPlayers;
                        Debug.Log("Time to start is " + timeToStart);
                    }
                    if (timeToStart <= 0)
                    {
                        StartGame();
                    }
                }
            }
        }
        public void EnableMM()
        {
            PV.RPC("IntoGameFromLBBY", RpcTarget.Others, MainMenu.menu.nickname);
        }
        void StartGame()
        {
            isgameloaded = true;
            if (!PhotonNetwork.IsMasterClient)
                return;
            if (MainMenu.menu.delaystart)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
            PhotonNetwork.LoadLevel(2);
            
        }
        void restarttimer()
        {
            lessthanMaxPlayers = staringtime;
            timeToStart = staringtime;
            atMaxPlayers = 3;
            readytocount = false;
            readytoStart = false;
        }

        void OnSceneFinishedLoading(Scene scene, LoadSceneMode loadSceneMode)
        {

            currentscene = scene.buildIndex;
            if (currentscene == 3 && !IsSpawned)
            {
                GameLogic.gameLogic.statHandler.stats[myNumberInRoom].NickName = PhotonNetwork.NickName;
                IsSpawned = true;
                var resps = GameObject.FindGameObjectsWithTag("Respawn");
                var respawn = resps[FindMyNum(PhotonNetwork.NickName)];
                float y = respawn.transform.localRotation.eulerAngles.y;
                PhotonNetwork.Instantiate(Path.Combine("Prefabs", MainMenu.menu.CarBelong[PlayerPrefs.GetInt("SelectedCar")].Name, MainMenu.menu.CarBelong[PlayerPrefs.GetInt("SelectedCar")].Name), respawn.transform.position, Quaternion.Euler(0,y,0));
                CarController.Carcontroller.TurnOn();
                /*if (MainMenu.menu.delaystart)
                {
                    PV.RPC("RPC_LoadedGameScene", RpcTarget.MasterClient);
                }*/
                PV.RPC("RPC_LoadedGameScene", RpcTarget.MasterClient);
            }
        }
        public int FindMyNum(string Nickname)
        {
            return myNumberInRoom;
        }
        public int FindFreeSpace()
        {
            int i;
            for (i = 1; i < 9; i++)
            {
                if (GameLogic.gameLogic.statHandler.stats[i].NickName is null || GameLogic.gameLogic.statHandler.stats[i].NickName == "") break;
            }
            i = (i == 9) ? 0 : i;
            return i;
        }
        public int FindPlayerNum(string Nickname)
        {
            int i;
            for(i = 1; i < 9; i++)
            {
                if (GameLogic.gameLogic.statHandler.stats[i].NickName == Nickname) break;
            }
            i = (i==9)? 0 : i;
            return i;
        }
        public void Cancel()
        {
            SceneManager.LoadScene(0);
            PhotonNetwork.LeaveRoom();
            Debug.Log("Player is left room");
        }

        IEnumerator Paint()
        {
            for(int i = 0;i<playersOnLine.Count;i++)
            {
                if (PhotonView.Find((playersOnLine[i] * 1000) + 1) == null)
                {
                    i--;
                    yield return null;
                }
                else
                PlayerP.PlayerPresets.PaintCar(playersOnLine[i], PatternsCodes[playersOnLine[i]], colors[playersOnLine[i]]);
            }
        }

        [PunRPC]
        private void RPC_LoadedGameScene()
        {
            /*if (playerInGame == PhotonNetwork.PlayerList.Length && !IsGameStarted)
            {
                PV.RPC("ENABLE", RpcTarget.All, 900f);
            }*/
            PV.RPC("ENABLE", RpcTarget.All, GameLogic.gameLogic.GameTimer);
            PV.RPC("SyncStats", RpcTarget.Others, JsonUtility.ToJson(GameLogic.gameLogic.statHandler), GameLogic.gameLogic.kills);
        }
        [PunRPC]
        private void IntoGameFromLBBY(string Name_Of_Owner)
        {
            MainMenu.menu.LeaderName = Name_Of_Owner;
            MainMenu.menu.AwaitingLeader = true;
            PhotonNetwork.LeaveRoom();
        }

        [PunRPC]
        private void SendPatternCode(int Num, string Code, float r, float g, float b)
        {
            PatternsCodes[Num] = Code;
            colors[Num].r = r;
            colors[Num].g = g;
            colors[Num].b = b;
        }

        [PunRPC]
        private void ENABLE(float Timer)
        {
            isgameloaded = true;
            playersOnLine.Clear();
            GameLogic.gameLogic.GameTimer = Timer;
            GameLogic.gameLogic.IsGameStarted = true;
            foreach (Player player in photonplayers)
            {
                playersOnLine.Add(player.ActorNumber);
                //GameLogic.gameLogic.statHandler.stats[FindMyNum(player.NickName)].NickName = player.NickName;
            }
            StartCoroutine(Paint());
            IsGameStarted = true;
        }
        [PunRPC]
        private void SyncStats(string StatJson, int TotalKills)
        {
            GameLogic.gameLogic.kills = TotalKills;
            GameLogic.gameLogic.statHandler = JsonUtility.FromJson<StatsBody>(StatJson);
            myNumberInRoom = FindPlayerNum(PhotonNetwork.NickName);
        }
    }
}
