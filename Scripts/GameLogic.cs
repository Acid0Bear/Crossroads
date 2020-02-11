using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace Cars
{
    public class GameLogic : MonoBehaviour
    {
        public StatsBody statHandler = new StatsBody{ stats = new StatHandler[9] };
        public static GameLogic gameLogic;
        public int kills = 0;
        public float GameTimer = 900f;
        float GetReady = 10f;
        bool StatsSended = false, StatsCalculated = false;
        public GameObject[] modes;
        public bool IsGameStarted = false;
        //public GameObject EndOfGameWindow;
        public TextMeshProUGUI TimerHandler;

        private void Awake()
        {
            if (gameLogic == null)
            {
                gameLogic = this;
            }
            else
            {
                Destroy(this);
            }
            switch (PhotonRoom.photonroom.Mode)
            {
                case "DM":
                    modes[0].SetActive(true);
                    break;
            }
        }
        private void Update()
        {
            if (GetReady > 0 && GameTimer == 900f && MainMenu.menu.delaystart)
            {
                CarController.Carcontroller.health.isAlive = false;
                if (!PhotonNetwork.IsMasterClient) return;
                GetReady = (GetReady > 0) ? GetReady - Time.deltaTime : 0;
                TimerHandler.text = string.Format("TIME TO START:\n{0}", Mathf.RoundToInt(GetReady));
                if (GetReady <= 0) CarController.Carcontroller.health.isAlive = true;
                PhotonView PV = PhotonView.Get(this);
                PV.RPC("SyncStartDelay", RpcTarget.Others, GetReady);
            }
            else if(!MainMenu.menu.delaystart && PhotonNetwork.PlayerList.Length < 2)
            {
                TimerHandler.text = "AWAITING FOR\nPLAYERS";
            }
            else {
                GameTimer = (GameTimer > 0 && IsGameStarted) ? GameTimer - Time.deltaTime : GameTimer;
                TimerHandler.text = string.Format("{0}:{1}", Mathf.FloorToInt(GameTimer / 60).ToString("00"), (GameTimer % 60).ToString("00"));
            }
            switch (PhotonRoom.photonroom.Mode)
            {
                case "DM":
                    GetTop3();
                    if (GameTimer <= 0 || kills == 3)
                    {
                        EndOfGame();
                    }
                    break;
            }
        }
        void GetTop3()
        {
            List<int> players = new List<int>();
            for (int i = 1; i < 9; i++)
            {
                players.Add(i);
            }
            for (int i = 0; i < 3; i++)
            {
                int TopKills = -1;
                int num = -1;
                for (int j = 1; j < 9; j++)
                {
                    if (statHandler.stats[j].Kills > TopKills && players.Contains(j))
                    {
                        TopKills = statHandler.stats[j].Kills;
                        num = j;
                    }
                }
                players.Remove(num);
                DMMode.DM.Nicknames[i].text = statHandler.stats[num].NickName;
                DMMode.DM.Kills[i].text = statHandler.stats[num].Kills.ToString();
            }
            DMMode.DM.kills.text = string.Format("{0}:{1}",MainMenu.menu.lang.DM[1], kills.ToString());
        }
        void EndOfGame()
        {
            CarController.Carcontroller.health.isAlive = false;
            if (!StatsCalculated)
            {
                EndGameTable.EGT.CalculateScores();
                StatsCalculated = true;
            }
            if (PhotonNetwork.IsMasterClient)
            {
                if (!StatsSended)
                {
                    PhotonNetwork.WebRpc("StatsAnalyzer", JsonUtility.ToJson(statHandler));
                    PhotonView PV = PhotonView.Get(this);
                    PV.RPC("SendStats", RpcTarget.All);
                }
            }
        }
        [PunRPC]
        void SendStats()
        {
            StatsSended = true;
        }
        [PunRPC]
        void SyncStartDelay(float TimeToStart)
        {
            TimerHandler.text = string.Format("Time to start - {0}", Mathf.RoundToInt(TimeToStart));
            GetReady = TimeToStart;
            if (TimeToStart <= 0) CarController.Carcontroller.health.isAlive = true;
        }
    }
}
[System.Serializable]
public class StatHandler
{
    public string NickName;
    public int Kills, Assists, Deaths, DamageDealt, Score;
    public float KDA;
}
[System.Serializable]
public class StatsBody
{
    public StatHandler[] stats;
}

