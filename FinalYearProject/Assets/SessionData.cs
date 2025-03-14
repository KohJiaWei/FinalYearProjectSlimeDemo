using System;

[Serializable]
public class SessionData
{
    public string timestamp;

    // Basic stats
    public int slimesKilled;
    public float slimesPerMinute;
    public int lightningBoltsUsed;
    public float survivalTime;
    public float totalTimeTaken;
}
