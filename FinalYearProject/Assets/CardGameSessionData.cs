using System;
using System.Collections.Generic;

// This class holds the metrics we want to save for each game session.
[Serializable]
public class CardGameSessionData
{
    public string timestamp;                  // For unique naming or record-keeping
    public float totalTime;                   // Time taken to complete the game
    public int incorrectMatches;              // Number of times the player guessed incorrectly
    public int totalAttempts;                 // Total attempts to match cards
    public List<float> attemptTimes;          // Time spent on each individual attempt

    public override string ToString()
    {
        return $"[CardGameSessionData] Time={totalTime}s, Incorrect={incorrectMatches}, Attempts={totalAttempts}, AttemptTimes={string.Join(", ", attemptTimes)}";
    }
}
