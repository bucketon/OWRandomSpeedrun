using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using System.Reflection;
using System.Linq;
using OWML.Common.Menus;
using System;
using HarmonyLib;

namespace OuterWildsRandomSpeedrun
{
    public class OuterWildsRandomSpeedrun : ModBehaviour
    {
        private const string RESUME_BUTTON_NAME = "Button-ResumeGame";

        private SpawnPoint _spawnPoint;
        private SpawnPoint _goalPoint;
        private string _spawnPointName;
        private string _goalPointName;
        private IModButton _speedrunButton;
        private IModButton _resetRunButton;
        private IModButton _pingusButton;
        private DateTime _startTime;
        private DateTime _endTime = DateTime.MinValue;
        private ScreenPrompt _timerPrompt;
        
        private bool _modEnabled = false;
        private Mesh _marshmallowMesh;
        private Material _marshmallowMaterial;
        private CanvasMarker _canvasMarker;

        private SpawnPointSelectorManager _manager;

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

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
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
            GlobalMessenger.AddListener("WakeUp", new Callback(this.OnWakeUp));

            ModHelper.Menus.MainMenu.OnInit += () =>
            {
                _speedrunButton = ModHelper.Menus.MainMenu.ResumeExpeditionButton.Duplicate(Constants.SPEEDRUN_BUTTON_TEXT);
                _speedrunButton.OnClick += SpeedRunButton_OnClick;

                _pingusButton = ModHelper.Menus.MainMenu.ResumeExpeditionButton.Duplicate("PINGUS MODE");
                _pingusButton.OnClick += PingusModeButton_OnClick;
            };

            ModHelper.Menus.PauseMenu.OnInit += () =>
            {
                _resetRunButton = ModHelper.Menus.PauseMenu.QuitButton.Duplicate(Constants.RESET_RUN_BUTTON_TEXT);
                _resetRunButton.OnClick += ResetRunButton_OnClick;
            };
        }

        private void Update()
        {
            if (!_isGameStarted || !_modEnabled)
            {
                return;
            }

            if (_justEnteredGame)
            {
                _justEnteredGame = false;
                _startTime = DateTime.Now;
                ResetSpawnNames();
            }

            if (_justStartedTimeLoop)
            {
                _justStartedTimeLoop = false;
                var spawner = GetSpawner();
                var spawnPoints = GetSpawnPoints(spawner);
                HandleBasicWarp(spawner, spawnPoints);
                InitMapMarker();
                SpawnGoal(_goalPoint.transform);
            }

            var elapsed = _endTime == DateTime.MinValue ? DateTime.Now - _startTime : _endTime - _startTime;
            
            var elapsedStr = string.Format("{0:D2}:{1:D2}.{2:D3}", elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds);
            _timerPrompt.SetText($"<color=#{ColorUtility.ToHtmlStringRGB(Constants.OW_ORANGE_COLOR)}>{elapsedStr}</color>");
        }

        private void OnWakeUp()
        {
            Locator.GetPlayerBody().SetVelocity(_spawnPoint.GetPointVelocity());
        }

        private void OnStartOfTimeLoop(int loopCount)
        {
            if (_modEnabled)
            {
                _justStartedTimeLoop = true;
                CreateTimer();
            }
        }

        private void ResetSpawnNames()
        {
            _spawnPointName = null;
            _goalPointName = null;
        }

        private void CreateTimer()
        {
            var screenPromptListObj = GameObject.Find("ScreenPromptListBottomLeft");
            var screenPromptList = screenPromptListObj.GetComponent<ScreenPromptList>();

            _timerPrompt = new ScreenPrompt("");
            var font = GetFontByName(Constants.OW_MENU_FONT_NAME);
            var screenPromptElementObj = ScreenPromptElement.CreateNewScreenPrompt(_timerPrompt, 20, font, screenPromptListObj.transform, TextAnchor.LowerLeft);
            var screenPromptElement = screenPromptElementObj.GetComponent<ScreenPromptElement>();
            screenPromptList.AddScreenPrompt(screenPromptElement);
        }

