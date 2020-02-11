using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Globalization;

namespace Cars
{
    public class PlayerP : MonoBehaviour
    {
        public static PlayerP PlayerPresets;
        public string PresetCode;
        public GameObject ColorCanvas;
        int Pattern = 0;
        private Material NewOne;
        public Color Color;

        private void Awake()
        {
            if (PlayerPresets == null)
            {
                PlayerPresets = this;
            }
            else
                Destroy(this);
        }
        void Start()
        {
            StartCoroutine(WaitForLogin());
        }
        IEnumerator WaitForLogin()
        {
            while (true)
            {
                if(ShopHandler.ShopHand.SelectedCar != null)
                {
                    UpdateCarLook();
                    break;
                }
                yield return null;
            }
        }
        public void UpdateCarLook()
        {
            if (PlayerPrefs.HasKey(ShopHandler.ShopHand.CarBelong[PlayerPrefs.GetInt("SelectedCar")].Name))
            {
                PresetCode = PlayerPrefs.GetString(ShopHandler.ShopHand.CarBelong[PlayerPrefs.GetInt("SelectedCar")].Name);
                var PD = ParseCode(PresetCode);
                if (PD.PatternID > CarController.Carcontroller.Patterns.Count - 1)
                {
                    GetCode(Color);
                    PlayerPrefs.SetString(ShopHandler.ShopHand.CarBelong[PlayerPrefs.GetInt("SelectedCar")].Name, PresetCode);
                }
                NewOne = new Material(CarController.Carcontroller.CarMat);
                Pattern = PD.PatternID;
                NewOne.SetTexture("_CarPattern", CarController.Carcontroller.Patterns[PD.PatternID]);
                Color = PD.color;
                NewOne.SetColor("_BaseColor", Color);
                CarController.Carcontroller.gameObject.GetComponent<MeshRenderer>().material = NewOne;
            }
            else
            {
                GetCode(Color);
                PlayerPrefs.SetString(ShopHandler.ShopHand.CarBelong[PlayerPrefs.GetInt("SelectedCar")].Name, PresetCode);
            }
        }
        public void ColorSelector()
        {
            if(ColorCanvas.activeSelf == true)
                ColorCanvas.SetActive(false);
            else
                ColorCanvas.SetActive(true);
        }

        public void SelectPattern(int Num)
        {
            Debug.Log("Attempting to set pattern number - " + Num);
            Pattern = Num;
            NewOne = new Material(CarController.Carcontroller.CarMat);
            NewOne.SetTexture("_CarPattern", CarController.Carcontroller.Patterns[Pattern]);
            PresetCode = PlayerPrefs.GetString(ShopHandler.ShopHand.CarBelong[PlayerPrefs.GetInt("SelectedCar")].Name);
            var PD = ParseCode(PresetCode);
            Color = PD.color;
            NewOne.SetColor("_BaseColor", Color);
            CarController.Carcontroller.gameObject.GetComponent<MeshRenderer>().material = NewOne;
            GetCode(Color);
            PlayerPrefs.SetString(ShopHandler.ShopHand.CarBelong[PlayerPrefs.GetInt("SelectedCar")].Name, PresetCode);
        }

        public void UpdateColor(Color Color_)
        {
            NewOne = CarController.Carcontroller.gameObject.GetComponent<MeshRenderer>().material;
            NewOne.SetColor("_BaseColor", Color_);
            CarController.Carcontroller.gameObject.GetComponent<MeshRenderer>().material = NewOne;
            GetCode(Color_);
            PlayerPrefs.SetString(ShopHandler.ShopHand.CarBelong[PlayerPrefs.GetInt("SelectedCar")].Name, PresetCode);
        }

        void GetCode(Color Col)
        {
            PresetCode = string.Format("P@{0}%R@{1}%G@{2}%B@{3}%", Pattern, Col.r.ToString(CultureInfo.InvariantCulture), Col.g.ToString(CultureInfo.InvariantCulture), Col.b.ToString(CultureInfo.InvariantCulture));
        }

        CarCustom ParseCode(string Code)
        {
            string Pattern = "", R = "" , G ="",B="";
            int P_Pattern;
            float P_R, P_G,P_B;
            CarCustom Result = new CarCustom();
            Color col = Color.red;
            for (int i = 0; i < Code.Length; i++)
            {
                if (Code[i] == 'P')
                    for (int j = i + 2; ; j++)
                    {
                        if (Code[j] == '%')
                            break;
                        Pattern += Code[j];
                    }
                if (Code[i] == 'R')
                    for (int j = i + 2; ; j++)
                    {
                        if (Code[j] == '%')
                            break;
                        R += Code[j];
                    }
                if (Code[i] == 'G')
                    for (int j = i + 2; ; j++)
                    {
                        if (Code[j] == '%')
                            break;
                        G += Code[j];
                    }
                if (Code[i] == 'B')
                    for (int j = i + 2; ; j++)
                    {
                        if (Code[j] == '%')
                            break;
                        B += Code[j];
                    }
            }
            P_Pattern = int.Parse(Pattern);
            P_R = float.Parse(R, CultureInfo.InvariantCulture);
            P_G = float.Parse(G, CultureInfo.InvariantCulture);
            P_B = float.Parse(B, CultureInfo.InvariantCulture);
            Result.PatternID = P_Pattern;
            col.r = P_R; col.g = P_G; col.b = P_B;
            Result.color = col;
            return Result;
        }
        public CarCustom GetParsed()
        {
            return ParseCode(PresetCode);
        }


        public void PaintCar(int Num, string Code, Color Col)
        {
            var car = PhotonView.Find((Num * 1000) + 1).gameObject;
            var PD = ParseCode(Code);
            NewOne = new Material(car.GetComponent<MeshRenderer>().material);
            NewOne.SetTexture("_CarPattern", CarController.Carcontroller.Patterns[PD.PatternID]);
            NewOne.SetColor("_BaseColor", Col);
            car.GetComponent<MeshRenderer>().material = NewOne;
        }
    }
    [System.Serializable]
    public class CarCustom
    {
        public int PatternID;
        public Color color;
    }
}
