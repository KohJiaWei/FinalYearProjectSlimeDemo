using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameTracker : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_Text SlimesKilled;
    public static GameTracker instance;
    public int slimesKilledCount = 0;
    private void Awake()
    {
        SlimesKilled.text = "Total slimes Killed: " + slimesKilledCount.ToString();
        instance = this;
    }

    public void incrementSlimeKilled()
    {
        slimesKilledCount++;
        SlimesKilled.text = "Total slimes Killed: " + slimesKilledCount.ToString();
        
    }
   
}
