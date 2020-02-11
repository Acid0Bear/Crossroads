using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using ExitGames.Client.Photon;
using System.IO;
using UnityEngine.UI;
namespace Cars
{
    public class MainMenu : MonoBehaviourPunCallbacks
    {
        public List<CarBelongings> CarBelong = new List<CarBelongings>();
        public string[] cars = new string[3];
        public static MainMenu menu;
        public TextMeshProUGUI Name;
        public GameObject car;
        public GameObject PVBlock;
        public int Maxplayers;
        public bool delaystart;
        public bool Logout;
        public bool LeftRoom;
        public string nickname;
        public string CurRoom;
        private string Json;
        public Progressiom prog;
        public List<TextMeshProUGUI> MenuGUI = new List<TextMeshProUGUI>();
        public Slider lvlSlider;
        public TextMeshProUGUI Lvl, Silver, Gold;
        public GameObject LoginPanel;
        [HideInInspector]
        public bool AwaitingLeader = false;
        [HideInInspector]
        public bool GroupLobby = false;
        [HideInInspector]
        public string LeaderName;
        [Header("For lobby")]
        public GameObject[] lobbyImgs;
        public GameObject[] lobbySeats;
        public LanguageHolder lang;
        public AudioSource efxSource;
        public AudioSource musicSource;
        private void Awake()
        {
            if (!PlayerPrefs.HasKey("MainVolume") || !PlayerPrefs.HasKey("SfxVol"))
            {
                PlayerPrefs.SetFloat("MainVolume", 0.1f);
                PlayerPrefs.SetFloat("SfxVol", 0.1f);
            }
            musicSource.volume = PlayerPrefs.GetFloat("MainVolume");
            efxSource.volume = PlayerPrefs.GetFloat("SfxVol");
            if (menu == null)
            {
                GameObject.DontDestroyOnLoad(this);
                menu = this;
            }
            else
            {
                if (menu.LeftRoom)
                    LeftRoom = true;
                Destroy(menu.gameObject);
                menu = this;
                GameObject.DontDestroyOnLoad(this);
            }
            if (Application.systemLanguage == SystemLanguage.Russian)
            {
                TextAsset file1 = Resources.Load("ru-RU") as TextAsset;
                lang = JsonUtility.FromJson<LanguageHolder>(file1.text);
            }
            else
            {
                TextAsset file1 = Resources.Load("en-EN") as TextAsset;
                lang = JsonUtility.FromJson<LanguageHolder>(file1.text);
            }
            AssignGUI();
        }
        public void PlaySingle(AudioClip clip)
        {
            efxSource.Stop();
            efxSource.volume = PlayerPrefs.GetFloat("SfxVol");
            efxSource.clip = clip;
            efxSource.Play();
        }

        public void LogoutGame()
        {
            PhotonNetwork.Disconnect();
            Logout = true;
            LoginPanel.SetActive(true);
            FriendsMenu.FrMenu.IsFriendsReceived = false;
            FriendsMenu.FrMenu.AwaitingResponse = false;
            PlayerPrefs.SetInt("SelectedCar", 0);
        }

        public void Update()
        {
            if (SceneManager.GetActiveScene().buildIndex == 1 && Input.GetMouseButtonDown(0) && ShopHandler.ShopHand.CarinfoPanel.activeSelf) ShopHandler.ShopHand.CarinfoPanel.SetActive(false);
            if (SceneManager.GetActiveScene().buildIndex == 1 && !musicSource.isPlaying )
            {
                musicSource.volume = PlayerPrefs.GetFloat("MainVolume");
                float pitch = Random.Range(.95f, 1.05f);
                musicSource.pitch = pitch;
                musicSource.Play();
            }
            else if(SceneManager.GetActiveScene().buildIndex == 1)
            {
                musicSource.volume = PlayerPrefs.GetFloat("MainVolume");
            }
        }
        public void AssignGUI()
        {
            lvlSlider.maxValue = prog.RequiredExp;
            lvlSlider.value = prog.curExp;
            Lvl.text = prog.curlvl.ToString();
            Silver.text = prog.Silver.ToString();
            Gold.text = prog.Gold.ToString();
            Name.text = nickname;
            MenuGUI[0].text = lang.ToBattle[0];
            MenuGUI[1].text = lang.ToBattle[2];
        }

