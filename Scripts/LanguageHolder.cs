using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace Cars
{
    public class LanguageHolder
    {
            public string[] FriendsSection = new string[3];
            public string[] ToBattle = new string[2];
            public string Maps;
            public string[] ModeDesc = new string[1];
            public string[] Modes = new string[1];
            public string[] ScoreTable = new string[5];
            public string[] Matchmaking = new string[5];
            public string[] Settings = new string[13];
            public string[] DM = new string[2];
            public string[] Processing = new string[4];
            public string[] Login = new string[7];
            public string[] QuickMenu = new string[3];
            public CarsWeapsDesc[] carsWeapsDesc = new CarsWeapsDesc[3];
    }
}
[System.Serializable]
public class CarsWeapsDesc
{
    public string CarName;
    public string[] light = new string[3];
    public string[] main = new string[3];
    public string[] heavy = new string[3];
    public string[] special = new string[3];
    public string[] gadget = new string[3];
}
[System.Serializable]
public class CarInfoHolder
{
    public TextMeshProUGUI[] light = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] main = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] heavy = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] special = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] gadget = new TextMeshProUGUI[3];

    public void Fill(TextMeshProUGUI[] target, string[] source)
    {
        for(int i = 0; i < 3; i++)
        {
            target[i].text = source[i];
        }
    }
}
