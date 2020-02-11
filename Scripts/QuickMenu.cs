using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Cars
{
    public class QuickMenu : MonoBehaviour
    {
        public Button QuickMenuOpen, cont, settings, exit;
        public TextMeshProUGUI Contt, sett, exitt;
        public AudioClip click;
        public RectTransform Panel;
        public GameObject SettingsPanel, QuickPanel;
        public CanvasScaler JoystickCanv;
        public Joystick joy;
        int CurScale;
        private void Awake()
        {
            if (PlayerPrefs.HasKey("UIScale"))
            {
                switch (PlayerPrefs.GetInt("UIScale"))
                {
                    case 4: JoystickCanv.referenceResolution = new Vector2(800, 600); break;
                    case 3: JoystickCanv.referenceResolution = new Vector2(900, 600); break;
                    case 2: JoystickCanv.referenceResolution = new Vector2(1000, 600); break;
                    case 1: JoystickCanv.referenceResolution = new Vector2(1100, 600); break;
                }
                CurScale = PlayerPrefs.GetInt("UIScale");
            }
                
            else
            {
                PlayerPrefs.SetInt("UIScale", 4);
                CurScale = 4;
                JoystickCanv.referenceResolution = new Vector2(800, 600);
            }
            exit.onClick.AddListener(() => MainMenu.menu.LeaveRoomOrCancelSearch());
            exit.onClick.AddListener(() => MainMenu.menu.PlaySingle(click));
            QuickMenuOpen.onClick.AddListener(() => SetupQuickMenu());
            QuickMenuOpen.onClick.AddListener(() => MainMenu.menu.PlaySingle(click));
            settings.onClick.AddListener(() => MainMenu.menu.PlaySingle(click));
            cont.onClick.AddListener(() => MainMenu.menu.PlaySingle(click));
        }
        private void Update()
        {
            if(QuickPanel.activeSelf && !SettingsPanel.activeSelf &&Input.GetMouseButton(0) && !RectTransformUtility.RectangleContainsScreenPoint(Panel, Input.mousePosition))
            {
                QuickPanel.SetActive(false);
                MainMenu.menu.PlaySingle(click);
                joy.IsReady = true;
            }
            else if(!QuickPanel.activeSelf && !joy.IsReady)
                joy.IsReady = true;
            if(CurScale != PlayerPrefs.GetInt("UIScale"))
            {
                switch (PlayerPrefs.GetInt("UIScale"))
                {
                    case 1: JoystickCanv.referenceResolution = new Vector2(800, 600); break;
                    case 2: JoystickCanv.referenceResolution = new Vector2(900, 600); break;
                    case 3: JoystickCanv.referenceResolution = new Vector2(1000, 600); break;
                    case 4: JoystickCanv.referenceResolution = new Vector2(1100, 600); break;
                }
                CurScale = PlayerPrefs.GetInt("UIScale");
            }
        }
        public void SetupQuickMenu()
        {
            joy.IsReady = false;
            Contt.text = MainMenu.menu.lang.QuickMenu[0];
            sett.text = MainMenu.menu.lang.QuickMenu[1];
            exitt.text = MainMenu.menu.lang.QuickMenu[2];
        }
    }
}