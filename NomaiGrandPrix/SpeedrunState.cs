using System;

namespace NomaiGrandPrix
{
    public class SpeedrunState
    {
        private static SpeedrunState Instance = new SpeedrunState();

        public static SpawnPointConfig? SpawnPoint
        {
            get => Instance._spawnPoint;
            set => Instance._spawnPoint = value;
        }
        private SpawnPointConfig? _spawnPoint;

        public static SpawnPointConfig? GoalPoint
        {
            get => Instance._goalPoint;
            set => Instance._goalPoint = value;
        }
        private SpawnPointConfig? _goalPoint;

        public static DateTime StartTime
        {
            get => Instance._startTime;
            set => Instance._startTime = value;
        }
        private DateTime _startTime;

        public static DateTime EndTime
        {
            get => Instance._endTime;
            set => Instance._endTime = value;
        }
        private DateTime _endTime = DateTime.MinValue;

        public static bool ModEnabled
        {
            get => Instance._modEnabled;
            set => Instance._modEnabled = value;
        }
        private bool _modEnabled = false;

        /// <summary>
        /// Set to true when we have just entered the game (from the title screen) and have pending operations to complete, false otherwise.
        /// </summary>
        public static bool JustEnteredGame
        {
            get => Instance._justEnteredGame;
            set => Instance._justEnteredGame = value;
        }
        private bool _justEnteredGame = false;

        /// <summary>
        /// Set to true when we have just begun a time loop and have pending operations to complete, false otherwise.
        /// </summary>
        public static bool JustStartedTimeLoop
        {
            get => Instance._justStartedTimeLoop;
            set => Instance._justStartedTimeLoop = value;
        }
        private bool _justStartedTimeLoop;

        /// <summary>
        /// Set to true when we are in the game (including death/meditation), and false if we are elsewhere (the title screen).
        /// </summary>
        public static bool IsGameStarted
        {
            get => Instance._isGameStarted;
            set => Instance._isGameStarted = value;
        }
        private bool _isGameStarted;

        public SpeedrunState()
        {
            // Default constructor required when using field initializers
        }
    }
}
