using System;
using System.IO;
using UnityEngine;
using TMPro;

public class GameTracker : MonoBehaviour
{
    public static GameTracker instance;

    // Existing fields
    public TMP_Text SlimesKilled;
    public TMP_Text gameTimer;
    private int slimesKilledCount = 0;
    private float elapsedTime = 0f;
    private bool isGameRunning = true;

    // NEW FIELDS for additional metrics
    private int totalShotsFired = 0;     // Used to calculate accuracy
    private int successfulHits = 0;      // Number of times the shot landed
    private int lightningBoltsUsed = 0;  // EEG-triggered ability
    private int powerUpsActivated = 0;   // Concentration peaks

    // Change this to wherever you want to save the file
    private string saveDirectory = @"C:\Unity_Projects\GameSessions";

    private void Awake()
    {
        // Basic Singleton pattern (optional)
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize UI
        SlimesKilled.text = "Total Slimes Killed: " + slimesKilledCount;

        // Make sure our directory exists
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
    }

    private void Update()
    {
        if (isGameRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateGameTimerDisplay();
        }
    }

    /// <summary>
    /// Called when a slime is killed.
    /// </summary>
    public void IncrementSlimeKilled()
    {
        slimesKilledCount++;
        SlimesKilled.text = "Total Slimes Killed: " + slimesKilledCount;
    }

    /// <summary>
    /// Called when the player fires a shot (hit or miss).
    /// </summary>
    public void RecordShot(bool wasHit)
    {
        totalShotsFired++;
        if (wasHit)
        {
            successfulHits++;
        }
    }

    /// <summary>
    /// Called when a lightning bolt ability is used (EEG-triggered).
    /// </summary>
    public void IncrementLightningBoltsUsed()
    {
        lightningBoltsUsed++;
    }

    /// <summary>
    /// Called when a power-up is activated (concentration peak).
    /// </summary>
    public void IncrementPowerUpsActivated()
    {
        powerUpsActivated++;
    }

    /// <summary>
    /// Stop updating the game timer.
    /// </summary>
    public void StopTimer()
    {
        isGameRunning = false;
    }

    private void UpdateGameTimerDisplay()
    {
        if (gameTimer == null) return; // safety check

        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        gameTimer.text = $"Time: {minutes:D2}:{seconds:D2}";
    }

    /// <summary>
    /// Saves the current metrics to JSON for later analysis.
    /// </summary>
    public void SaveSession()
    {
        // Compute derived metrics
        float slimesPerMinute = (elapsedTime > 0)
            ? slimesKilledCount / (elapsedTime / 60f)
            : 0f;

        float accuracy = (totalShotsFired > 0)
            ? (successfulHits / (float)totalShotsFired) * 100f
            : 0f;

        float survivalTime = elapsedTime;  // or a separate formula if you prefer

        // Populate our session data class
        SessionData sessionData = new SessionData
        {
            timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"),
            slimesKilled = slimesKilledCount,
            slimesPerMinute = slimesPerMinute,
            lightningBoltsUsed = lightningBoltsUsed,
            survivalTime = survivalTime,
            totalTimeTaken = elapsedTime
        };

        // Convert to JSON
        string json = JsonUtility.ToJson(sessionData, true);

        // Save to file
        string fileName = "Session_" + sessionData.timestamp + ".json";
        string fullPath = Path.Combine(saveDirectory, fileName);
        File.WriteAllText(fullPath, json);

        Debug.Log($"Session saved successfully: {fullPath}");
    }
}
