using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using System.Reflection;
using System.Linq;
using OWML.Common.Menus;
using System;

namespace OuterWildsRandomSpeedrun
{
    public class OuterWildsRandomSpeedrun : ModBehaviour
    {
        private const string SPEEDRUN_BUTTON_TEXT = "NOMAI GRAND PRIX";
        private const string RESUME_BUTTON_NAME = "Button-ResumeGame";
        private const string OW_MENU_FONT_NAME = "Adobe - SerifGothicStd-ExtraBold";
        private static Color OW_ORANGE_COLOR = new Color(0.968f, 0.498f, 0.207f);

        private SpawnPoint _goalPoint;
        private string _spawnPointName;
        private string _goalPointName;
        private IModButton _speedrunButton; 
        private DateTime _startTime;
        private ScreenPrompt _timerPrompt;
        private bool _modEnabled = false;

        /// <summary>
        /// Set to true when we have just entered the game (from the title screen) and have pending operations to complete, false otherwise.
        /// </summary>
        private bool _justEnteredGame = false;

        /// <summary>
        /// Set to true when we have just began a time loop and have pending operations to complete, false otherwise.
        /// </summary>
        private bool _justStartedTimeLoop;

        /// <summary>
        /// Set to true when we are in the game (including death/meditation), and false if we are elsewhere (the title screen).
        /// </summary>
        private bool _isGameStarted;
        
        private System.Random _random;

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

            _random = new System.Random((int)DateTime.Now.Ticks);
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

            if (_justEnteredGame) {
                _justEnteredGame = false;
                _startTime = DateTime.Now;
                ResetSpawnNames();
            }

            if (_justStartedTimeLoop) {
                _justStartedTimeLoop = false;
                var spawner = GetSpawner();
                var spawnPoints = GetSpawnPoints(spawner);
                HandleBasicWarp(spawner, spawnPoints);
                InitMapMarker();
            }

            var elapsed = DateTime.Now - _startTime;
            var elapsedStr = string.Format("{0:D2}:{1:D2}.{2:D3}", elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds);
            _timerPrompt.SetText($"<color=#{ColorUtility.ToHtmlStringRGB(OW_ORANGE_COLOR)}>{elapsedStr}</color>");
        }

        private void OnStartOfTimeLoop(int loopCount) {
            if (_modEnabled) {
                _justStartedTimeLoop = true;
                CreateTimer();
            }
        }

        private void ResetSpawnNames() {
            _spawnPointName = null;
            _goalPointName = null;
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

        private void HandleBasicWarp(PlayerSpawner spawner, SpawnPoint[] spawnPoints)
        {
            if (_spawnPointName == null || _goalPointName == null) {
                _spawnPointName = GetRandomSpawnPointName(spawnPoints);
                _goalPointName = GetRandomSpawnPointName(spawnPoints);
            }

            var spawnPoint = GetSpawnPointByName(spawnPoints, _spawnPointName);
            _goalPoint = GetSpawnPointByName(spawnPoints, _goalPointName);
            ModHelper.Console.WriteLine($"Warp to {spawnPoint.ToString()}!", MessageType.Success);
            spawner.DebugWarp(spawnPoint);
            var player = GameObject.FindGameObjectWithTag("Player");
            var playerController = player.GetComponent<PlayerSpacesuit>();
            playerController.SuitUp();
            var oxygenController = player.GetComponent<PlayerResources>();
            oxygenController.UpdateOxygen();
            var ship = GameObject.FindGameObjectWithTag("Ship");
            ship.SetActive(false);
            var villageMusicController = FindObjectOfType<VillageMusicVolume>();
            villageMusicController.OnEffectVolumeExit(spawner.gameObject);
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
            _justEnteredGame = true;
            GameObject.Find(RESUME_BUTTON_NAME).GetComponent<SubmitActionLoadScene>().Submit();
        }

        protected SpawnPoint[] GetSpawnPoints(PlayerSpawner spawner) {
            var spawnPointsField = typeof(PlayerSpawner)
                .GetField("_spawnList", BindingFlags.NonPublic | BindingFlags.Instance);
            var spawnPoints = spawnPointsField?.GetValue(spawner) as SpawnPoint[];
            spawnPoints = spawnPoints.OrderBy(x => x.name).ToArray();

            ModHelper.Console.WriteLine($"Registered {spawnPoints.Length} spawn points", MessageType.Info);

            var stringbuilder = "";
            foreach (var point in spawnPoints)
            {
                stringbuilder += point.name;
                stringbuilder += ", ";
            }
            ModHelper.Console.WriteLine(stringbuilder, MessageType.Info);

            return spawnPoints;
        }

        protected void InitMapMarker() {
            var markerManager = Locator.GetMarkerManager();
            var canvasMarker = markerManager.InstantiateNewMarker();
            markerManager.RegisterMarker(canvasMarker, _goalPoint.transform, "GOAL");
            canvasMarker._mainTextField.color = OW_ORANGE_COLOR;
            canvasMarker._marker.material.color = OW_ORANGE_COLOR;
            canvasMarker._offScreenIndicator._textField.color = OW_ORANGE_COLOR;
            canvasMarker._offScreenIndicator._arrow.GetComponentInChildren<MeshRenderer>().material.color = OW_ORANGE_COLOR;
            canvasMarker.SetVisibility(true);
        }

        protected PlayerSpawner GetSpawner() {
            ModHelper.Console.WriteLine($"initialize spawner.", MessageType.Info);
            return GameObject.FindGameObjectWithTag("Player").GetRequiredComponent<PlayerSpawner>();
        }

        protected string GetRandomSpawnPointName(SpawnPoint[] spawnPoints)
        {   
            var randIndex = _random.Next(spawnPoints.Length);

            ModHelper.Console.WriteLine($"Spawn point {spawnPoints[randIndex]} set, from index {randIndex}", MessageType.Info);
            return spawnPoints[randIndex].name;
        }
        protected SpawnPoint GetSpawnPointByName(SpawnPoint[] spawnPoints, string name)
        {
            return spawnPoints.Where(point => { return point.name.Equals(name); }).First();
        }
    }
}