        public void LeaveRoomOrCancelSearch()
        {
            if (PhotonNetwork.InRoom)
                PhotonNetwork.LeaveRoom();
            if (PhotonNetwork.InLobby)
                PhotonNetwork.LeaveLobby();
            if (SceneManager.GetActiveScene().buildIndex != 1)
            {
                Destroy(PVBlock);
                SceneManager.LoadScene(0);
                PhotonNetwork.AutomaticallySyncScene = false;
                LeftRoom = true;
                PhotonNetwork.Disconnect();
            }
        }
        public void ReloadScene()
        {
            Destroy(PVBlock);
            SceneManager.LoadScene(0);
            PhotonNetwork.Disconnect();
        }
        public void Play()
        {
            if (CurRoom != "" && !LeftRoom)
                PhotonNetwork.RejoinRoom(CurRoom);
            else if (PhotonNetwork.InRoom && PhotonRoom.photonroom.Mode == "LBBY")
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonRoom.photonroom.EnableMM();
                    PhotonNetwork.LeaveRoom();
                    StartCoroutine(LeaveAndSearch());
                }
            }
            else
            {
                LeftRoom = false;
                var RoomType = new ExitGames.Client.Photon.Hashtable { { "M", "DM" } };
                PhotonNetwork.JoinRandomRoom(RoomType, (byte)Maxplayers);
                MenuGUI[2].text = lang.Matchmaking[0];
                MenuGUI[3].text = lang.Matchmaking[1];
            }
        }
        IEnumerator LeaveAndSearch()
        {
            while (true)
            {
                if (PhotonNetwork.IsConnectedAndReady)
                {
                    var RoomType = new ExitGames.Client.Photon.Hashtable { { "M", "DM" } };
                    GroupLobby = true;
                    PhotonNetwork.JoinRandomRoom(RoomType, (byte)Maxplayers, MatchmakingMode.FillRoom, null, "", PhotonRoom.photonroom.UserIds.ToArray());
                    break;
                }
                yield return null;
            }
        }

        string[] CreateRoomPropertiesForLobby()
        {
            return new string[]
            {
            "M"
            };
        }

        public override void OnWebRpcResponse(OperationResponse response)
        {
            Debug.Log(response.DebugMessage);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.Log(message);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            base.OnJoinRandomFailed(returnCode, message);
            Debug.Log("No alaliable rooms");
            if (!GroupLobby)
                CreateRoom();
            else
            {
                CreateRoom(PhotonRoom.photonroom.UserIds.ToArray());
            }
        }

        public void CreateRoom()
        {
            int roomNum = Random.Range(0, 10000);
            var RoomType = new ExitGames.Client.Photon.Hashtable { { "M", "DM" } };
            RoomOptions roomops = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)Maxplayers, EmptyRoomTtl = 100, PublishUserId = true, PlayerTtl = /*300000*/500, CustomRoomProperties = RoomType, CustomRoomPropertiesForLobby = CreateRoomPropertiesForLobby() };
            PhotonNetwork.CreateRoom("Room" + roomNum, roomops);
        }
        public void CreateRoom(string[] ExpUsers)
        {
            int roomNum = Random.Range(0, 10000);
            var RoomType = new ExitGames.Client.Photon.Hashtable { { "M", "DM" } };
            RoomOptions roomops = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)Maxplayers, EmptyRoomTtl = 100, PublishUserId = true, PlayerTtl = /*300000*/500, CustomRoomProperties = RoomType, CustomRoomPropertiesForLobby = CreateRoomPropertiesForLobby() };
            PhotonNetwork.CreateRoom("Room" + roomNum, roomops, null, ExpUsers);
        }
        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.Log("Failed to create a room");
            CreateRoom();
        }

        public void Cancel()
        {
            PhotonNetwork.LeaveRoom();
            Debug.Log("Player is leaved room");
        }
        public override void OnJoinedRoom()
        {
            Debug.Log("We joined room");
        }
    }
}

[System.Serializable]
public class Progressiom
{
    public int curExp, RequiredExp, curlvl;
    public int Silver,Gold;
    public void CheckLvl()
    {
        if (curExp > RequiredExp)
        {
            curExp -= RequiredExp;
            curlvl++;
        }
    }
}

