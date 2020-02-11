using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace Cars {
    public class DMMode : MonoBehaviour
    {
        public static DMMode DM;
        public TextMeshProUGUI[] Nicknames;
        public TextMeshProUGUI[] Kills;
        public TextMeshProUGUI kills, Label;

        private void Awake()
        {
            if (DM == null)
                DM = this;
            Label.text = MainMenu.menu.lang.DM[0];
        }

    }
}
