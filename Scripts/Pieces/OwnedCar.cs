using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Cars {
    public class OwnedCar : MonoBehaviour
    {
        public Button patterns, SelectCar;
        public int CarID;
        public string CarName;
        public TextMeshProUGUI CarNameHandler;
        public AudioClip onclick;

        public void SetUpOwned(int id, string name)
        {
            CarID = id;
            CarName = name;
            CarNameHandler.text = CarName;
            patterns.onClick.AddListener(() => ShopHandler.ShopHand.UpdatePatternsList(PlayerPrefs.GetInt("SelectedCar")));
            patterns.onClick.AddListener(() => MainMenu.menu.PlaySingle(onclick));
            SelectCar.onClick.AddListener(() => GetCar());
        }

        void GetCar()
        {
            if (ShopHandler.ShopHand.SelectedCar != null)
                Destroy(ShopHandler.ShopHand.SelectedCar);
            CarController.Carcontroller = null;
            ShopHandler.ShopHand.SelectedCar = Instantiate(ShopHandler.ShopHand.CarBelong[CarID].Body, ShopHandler.ShopHand.SelectedCarPivot);
            if(ShopHandler.ShopHand.PatternsSelector.activeSelf)
            ShopHandler.ShopHand.UpdatePatternsList(CarID);
            PlayerPrefs.SetInt("SelectedCar", CarID);
            PlayerP.PlayerPresets.PresetCode = PlayerPrefs.GetString(ShopHandler.ShopHand.CarBelong[CarID].Name);
            PlayerP.PlayerPresets.UpdateCarLook();
        }
    }
}
