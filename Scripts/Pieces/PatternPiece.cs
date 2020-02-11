using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Cars
{
    public class PatternPiece : MonoBehaviour
    {
        public GameObject Blocked;
        public GameObject Color;
        public Button buy;
        public TMPro.TextMeshProUGUI Price;
        public Image PriceIcon;
        public Sprite gold, silver;
        public bool IsBlocked;
        int carId;
        int priceReg, pieceid;

        public void ColorSelector()
        {
            PlayerP.PlayerPresets.ColorSelector();
        }

        public void setBlock(int price, ShopHandler shopHandler, int CarId, int PieceID)
        {
            carId = CarId;
            Blocked.SetActive(true);
            Color.SetActive(false);
            priceReg = price;
            if (price < 0)
            {
                PriceIcon.sprite = gold;
                price *= -1;
            }
            Price.text = price.ToString();
            buy.onClick.AddListener(() => shopHandler.BuyPiece(priceReg, carId, PieceID));
        }
    }
}
