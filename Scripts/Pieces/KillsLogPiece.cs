using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KillsLogPiece : MonoBehaviour
{
    public string Killer, Victim;
    public TextMeshProUGUI KName, VName;
    float timer = 10f;
    public void SetUpUI()
    {
        KName.text = Killer;
        VName.text = Victim;
    }
    private void Update()
    {
        timer = (timer > 0)? timer-Time.deltaTime : DeletePiece();
    }
    float DeletePiece()
    {
        Destroy(gameObject);
        return 0;
    }
}
