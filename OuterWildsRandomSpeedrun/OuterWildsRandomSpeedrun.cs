using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using System.IO;
using System.Reflection;
using System.Linq;
using OWML.Common.Menus;
using System;
using HarmonyLib;
using UnityEngine.UI;
using System.Collections.Generic;

namespace OuterWildsRandomSpeedrun
{
    public class OuterWildsRandomSpeedrun : ModBehaviour
    {
        private const string RESUME_BUTTON_NAME = "Button-ResumeGame";

        private SpawnPoint _spawnPoint;
        private SpawnPoint _goalPoint;
        private IModButton _speedrunButton;
        private IModButton _resetRunButton;
        private ScreenPrompt _timerPrompt;
        private Mesh _marshmallowMesh;
        private Material _marshmallowMaterial;
        private CanvasMarker _canvasMarker;

        private SpawnPointSelectorManager _manager;

        private System.Random _random;

        private SpawnPointPool _spawnPointPool;

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
            ModHelper.Console.WriteLine($"Mod {nameof(OuterWildsRandomSpeedrun)} is loaded!", MessageType.Success);

            // Initialize spawn points from TSV
            var parentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var pathToTsv = Path.Combine(parentDir, "SpawnPoints.tsv");
            _spawnPointPool = SpawnPointPool.FromTsv(pathToTsv);
            ModHelper.Console.WriteLine($"Loaded {_spawnPointPool.SpawnPointConfigs.Count} spawn points", MessageType.Debug);

            _random = new System.Random((int)DateTime.Now.Ticks);

            GlobalMessenger<int>.AddListener("StartOfTimeLoop", new Callback<int>(this.OnStartOfTimeLoop));
            GlobalMessenger.AddListener("WakeUp", new Callback(this.OnWakeUp));

            // The mod breaks without this for reasons unknown
            ModHelper.HarmonyHelper.EmptyMethod<DebugInputManager>("Awake");


            ModHelper.Menus.MainMenu.OnInit += () =>
            {
                _speedrunButton = ModHelper.Menus.MainMenu.ResumeExpeditionButton.Duplicate(Constants.SPEEDRUN_BUTTON_TEXT);
                _speedrunButton.OnClick += SpeedRunButton_OnClick;
            };

            ModHelper.Menus.PauseMenu.OnInit += () =>
            {
                _resetRunButton = ModHelper.Menus.PauseMenu.QuitButton.Duplicate(Constants.RESET_RUN_BUTTON_TEXT);
                _resetRunButton.OnClick += ResetRunButton_OnClick;
            };
        }

        private void Update()
        {
            if (!SpeedrunState.INSTANCE.IsGameStarted || !SpeedrunState.INSTANCE.ModEnabled)
            {
                return;
            }

            if (SpeedrunState.INSTANCE.JustEnteredGame)
            {
                SpeedrunState.INSTANCE.JustEnteredGame = false;
                SpeedrunState.INSTANCE.StartTime = DateTime.Now;
            }

            if (SpeedrunState.INSTANCE.JustStartedTimeLoop)
            {
                SpeedrunState.INSTANCE.JustStartedTimeLoop = false;
                var spawner = GetSpawner();
                var spawnPoints = GetSpawnPoints(spawner);
                HandleBasicWarp(spawner, spawnPoints);
                InitMapMarker();
                SpawnGoal(_goalPoint.transform);
            }

            var elapsed = SpeedrunState.INSTANCE.EndTime == DateTime.MinValue ? DateTime.Now - SpeedrunState.INSTANCE.StartTime : SpeedrunState.INSTANCE.EndTime - SpeedrunState.INSTANCE.StartTime;

            var elapsedStr = string.Format("{0:D2}:{1:D2}.{2:D3}", elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds);
            _timerPrompt.SetText($"<color=#{ColorUtility.ToHtmlStringRGB(Constants.OW_ORANGE_COLOR)}>{elapsedStr}</color>");
        }

        private void OnWakeUp()
        {
            Locator.GetPlayerBody().SetVelocity(_spawnPoint.GetPointVelocity());
        }

        private void OnStartOfTimeLoop(int loopCount)
        {
            if (SpeedrunState.INSTANCE.ModEnabled)
            {
                SpeedrunState.INSTANCE.JustStartedTimeLoop = true;
                CreateTimer();
            }
        }

