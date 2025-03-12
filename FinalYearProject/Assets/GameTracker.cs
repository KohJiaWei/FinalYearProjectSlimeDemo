using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameTracker : MonoBehaviour
{
    public TMP_Text SlimesKilled;
    public TMP_Text gameTimer;
    public static GameTracker instance;

    private int slimesKilledCount = 0;
    private float elapsedTime = 0f;
    private bool isGameRunning = true; // Flag to stop timer if needed

    private void Awake()
    {
        instance = this;
        SlimesKilled.text = "Total slimes Killed: " + slimesKilledCount.ToString();
    }

    private void Update()
    {
        if (isGameRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateGameTimerDisplay();
        }
    }

    public void incrementSlimeKilled()
    {
        slimesKilledCount++;
        SlimesKilled.text = "Total slimes Killed: " + slimesKilledCount.ToString();
    }

    private void UpdateGameTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        gameTimer.text = $"Time: {minutes:D2}:{seconds:D2}";
    }

    public void StopTimer()
    {
        isGameRunning = false;
    }
}
