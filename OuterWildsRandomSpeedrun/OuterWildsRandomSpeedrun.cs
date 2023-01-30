using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using OWML.Common.Menus;
using System;

namespace OuterWildsRandomSpeedrun
{
    public class OuterWildsRandomSpeedrun : ModBehaviour
    {
        private const string SPEEDRUN_BUTTON_TEXT = "NOMAI GRAND PRIX";
        private const string RESUME_BUTTON_NAME = "Button-ResumeGame";
        private const string OW_ORANGE_COLOR = "#F67E34";
        private const string OW_MENU_FONT_NAME = "Adobe - SerifGothicStd-ExtraBold";
        protected SpawnPoint[] _spawnPoints;
        protected int _spawnPointIndex = 0;

        protected PlayerSpawner _spawner;

        protected SpawnPoint _spawnPoint;

        protected IModButton _speedrunButton; 

        private long _startTimeMillis;
        private ScreenPrompt _timerPrompt;
        private bool _modEnabled = false;
        private bool _shouldStartTimer = false;
        private bool _shouldWarp;
        private bool _isGameStarted;

        private void Awake()
        {
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
        }

        private void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"My mod {nameof(OuterWildsRandomSpeedrun)} is loaded!", MessageType.Success);

            // Example of accessing game code.
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene != OWScene.SolarSystem) return;
                ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);
            };

            GlobalMessenger<int>.AddListener("StartOfTimeLoop", new Callback<int>(this.OnStartOfTimeLoop));

            ModHelper.HarmonyHelper.EmptyMethod<DebugInputManager>("Awake");
            ModHelper.Events.Subscribe<DebugInputManager>(Events.AfterStart);
            ModHelper.Events.Subscribe<DebugInputManager>(Events.AfterAwake);
            ModHelper.Events.Subscribe<PlayerAudioController>(Events.AfterStart);
            ModHelper.Events.Subscribe<RingWorldController>(Events.AfterStart);
            ModHelper.Events.Subscribe<TitleScreenManager>(Events.AfterStart);
            ModHelper.Events.Event += OnEvent;
            
            ModHelper.Menus.MainMenu.OnInit += () => {
                _speedrunButton = ModHelper.Menus.MainMenu.ResumeExpeditionButton.Duplicate(SPEEDRUN_BUTTON_TEXT);
                _speedrunButton.OnClick += SpeedRunButton_OnClick;
            };
        }

        private void Update()
        {
            if (!_isGameStarted || !_modEnabled) {
                return;
            }

            if (_shouldStartTimer) {
                _shouldStartTimer = false;
                _startTimeMillis = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            }

            if (_shouldWarp) {
                HandleBasicWarp();
            }
            
            var elapsed = TimeSpan.FromMilliseconds(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - _startTimeMillis);
            var elapsedStr = string.Format("{0:D2}:{1:D2}.{2:D3}", elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds);
            _timerPrompt.SetText($"<color={OW_ORANGE_COLOR}>{elapsedStr}</color>");
        }

        private void OnStartOfTimeLoop(int loopCount) {
            if (_modEnabled) {
                _shouldWarp = true;
                CreateTimer();
            }
        }

        private void CreateTimer()
        {
            var screenPromptListObj = GameObject.Find("ScreenPromptListBottomLeft");
            var screenPromptList = screenPromptListObj.GetComponent<ScreenPromptList>();
            
            _timerPrompt = new ScreenPrompt("");
            
            var font = GetFontByName(OW_MENU_FONT_NAME);
            var screenPromptElementObj = ScreenPromptElement.CreateNewScreenPrompt(_timerPrompt, 20, font, screenPromptListObj.transform, TextAnchor.LowerLeft);
            var screenPromptElement = screenPromptElementObj.GetComponent<ScreenPromptElement>();
            screenPromptList.AddScreenPrompt(screenPromptElement);
        }

        private void HandleBasicWarp()
        {
            _shouldWarp = false;
            InitSpawner();
            SetSpawnPoint();
            ModHelper.Console.WriteLine($"Warp to {_spawnPoint.ToString()}!", MessageType.Success);
            _spawner.DebugWarp(_spawnPoint);
            var player = GameObject.FindGameObjectWithTag("Player");
            var playerController = player.GetComponent<PlayerSpacesuit>();
            playerController.SuitUp();
            var oxygenController = player.GetComponent<PlayerResources>();
            oxygenController.UpdateOxygen();
            var ship = GameObject.FindGameObjectWithTag("Ship");
            ship.SetActive(false);
        }

        private Font GetFontByName(string name) {
            var fonts = Resources.FindObjectsOfTypeAll(typeof(Font)) as Font[];
            return fonts.First(font => font.name == name);
        }

        private void OnEvent(MonoBehaviour behaviour, Events ev)
        {
            if (behaviour is DebugInputManager && ev == Events.AfterStart)
            {
                ModHelper.Console.WriteLine("isStarted!", MessageType.Success);
                _isGameStarted = true;
            }
            if (behaviour is TitleScreenManager && ev == Events.AfterStart) {
                _isGameStarted = false;
                _modEnabled = false;
            }
        }

        private void SpeedRunButton_OnClick() {
            _modEnabled = true;
            _shouldStartTimer = true;
            GameObject.Find(RESUME_BUTTON_NAME).GetComponent<SubmitActionLoadScene>().Submit();
        }

        protected void SetSpawnPoint()
        {
            InitSpawnPoints();
            _spawnPoint = GetRandomSpawnPoint();
            InitMapMarker();
        }

        protected void InitSpawnPoints() {
            //if (_spawnPoints != null) {
            //    return;
            //}

            var spawnPointsField = typeof(PlayerSpawner)
                .GetField("_spawnList", BindingFlags.NonPublic | BindingFlags.Instance);
            var spawnPoints = spawnPointsField?.GetValue(_spawner) as SpawnPoint[];
            _spawnPoints = spawnPoints.OrderBy(x => x.name).ToArray();

            ModHelper.Console.WriteLine($"Registered {spawnPoints.Length} spawn points", MessageType.Info);

            var stringbuilder = "";
            foreach (var point in _spawnPoints)
            {
                stringbuilder += point.name;
                stringbuilder += ", ";
            }
            ModHelper.Console.WriteLine(stringbuilder, MessageType.Info);
        }

        protected void InitMapMarker() {
            var entries = FindObjectsOfType<ShipLogEntryLocation>();
            var entryLocation = entries.Where(entry => { return entry._entryID.Equals("BH_OBSERVATORY"); }).First();
            var markerManager = Locator.GetMarkerManager();
            var canvasMarker = markerManager.InstantiateNewMarker();
            markerManager.RegisterMarker(canvasMarker, entryLocation.transform, "GOAL");
            canvasMarker.SetVisibility(true);
        }

        protected void InitSpawner() {
            ModHelper.Console.WriteLine($"initialize spawner.", MessageType.Info);
            _spawner = GameObject.FindGameObjectWithTag("Player").GetRequiredComponent<PlayerSpawner>();
        }

        protected SpawnPoint GetRandomSpawnPoint()
        {
            /*List<SpawnLocation> validSpawnPoints = new List<SpawnLocation> { 
                SpawnLocation.HourglassTwin_1,
                SpawnLocation.HourglassTwin_2,
                //SpawnLocation.GasGiant,
                SpawnLocation.BrittleHollow,
                SpawnLocation.DarkBramble,
                SpawnLocation.GasGiantMoon,
                SpawnLocation.QuantumMoon,
                SpawnLocation.LunarLookout,
                SpawnLocation.SignalDish,
                SpawnLocation.SunStation,
                SpawnLocation.TimberHearth_Alt,
            };*/
            var random = new System.Random((int)Time.time);
            var randIndex = random.Next(_spawnPoints.Length);

            ModHelper.Console.WriteLine($"Spawn point {_spawnPoints[randIndex]} set, from index {randIndex}", MessageType.Info);
            return _spawnPoints[randIndex];
        }
    }
}