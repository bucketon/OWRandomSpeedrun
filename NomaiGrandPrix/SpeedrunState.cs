using System;

namespace NomaiGrandPrix
{
  public class SpeedrunState
  {
    public SpawnPointConfig? SpawnPoint { get; set; }
    public SpawnPointConfig? GoalPoint { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; } = DateTime.MinValue;
    public bool ModEnabled { get; set; } = false;

    /// <summary>
    /// Set to true when we have just entered the game (from the title screen) and have pending operations to complete, false otherwise.
    /// </summary>
    public bool JustEnteredGame { get; set; } = false;

    /// <summary>
    /// Set to true when we have just begun a time loop and have pending operations to complete, false otherwise.
    /// </summary>
    public bool JustStartedTimeLoop { get; set; }

    /// <summary>
    /// Set to true when we are in the game (including death/meditation), and false if we are elsewhere (the title screen).
    /// </summary>
    public bool IsGameStarted { get; set; }

    public SpeedrunState()
    {
      // Default constructor required when using field initializers
    }
  }
}
