using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.SceneManagement;
using TMPro;
using System;
namespace Cars
{
    public class LoginMenu : MonoBehaviourPunCallbacks
    {
        public GameObject LoginPanel;
        public TMP_InputField user_name, user_password, user_passRepeat, user_passwordreg;
        public TextMeshProUGUI error_text;
        public List<TextMeshProUGUI> GUI = new List<TextMeshProUGUI>();
        private int code;
        private int rank;
        private string username, pass, passRepeat;
        public GameObject Processing, ProcClose;
        public TextMeshProUGUI proc;
        public RectTransform loading;
        private void Start()
        {
            if (PhotonNetwork.IsConnectedAndReady)
            {
                LoginPanel.SetActive(false);
            }
            if (PlayerPrefs.HasKey("Username") && PlayerPrefs.HasKey("Pass") && MainMenu.menu.Logout == false)
            {
                username = PlayerPrefs.GetString("Username");
                pass = PlayerPrefs.GetString("Pass");
                Login();
            }
            for(int i = 0; i < GUI.Count; i++)
            {
                GUI[i].text = MainMenu.menu.lang.Login[i];
            }
        }

        private void Update()
        {
            var loc = loading.localRotation;
            loc.x += 5;
            loading.localRotation = loc;
        }

        public void UserName()
        {
            username = user_name.text.ToString();
        }
        public void UserRegPassword()
        {
            pass = user_passwordreg.text.ToString();
        }

        public void UserPassword()
        {
            pass = user_password.text.ToString();
        }
        public void UserPasswordRepeat()
        {
            passRepeat = user_passRepeat.text.ToString();
        }
        public void Login()
        {
            if (pass != passRepeat && code == 1)
            {
                error_text.text = "Passwords dont match!";
                Debug.Log(pass);
                Debug.Log(passRepeat);
                return;
            }
            if (pass == "" || username == "" || pass == null || username == null)
            {
                error_text.text = "Fields can't be empty!";
                return;
            }
            error_text.text = "";
            AuthenticationValues customAuth = new AuthenticationValues();
            customAuth.AuthType = CustomAuthenticationType.Custom;
            customAuth.AddAuthParameter("username", username);
            customAuth.AddAuthParameter("password", pass);
            customAuth.AddAuthParameter("code", code.ToString());
            PhotonNetwork.AuthValues = customAuth;
            if (PlayerPrefs.HasKey("ID"))
                PhotonNetwork.AuthValues.UserId = "Id" + PlayerPrefs.GetString("ID");
            PhotonNetwork.ConnectUsingSettings();
            Processing.SetActive(true);
            proc.text = MainMenu.menu.lang.Processing[0];
            if (!PlayerPrefs.HasKey("Username") && !PlayerPrefs.HasKey("Pass") || PlayerPrefs.GetString("Username") != username || PlayerPrefs.GetString("Pass") != pass)
            {
                PlayerPrefs.SetString("Username", username);
                PlayerPrefs.SetString("Pass", pass);
            }
        }
        public override void OnConnected()
        {
            if (code == 1)
            {
                PhotonNetwork.Disconnect();
                AuthenticationValues customAuth = new AuthenticationValues();
                customAuth.AuthType = CustomAuthenticationType.Custom;
                customAuth.AddAuthParameter("username", username);
                customAuth.AddAuthParameter("password", pass);
                customAuth.AddAuthParameter("code", "0");
                PhotonNetwork.AuthValues = customAuth;
                PhotonNetwork.AuthValues.UserId = "Id" + PlayerPrefs.GetString("ID");
                PhotonNetwork.ConnectUsingSettings();
            }
            if (MainMenu.menu.Logout)
            {
                return;
            }
            Debug.Log("Connected to master");
            MainMenu.menu.nickname = username;
            MainMenu.menu.AssignGUI();
            PhotonNetwork.NickName = username;
            Processing.SetActive(false);
            LoginPanel.SetActive(false);
            ShopHandler.ShopHand.updateOwnedCars();
        }


        public override void OnCustomAuthenticationFailed(string debugMessage)
        {
            proc.text = debugMessage;
            ProcClose.SetActive(true);
        }

        public void SetCode()
        {
            code = 1;
            pass = "";
        }
        public void RevertCode()
        {
            code = 0;
        }
        string ConvertObjectToString(object obj)
        {
            return obj?.ToString() ?? string.Empty;
        }

        public override void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
            var keys = data.Keys;
            object raw = null;
            foreach (string j in keys)
            {
                var temps = data.TryGetValue(j, out raw);
                //Debug.Log(string.Format("IDHIDED -> {0}  NameHIDED -> {1}", j, (string)NotSoRaw[0]));
                if (j == "0")
                    PlayerPrefs.SetString("ID", (string)raw);
                else if (j == "1")
                {
                    MainMenu.menu.CurRoom = (string)raw;
                }
                else if (j == "2")
                {
                    var str = (string)raw;
                    string temp = "";
                    for (int i = 0; i < str.Length; i++)
                    {
                        if (str[i] != '/')
                            temp += str[i];
                        else
                        {
                            var CurValue = int.Parse(temp);
                            temp = "";
                            i++;
                            for (; i < str.Length; i++)
                            {
                                if (str[i] != ':')
                                    temp += str[i];
                                else
                                {
                                    MainMenu.menu.prog.RequiredExp = int.Parse(temp);
                                    MainMenu.menu.prog.curExp = CurValue;
                                    i++;
                                    temp = "";
                                    for (; i < str.Length; i++)
                                    {
                                        temp += str[i];
                                    }
                                    MainMenu.menu.prog.curlvl = int.Parse(temp);
                                }
                            }
                        }
                    }
                }
                else if (j == "3")
                {
                    var Silver = (string)raw;
                    MainMenu.menu.prog.Silver = int.Parse(Silver);
                }
                else if (j == "4")
                {
                    var Gold = (string)raw;
                    MainMenu.menu.prog.Gold = int.Parse(Gold);
                }
                else if (j == "5")
                {
                    var obj = Array.ConvertAll<object, string>((object[])raw, ConvertObjectToString);
                    string[] cars = new string[obj.Length - 1];
                    for (int i = 1; i < obj.Length; i++)
                    {
                        cars[i - 1] = obj[i];
                    }
                    MainMenu.menu.cars = cars;
                }
            }
            if (MainMenu.menu.Logout)
            {
                MainMenu.menu.ReloadScene();
            }
        }
    }
}
