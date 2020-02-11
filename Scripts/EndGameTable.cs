using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Cars
{
    public class EndGameTable : MonoBehaviour
    {
        public static EndGameTable EGT;
        public GameObject Results;
        public List<UserPiece> pieces = new List<UserPiece>();
        List<int> Players = new List<int>();
        public TextMeshProUGUI EarnedExp, EarnedMoney;
        public TextMeshProUGUI MVP_N, MVP_KDA, KILLER_Name, KILLER_KillsDamage;
        public Button Leave;
        int Killer, MVP;
        private void Awake()
        {
            if (EGT == null)
            {
                EGT = this;
            }
            else
                Destroy(this);
        }
        public void CalculateScores()
        {
            int MostKills = 0, MostDMG = 0, Killer = 0;
            int MostKillss = 0, MVP = 0;
            float BestKDA = 0;
            for (int i = 1; i < 9; i++)
            {
                Players.Add(i);
                if (GameLogic.gameLogic.statHandler.stats[i].NickName != "")
                {
                    GameLogic.gameLogic.statHandler.stats[i].Score = GameLogic.gameLogic.statHandler.stats[i].Kills * 100 + GameLogic.gameLogic.statHandler.stats[i].Assists * 50 + GameLogic.gameLogic.statHandler.stats[i].DamageDealt;
                    if (GameLogic.gameLogic.statHandler.stats[i].Deaths != 0)
                        GameLogic.gameLogic.statHandler.stats[i].KDA = GameLogic.gameLogic.statHandler.stats[i].Kills / GameLogic.gameLogic.statHandler.stats[i].Deaths;
                    else
                        GameLogic.gameLogic.statHandler.stats[i].KDA = GameLogic.gameLogic.statHandler.stats[i].Kills / 1;
                    if (GameLogic.gameLogic.statHandler.stats[i].Kills > MostKills && GameLogic.gameLogic.statHandler.stats[i].DamageDealt > MostDMG)
                        Killer = i;
                    if (GameLogic.gameLogic.statHandler.stats[i].Kills > MostKillss && GameLogic.gameLogic.statHandler.stats[i].KDA > BestKDA)
                        MVP = i;
                }
            }
            Results.SetActive(true);
            MVP_N.text = GameLogic.gameLogic.statHandler.stats[MVP].NickName;
            MVP_KDA.text = GameLogic.gameLogic.statHandler.stats[MVP].KDA.ToString("0.0");
            KILLER_Name.text = GameLogic.gameLogic.statHandler.stats[Killer].NickName;
            KILLER_KillsDamage.text = string.Format("Kills:{0}\nDamage:{1}", GameLogic.gameLogic.statHandler.stats[Killer].Kills, GameLogic.gameLogic.statHandler.stats[Killer].DamageDealt);
            Leave.onClick.AddListener(() => MainMenu.menu.LeaveRoomOrCancelSearch());
            for (int i = 1; i < 9; i++)
            {
                int Best = -1;
                int Temp = 0;
                foreach (int j in Players)
                {
                    if (Best < GameLogic.gameLogic.statHandler.stats[j].Score)
                    {
                        Best = GameLogic.gameLogic.statHandler.stats[j].Score;
                        Temp = j;
                    }
                }
                Players.Remove(Temp);
                if (i < 4 && GameLogic.gameLogic.statHandler.stats[Temp].NickName != "")
                    GameLogic.gameLogic.statHandler.stats[Temp].Score += Mathf.RoundToInt(1000 / i);
                pieces[i - 1].Name.text = GameLogic.gameLogic.statHandler.stats[Temp].NickName;
                pieces[i - 1].Score.text = GameLogic.gameLogic.statHandler.stats[Temp].Score.ToString();
                pieces[i - 1].Kills.text = GameLogic.gameLogic.statHandler.stats[Temp].Kills.ToString();
                pieces[i - 1].Deaths.text = GameLogic.gameLogic.statHandler.stats[Temp].Deaths.ToString();
                pieces[i - 1].KDA.text = GameLogic.gameLogic.statHandler.stats[Temp].KDA.ToString("0.0");
            }
            EarnedExp.text = "+" + GameLogic.gameLogic.statHandler.stats[PhotonNetwork.LocalPlayer.ActorNumber].Score;
            MainMenu.menu.prog.curExp += GameLogic.gameLogic.statHandler.stats[PhotonNetwork.LocalPlayer.ActorNumber].Score;
            EarnedMoney.text = "+" + Mathf.RoundToInt(GameLogic.gameLogic.statHandler.stats[PhotonNetwork.LocalPlayer.ActorNumber].Score / 6);
            MainMenu.menu.prog.Silver += Mathf.RoundToInt(GameLogic.gameLogic.statHandler.stats[PhotonNetwork.LocalPlayer.ActorNumber].Score / 6);
        }
    }
}

[System.Serializable]
public class UserPiece
{
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Score;
    public TextMeshProUGUI Kills;
    public TextMeshProUGUI Deaths;
    public TextMeshProUGUI KDA;
}