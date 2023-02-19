using System;

namespace OuterWildsRandomSpeedrun
{
  public struct SpeedrunState
  {
    public static SpeedrunState INSTANCE = new SpeedrunState();

    public string SpawnPointId;
    public string GoalPointId;
    public string GoalPointName;
    public DateTime StartTime;
    public DateTime EndTime = DateTime.MinValue;
    public bool ModEnabled = false;

    /// <summary>
    /// Set to true when we have just entered the game (from the title screen) and have pending operations to complete, false otherwise.
    /// </summary>
    public bool JustEnteredGame = false;

    /// <summary>
    /// Set to true when we have just begun a time loop and have pending operations to complete, false otherwise.
    /// </summary>
    public bool JustStartedTimeLoop;

    /// <summary>
    /// Set to true when we are in the game (including death/meditation), and false if we are elsewhere (the title screen).
    /// </summary>
    public bool IsGameStarted;

    public SpeedrunState()
    {
      // Default constructor required when using field initializers
    }
  }
}
