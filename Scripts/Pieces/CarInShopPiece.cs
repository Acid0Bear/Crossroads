using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Cars {
    public class CarInShopPiece : MonoBehaviour
    {
        public int GolPrice, SilPrice, CarID;
        public bool IsOwned;
        public TextMeshProUGUI gol, sil, CarName;
        public Button BuySilv, BuyGold, ShowCar, GetInfo;
        public GameObject prices, Owned;
        public void SetupPiece(string carName, int golPrice, int silPrice, bool isOwned, int Car_id)
        {
            CarID = Car_id;
            GolPrice = golPrice;
            SilPrice = silPrice;
            gol.text = GolPrice.ToString();
            sil.text = SilPrice.ToString();
            CarName.text = carName;
            GetInfo.onClick.AddListener(() => ShopHandler.ShopHand.GetCarinfo(CarID));
            if (isOwned)
            {
                prices.SetActive(false);
                Owned.SetActive(true);
                return;
            }
            BuySilv.onClick.AddListener(() => ShopHandler.ShopHand.BuyPiece(SilPrice,CarID,0));
            BuyGold.onClick.AddListener(() => ShopHandler.ShopHand.BuyPiece(GolPrice*-1, CarID, 0));
        }
    }
}