        private void HandleBasicWarp(PlayerSpawner spawner, SpawnPoint[] spawnPoints)
        {
            if (_spawnPointName == null || _goalPointName == null)
            {
                _spawnPointName = GetRandomSpawnPointName();
                _goalPointName = GetRandomSpawnPointName();
            }

            _spawnPoint = GetSpawnPointByName(spawnPoints, _spawnPointName);
            _goalPoint = GetSpawnPointByName(spawnPoints, _goalPointName);
            ModHelper.Console.WriteLine($"Warp to {_spawnPoint.ToString()}!", MessageType.Success);
            spawner.DebugWarp(_spawnPoint);
            var player = GameObject.FindGameObjectWithTag("Player");
            var playerController = player.GetComponent<PlayerSpacesuit>();
            playerController.SuitUp();
            var oxygenController = player.GetComponent<PlayerResources>();
            oxygenController.UpdateOxygen();
            var ship = GameObject.FindGameObjectWithTag("Ship");
            ship.SetActive(false);
            var villageMusicController = FindObjectOfType<VillageMusicVolume>();
            ModHelper.Console.WriteLine($"villageMusicController {(villageMusicController == null ? "is" : "isn\'t")} null", MessageType.Success);
            villageMusicController.Deactivate();
        }

        private Font GetFontByName(string name)
        {
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
            if (behaviour is TitleScreenManager && ev == Events.AfterStart)
            {
                _isGameStarted = false;
                _modEnabled = false;
            }
        }

        private void SpeedRunButton_OnClick()
        {
            if (FindObjectOfType<TitleScreenManager>()._profileManager.currentProfileGameSave.loopCount <= 1)
            {
                ModHelper.Menus.PopupManager.CreateMessagePopup("Finish the tutorial first!");
                return;
            }

            _modEnabled = true;
            _justEnteredGame = true;
            GameObject.Find(RESUME_BUTTON_NAME).GetComponent<SubmitActionLoadScene>().Submit();
        }
        
        private void ResetRunButton_OnClick()
        {
            _justEnteredGame = true;
            Locator.GetDeathManager().KillPlayer(DeathType.Meditation);
            ModHelper.Menus.PauseMenu.Close();
        }

        private void PingusModeButton_OnClick()
        {
            _manager = SpawnPointSelectorManager.Instance;
            _manager.ModHelper = ModHelper;
            _manager.DisplayMenu();
        }

        protected SpawnPoint[] GetSpawnPoints(PlayerSpawner spawner)
        {
            var spawnPointsField = typeof(PlayerSpawner)
                .GetField("_spawnList", BindingFlags.NonPublic | BindingFlags.Instance);
            var spawnPoints = spawnPointsField?.GetValue(spawner) as SpawnPoint[];
            spawnPoints = spawnPoints.OrderBy(x => x.name).ToArray();

            ModHelper.Console.WriteLine($"Registered {spawnPoints.Length} spawn points", MessageType.Info);

            // var stringbuilder = "";
            // foreach (var point in spawnPoints)
            // {
            //     stringbuilder += point.name;
            //     stringbuilder += ", ";
            // }
            //ModHelper.Console.WriteLine(stringbuilder, MessageType.Info);

            return spawnPoints;
        }

        protected void InitMapMarker()
        {
            var markerManager = Locator.GetMarkerManager();
            _canvasMarker = markerManager.InstantiateNewMarker();
            markerManager.RegisterMarker(_canvasMarker, _goalPoint.transform, "GOAL");
            _canvasMarker._mainTextField.color = Constants.OW_ORANGE_COLOR;
            _canvasMarker._marker.material.color = Constants.OW_ORANGE_COLOR;
            _canvasMarker._offScreenIndicator._textField.color = Constants.OW_ORANGE_COLOR;
            _canvasMarker._offScreenIndicator._arrow.GetComponentInChildren<MeshRenderer>().material.color = Constants.OW_ORANGE_COLOR;
            _canvasMarker.SetVisibility(true);

            var mapMarkerManager = Locator.GetMapController().GetMarkerManager();
            var mapMarker = mapMarkerManager.InstantiateNewMarker(true);
            mapMarkerManager.RegisterMarker(mapMarker, _goalPoint.transform, UITextType.None);
            mapMarker.SetLabel("GOAL");
            var materialInstance = Instantiate(mapMarker._textField.material);
            materialInstance.color = OW_ORANGE_COLOR;
            mapMarker._textField.material = materialInstance;
            mapMarker.SetColor(OW_ORANGE_COLOR);
            mapMarker.SetVisibility(true);
        }

        protected PlayerSpawner GetSpawner()
        {
            ModHelper.Console.WriteLine($"initialize spawner.", MessageType.Info);
            return GameObject.FindGameObjectWithTag("Player").GetRequiredComponent<PlayerSpawner>();
        }

        protected string GetRandomSpawnPointName()
        {
            var randIndex = _random.Next(SpawnPointNames.SPAWN_POINT_NAMES.Count);

            ModHelper.Console.WriteLine($"Spawn point {SpawnPointNames.SPAWN_POINT_NAMES[randIndex]} set, from index {randIndex}", MessageType.Info);
            return SpawnPointNames.SPAWN_POINT_NAMES[randIndex];
        }

        protected SpawnPoint GetSpawnPointByName(SpawnPoint[] spawnPoints, string name)
        {
            return spawnPoints.Where(point => { return point.name.Equals(name); }).First();
        }

        protected void SpawnGoal(Transform parent)
        {
            var go = new GameObject("GoalPoint");
            var collider = go.AddComponent<SphereCollider>();
            collider.isTrigger = true;

            var mesh = new GameObject("CollectibleMarshmellow_Mesh");
            var marshmallowGameObject = GameObject.Find("Player_Body/RoastingSystem/Stick_Root/Stick_Pivot/Stick_Tip/Mallow_Root/Props_HEA_Marshmallow");
            if (_marshmallowMesh == null)
                _marshmallowMesh = marshmallowGameObject.GetComponent<MeshFilter>().mesh;
            if (_marshmallowMaterial == null)
                _marshmallowMaterial = marshmallowGameObject.GetComponent<MeshRenderer>().material;

            mesh.AddComponent<MeshFilter>().mesh = _marshmallowMesh;
            mesh.AddComponent<MeshRenderer>().material = _marshmallowMaterial;
            mesh.transform.parent = go.transform;
            mesh.transform.localScale = Vector3.one * 10f;
            mesh.transform.localEulerAngles = new Vector3(0f, 0f, 0f);

            Marshmallow marshmallow = go.AddComponent<Marshmallow>();
            marshmallow.OnCollected += () =>
            {
                ModHelper.Console.WriteLine($"VICTORY!!!!", MessageType.Info);
                _endTime = DateTime.Now;
                marshmallow.gameObject.SetActive(false);
                _canvasMarker.gameObject.SetActive(false);
            };

            go.transform.parent = parent;
            go.transform.localPosition = new Vector3(0, 0, 0);
        }
    }
}