        private void ResetSpawnNames()
        {
            SpeedrunState.INSTANCE.SpawnPointId = null;
            SpeedrunState.INSTANCE.GoalPointId = null;
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
            if (SpeedrunState.INSTANCE.SpawnPointId == null || SpeedrunState.INSTANCE.GoalPointId == null)
            {
                ModHelper.Console.WriteLine("Warps were null");
                SpeedrunState.INSTANCE.SpawnPointId = GetRandomSpawnPointName();
                SpeedrunState.INSTANCE.GoalPointId = GetRandomSpawnPointName();
            }

            _spawnPoint = GetSpawnPointByName(spawnPoints, SpeedrunState.INSTANCE.SpawnPointId);
            _goalPoint = GetSpawnPointByName(spawnPoints, SpeedrunState.INSTANCE.GoalPointId);
            ModHelper.Console.WriteLine($"Warp to {_spawnPoint.ToString()}!", MessageType.Success);
            spawner.DebugWarp(_spawnPoint);
            var player = GameObject.FindGameObjectWithTag("Player");
            var playerController = player.GetComponent<PlayerSpacesuit>();
            playerController.SuitUp();
            var oxygenController = player.GetComponent<PlayerResources>();
            oxygenController.UpdateOxygen();
            if (!ModHelper.Config.GetSettingsValue<bool>("ShipSpawns"))
            {
                var ship = GameObject.FindGameObjectWithTag("Ship");
                ship.SetActive(false);
            }
            var villageMusicController = FindObjectOfType<VillageMusicVolume>();
            ModHelper.Console.WriteLine($"villageMusicController {(villageMusicController == null ? "is" : "isn\'t")} null", MessageType.Success);
            villageMusicController.Deactivate();
        }

        private Font GetFontByName(string name)
        {
            var fonts = Resources.FindObjectsOfTypeAll(typeof(Font)) as Font[];
            return fonts.First(font => font.name == name);
        }

        private void SpeedRunButton_OnClick()
        {
            if (FindObjectOfType<TitleScreenManager>()._profileManager.currentProfileGameSave.loopCount <= 1)
            {
                ModHelper.Menus.PopupManager.CreateMessagePopup("Finish the tutorial first!");
                return;
            }

            _manager = SpawnPointSelectorManager.Instance;
            _manager.SpawnPointConfigs = _spawnPointPool.SpawnPointConfigs as List<SpawnPointConfig>;
            _manager.ModHelper = ModHelper;
            var titleStreaming = ModHelper.Menus.MainMenu.ResumeExpeditionButton.Button.GetComponent<SubmitActionLoadScene>()._titleScreenStreaming;
            var loadingText = _speedrunButton.Button.GetComponentInChildren<Text>();
            _manager.ConfigureSubmitAction(titleStreaming, loadingText);
            _manager.DisplayMenu();
        }

        private void ResetRunButton_OnClick()
        {
            SpeedrunState.INSTANCE.JustEnteredGame = true;
            Locator.GetDeathManager().KillPlayer(DeathType.Meditation);
            ModHelper.Menus.PauseMenu.Close();
        }

        protected SpawnPoint[] GetSpawnPoints(PlayerSpawner spawner)
        {
            var spawnPointsField = typeof(PlayerSpawner)
                .GetField("_spawnList", BindingFlags.NonPublic | BindingFlags.Instance);
            var spawnPoints = spawnPointsField?.GetValue(spawner) as SpawnPoint[];
            spawnPoints = spawnPoints.OrderBy(x => x.name).ToArray();

            ModHelper.Console.WriteLine($"Registered {spawnPoints.Length} spawn points", MessageType.Info);

            return spawnPoints;
        }

        protected void InitMapMarker()
        {
            var labelText = $"GOAL: {SpeedrunState.INSTANCE.GoalPointName.ToUpper()}";
            var markerManager = Locator.GetMarkerManager();
            _canvasMarker = markerManager.InstantiateNewMarker();
            markerManager.RegisterMarker(_canvasMarker, _goalPoint.transform, labelText);
            _canvasMarker._mainTextField.color = Constants.OW_ORANGE_COLOR;
            _canvasMarker._marker.material.color = Constants.OW_ORANGE_COLOR;
            _canvasMarker._offScreenIndicator._textField.color = Constants.OW_ORANGE_COLOR;
            _canvasMarker._offScreenIndicator._arrow.GetComponentInChildren<MeshRenderer>().material.color = Constants.OW_ORANGE_COLOR;
            _canvasMarker.SetVisibility(true);

            var mapMarkerManager = Locator.GetMapController().GetMarkerManager();
            var mapMarker = mapMarkerManager.InstantiateNewMarker(true);
            mapMarkerManager.RegisterMarker(mapMarker, _goalPoint.transform, UITextType.None);
            mapMarker.SetLabel(labelText);
            var materialInstance = Instantiate(mapMarker._textField.material);
            materialInstance.color = Constants.OW_ORANGE_COLOR;
            mapMarker._textField.material = materialInstance;
            mapMarker.SetColor(Constants.OW_ORANGE_COLOR);
            mapMarker.SetVisibility(true);
        }

        protected PlayerSpawner GetSpawner()
        {
            ModHelper.Console.WriteLine($"initialize spawner.", MessageType.Info);
            return GameObject.FindGameObjectWithTag("Player").GetRequiredComponent<PlayerSpawner>();
        }

        protected string GetRandomSpawnPointName() =>
            _spawnPointPool.RandomSpawnPointConfig(_random).internalId;

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
                SpeedrunState.INSTANCE.EndTime = DateTime.Now;
                marshmallow.gameObject.SetActive(false);
                _canvasMarker.gameObject.SetActive(false);
            };

            go.transform.parent = parent;
            go.transform.localPosition = new Vector3(0, 0, 0);
        }
    }
}