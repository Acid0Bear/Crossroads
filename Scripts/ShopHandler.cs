using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Cars
{
    public class ShopHandler : MonoBehaviourPunCallbacks
    {
        public List<CarBelongings> CarBelong = new List<CarBelongings>();
        public GameObject SelectedCar;
        public GameObject PatternsSelector, ClosePatterns;
        public GameObject PatternPiece;
        public GameObject carPiece;
        public GameObject OwnedCarPiece, FreeGarageSpace;
        public Transform PatternList;
        public Transform CarShopList;
        public Transform OwnedCarsList;
        public Transform SelectedCarPivot;
        public GameObject Processing, ProcClose;
        public TextMeshProUGUI proc;
        public RectTransform loading;
        int ShopResult = 0;

        List<GameObject> PatPieces = new List<GameObject>();
        List<GameObject> CarPieces = new List<GameObject>();
        List<GameObject> OwnedCars = new List<GameObject>();
        public GameObject CarinfoPanel;
        public CarInfoHolder CarInfo;
        [HideInInspector]
        public static ShopHandler ShopHand;
        private void Awake()
        {
            MainMenu.menu.CarBelong = CarBelong;
            if (ShopHand == null)
                ShopHand = this;
        }
        public void GetCarinfo(int CarID)
        {
            CarinfoPanel.SetActive(true);
            CarInfo.Fill(CarInfo.light,MainMenu.menu.lang.carsWeapsDesc[CarID].light);
            CarInfo.Fill(CarInfo.main, MainMenu.menu.lang.carsWeapsDesc[CarID].main);
            CarInfo.Fill(CarInfo.heavy, MainMenu.menu.lang.carsWeapsDesc[CarID].heavy);
            CarInfo.Fill(CarInfo.special, MainMenu.menu.lang.carsWeapsDesc[CarID].special);
            CarInfo.Fill(CarInfo.gadget, MainMenu.menu.lang.carsWeapsDesc[CarID].gadget);
        }
        public void updateShop()
        {
            if (CarPieces.Count > 0)
            {
                foreach (GameObject piece in CarPieces)
                    Destroy(piece);
                CarPieces.Clear();
            }
            for (int i = 0; i < MainMenu.menu.cars.Length; i++)
            {
                var Piece = Instantiate(carPiece, CarShopList);
                CarPieces.Add(Piece);
                var script = Piece.GetComponent<CarInShopPiece>();
                bool isOwned = (MainMenu.menu.cars[i][0] == '1') ? true : false;
                script.SetupPiece(CarBelong[i].Name, CarBelong[i].GolPrice, CarBelong[i].SilPrice, isOwned, i);
            }
        }
        public void updateOwnedCars()
        {
            if (SelectedCar != null) { Destroy(SelectedCar); CarController.Carcontroller = null; }
            if (PlayerPrefs.HasKey("SelectedCar"))
            {
                SelectedCar = Instantiate(CarBelong[PlayerPrefs.GetInt("SelectedCar")].Body, SelectedCarPivot);
                PlayerP.PlayerPresets.UpdateCarLook();
            }
            else
            {
                PlayerPrefs.SetInt("SelectedCar", 0);
                SelectedCar = Instantiate(CarBelong[0].Body, SelectedCarPivot);
                PlayerP.PlayerPresets.UpdateCarLook();
            }
            if (OwnedCars.Count > 0)
            {
                foreach (GameObject piece in OwnedCars)
                    Destroy(piece);
                OwnedCars.Clear();
            }
            for (int i = 0; i < MainMenu.menu.cars.Length; i++)
            {
                if (MainMenu.menu.cars[i][0] == '1')
                {
                    var Piece = Instantiate(OwnedCarPiece, OwnedCarsList);
                    OwnedCars.Add(Piece);
                    var script = Piece.GetComponent<OwnedCar>();
                    script.SetUpOwned(i, CarBelong[i].Name);
                }
            }
            if(OwnedCars.Count < 5)
            {
                int NumOfSpaces = 5 - OwnedCars.Count;
                for (int i = 0; i < NumOfSpaces; i++)
                {
                    var Piece = Instantiate(FreeGarageSpace, OwnedCarsList);
                    OwnedCars.Add(Piece);
                }
            }
        }
        public void UpdatePatternsList(int CarId)
        {
            PatternsSelector.SetActive(true);
            ClosePatterns.SetActive(true);
            if (PatPieces.Count > 0)
            {
                foreach (GameObject piece in PatPieces)
                    Destroy(piece);
                PatPieces.Clear();
            }
            var Piece = Instantiate(PatternPiece, PatternList);
            PatPieces.Add(Piece);
            var button = Piece.GetComponent<Button>();
            button.onClick.AddListener(() => PlayerP.PlayerPresets.SelectPattern(CarBelong[CarId].DefaultPattern));
            for (int i = 0; i < CarBelong[CarId].prices.Count; i++)
            {
                Piece = Instantiate(PatternPiece, PatternList);
                PatPieces.Add(Piece);
                if (MainMenu.menu.cars[CarId][i + 1] == '0')
                {
                    var script = Piece.GetComponent<PatternPiece>();
                    script.setBlock(CarBelong[CarId].prices[i], this, CarId, i + 1);
                }
                button = Piece.GetComponent<Button>();
                var Img = Piece.GetComponentInChildren<RawImage>();
                Img.texture = CarController.Carcontroller.Patterns[i+1];
                Color col = new Color();
                col.a = 255;
                col.r = 255;
                col.g = 255;
                col.b = 255;
                Img.color = col;
                var text = Piece.GetComponentInChildren<TextMeshProUGUI>();
                var num = (i + 1);
                text.text = "";
                button.onClick.AddListener(() => PlayerP.PlayerPresets.SelectPattern(num));
            }
        }
        public void BuyPiece(int price, int CarId, int pieceId)
        {
            string CarName = "";
            switch (CarId)
            {
                case 0: CarName = "Agera"; break;
                case 1: CarName = "GT500"; break;
                case 2: CarName = "Thunder"; break;
            }
            PhotonNetwork.WebRpc("ShopReq", JsonUtility.ToJson(new BuyQueue { Price = price, car = CarName, piece = pieceId, Username = MainMenu.menu.nickname }));
            StartCoroutine(ShopResponce(CarId, pieceId, price));
            Processing.SetActive(true);
            proc.text = MainMenu.menu.lang.Processing[1];
        }
        public override void OnWebRpcResponse(OperationResponse response)
        {
            object Php, Message;
            response.Parameters.TryGetValue(209, out Php);
            response.Parameters.TryGetValue(206, out Message);
            string Adress = (string)Php;
            string MSG = (string)Message;
            if (Adress == "ShopReq" && MSG == "Not enough money")
            {
                proc.text = MainMenu.menu.lang.Processing[2];
                ShopResult = 2;
            }
            else if (Adress == "ShopReq" && MSG == "Something went wrong")
            {
                proc.text = MainMenu.menu.lang.Processing[3];
                ShopResult = 2;
            }
            else if (Adress == "ShopReq" && MSG == "Success!")
                ShopResult = 1;
        }
        IEnumerator ShopResponce(int CarID, int PatternID, int price)
        {
            while (true)
            {
                if (ShopResult == 1)
                {
                    Debug.Log("Yay");
                    string tmp = MainMenu.menu.cars[CarID];
                    var aga = tmp.ToCharArray();
                    aga[PatternID] = '1';
                    tmp = new string(aga);
                    MainMenu.menu.cars[CarID] = tmp;
                    if(price > 0)
                    MainMenu.menu.prog.Silver -= price;
                    else
                    {
                        price *= -1;
                        MainMenu.menu.prog.Gold -= price;
                    }
                    MainMenu.menu.AssignGUI();
                    if(PatternsSelector.activeSelf)
                    UpdatePatternsList(PlayerPrefs.GetInt("SelectedCar"));
                    updateOwnedCars();
                    updateShop();
                    PlayerP.PlayerPresets.UpdateCarLook();
                    ShopResult = 0;
                    Processing.SetActive(false);
                    break;
                }
                else if (ShopResult == 2)
                {
                    Debug.Log("Error");
                    ProcClose.SetActive(true);
                    ShopResult = 0;
                    break;
                }
                yield return null;
            }
        }
    }
}
[System.Serializable]
public class CarBelongings
{
    public List<int> prices;
    public int DefaultPattern;
    public string Name;
    public int SilPrice;
    public int GolPrice;
    public GameObject Body;
}
[System.Serializable]
public class BuyQueue
{
    public string car, Username;
    public int piece,Price;
}