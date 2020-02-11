using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Cars
{
    public class SettingHandler : MonoBehaviour
    {
        public Slider Music, SFX, Cars, Weapons, UIscale;
        float MusicVol, SFXVol, CarsVol, WeapVol;
        public TMP_Dropdown Quality;
        public List<TextMeshProUGUI> Labels = new List<TextMeshProUGUI>();
        public Button Apply_, Discard, Exit;
        public AudioClip click;
        int QualityLevel, UI_Scale;
        private void Awake()
        {
            Apply_.onClick.AddListener(() => MainMenu.menu.PlaySingle(click));
            Discard.onClick.AddListener(() => MainMenu.menu.PlaySingle(click));
            Exit.onClick.AddListener(() => MainMenu.menu.PlaySingle(click));
        }
        public void SetMusicVolume()
        {
            PlayerPrefs.SetFloat("MainVolume", Music.value);
        }
        public void SetSFXVolume()
        {
            PlayerPrefs.SetFloat("SfxVol", SFX.value);
        }
        public void SetCarsVolume()
        {
            PlayerPrefs.SetFloat("CarsVolume", Cars.value);
        }
        public void SetWeaponsVolume()
        {
            PlayerPrefs.SetFloat("WeaponsVolume", Weapons.value);
        }
        public void SetGraphics()
        {
            QualityLevel = Quality.value;
            PlayerPrefs.SetInt("Quality", QualityLevel);
            QualitySettings.SetQualityLevel(QualityLevel);
        }
        public void SetUIScale()
        {
            PlayerPrefs.SetInt("UIScale", (int)UIscale.value);
        }
        public void FetchSettingsLang()
        {
            for (int i = 0; i < Labels.Count; i++)
            {
                Labels[i].text = MainMenu.menu.lang.Settings[i];
            }
        }

        public void GetValues()
        {
            FetchSettingsLang();
            MusicVol = PlayerPrefs.GetFloat("MainVolume");
            Music.value = MusicVol;
            SFXVol = PlayerPrefs.GetFloat("SfxVol");
            SFX.value = SFXVol;
            CarsVol = PlayerPrefs.GetFloat("CarsVolume");
            Cars.value = CarsVol;
            WeapVol = PlayerPrefs.GetFloat("WeaponsVolume");
            Weapons.value = WeapVol;
            QualityLevel = PlayerPrefs.GetInt("Quality");
            Quality.value = QualityLevel;
            QualitySettings.SetQualityLevel(QualityLevel);
            UI_Scale = PlayerPrefs.GetInt("UIScale");
            if(UI_Scale > 4)
                PlayerPrefs.SetInt("UIScale", 4);
            UIscale.value = UI_Scale;
        }

        public void Cancel()
        {
            Music.value = MusicVol;
            SFX.value = SFXVol;
            Cars.value = CarsVol;
            Weapons.value = WeapVol;
            UIscale.value = UI_Scale;
            PlayerPrefs.SetFloat("MainVolume", MusicVol);
            PlayerPrefs.SetFloat("SfxVol", SFXVol);
            PlayerPrefs.SetFloat("CarsVolume", CarsVol);
            PlayerPrefs.SetFloat("WeaponsVolume", WeapVol);
            PlayerPrefs.SetInt("UIScale", UI_Scale);
        }
        public void Apply()
        {
            MusicVol = PlayerPrefs.GetFloat("MainVolume");
            Music.value = MusicVol;
            SFXVol = PlayerPrefs.GetFloat("SfxVol");
            SFX.value = SFXVol;
            CarsVol = PlayerPrefs.GetFloat("CarsVolume");
            Cars.value = CarsVol;
            WeapVol = PlayerPrefs.GetFloat("WeaponsVolume");
            Weapons.value = WeapVol;
            UI_Scale = PlayerPrefs.GetInt("UIScale");
            UIscale.value = UI_Scale;
        }
    }
}
