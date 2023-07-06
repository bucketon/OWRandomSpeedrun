using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using System.IO;
using System.Reflection;
using System.Linq;
using OWML.Common.Menus;
using System;
using HarmonyLib;
using System.Collections.Generic;

namespace TeamOptimism
{
    public class NomaiGrandPrix : ModBehaviour
    {
        private const string RESUME_BUTTON_NAME = "Button-ResumeGame";

        private SpawnPoint _spawnPoint;
        private SpawnPoint _goalPoint;
        private IModButton _speedrunButton;
        private IModButton _resetRunButton;
        private ScreenPrompt _timerPrompt;
        private ScreenPrompt _spawnGoalPrompt;
        private Mesh _marshmallowMesh;
        private Material _marshmallowMaterial;
        private CanvasMarker _canvasMarker;

        private SpawnPointSelectorManager _manager;

        private System.Random _random;

        private SpawnPointPool _spawnPointPool;

        private AssetBundle _confettiBundle;

        private Func<SpawnPointConfig, bool> _spawnFilter;
        private Func<SpawnPointConfig, bool> _goalFilter;

        private static Dictionary<string, string> OuterWarpPathsBySpawnPointID = new Dictionary<string, string>
        {
            ["SpawnPoint_ElsinoreSeed"] = "DB_Elsinore_Body/Sector_ElsinoreDimension/Interactables_ElsinoreDimension/OuterWarp_Elsinore",
            ["SpawnPoint_EscapePod"] = "DB_EscapePodDimension_Body/Sector_EscapePodDimension/Interactables_EscapePodDimension/OuterWarp_EscapePod",
            ["SpawnPoint_PioneerCamp"] = "DB_PioneerDimension_Body/Sector_PioneerDimension/Interactables_PioneerDimension/OuterWarp_Pioneer",
            ["SpawnPoint_NomaiGrave"] = "DB_EscapePodDimension_Body/Sector_EscapePodDimension/Interactables_EscapePodDimension/OuterWarp_EscapePod",
            ["Spawn_Vessel"] = "DB_VesselDimension_Body/Sector_VesselDimension/Interactables_VesselDimension/OuterWarp_Vessel"
        };

        // Allows method matches to access the ModHelper
        public static NomaiGrandPrix Instance;

        public SpeedrunState SpeedrunState { get; set; }

        public Func<SpawnPointConfig, bool> SpawnFilter
        {
            get { return _spawnFilter; }
        }
        public Func<SpawnPointConfig, bool> GoalFilter
        {
            get { return _goalFilter; }
        }

        private void Awake()
        {
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.

            Instance = this;
            SpeedrunState = new SpeedrunState();
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        private void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"Mod {nameof(NomaiGrandPrix)} is loaded!", MessageType.Success);
            var hasDlc = EntitlementsManager.IsDlcOwned() == EntitlementsManager.AsyncOwnershipStatus.Owned;

            //unfortunately we need to initialize these in Start since we don't have access to ModHelper earlier.
            _spawnFilter = config =>
            {
                return config.shouldSpawn && (config.area == Area.None || ModHelper.Config.GetSettingsValue<bool>($"Spawn{config.area}"));
            };
            _goalFilter = config =>
            {
                return config.shouldGoal && (config.area == Area.None || ModHelper.Config.GetSettingsValue<bool>($"Goal{config.area}"));
            };

            // Initialize spawn points from TSV
            var parentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var pathToTsv = Path.Combine(parentDir, "SpawnPoints.tsv");
            _spawnPointPool = SpawnPointPool.FromTsv(
                pathToTsv,
                spawnPoint => (spawnPoint.area & (Area.Stranger | Area.DreamZone)) == 0 || hasDlc
            );
            ModHelper.Console.WriteLine($"Loaded {_spawnPointPool.SpawnPointConfigs.Count} spawn points", MessageType.Debug);

            _random = new System.Random((int)DateTime.Now.Ticks);

            GlobalMessenger<int>.AddListener("StartOfTimeLoop", new Callback<int>(this.OnStartOfTimeLoop));

            // The mod breaks without this for reasons unknown
            ModHelper.HarmonyHelper.EmptyMethod<DebugInputManager>("Awake");

            ModHelper.Menus.MainMenu.OnInit += () =>
            {
                _speedrunButton = ModHelper.Menus.MainMenu.ResumeExpeditionButton.Duplicate(Constants.SPEEDRUN_BUTTON_TEXT);
                _speedrunButton.OnClick += SpeedRunButton_OnClick;
            };

            ModHelper.Menus.PauseMenu.OnInit += () =>
            {
                if (SpeedrunState.ModEnabled)
                {
                    _resetRunButton = ModHelper.Menus.PauseMenu.QuitButton.Duplicate(Constants.RESET_RUN_BUTTON_TEXT);
                    _resetRunButton.OnClick += ResetRunButton_OnClick;
                }
            };
        }

        private void Update()
        {
            if (!SpeedrunState.IsGameStarted || !SpeedrunState.ModEnabled)
            {
                return;
            }

            if (SpeedrunState.JustStartedTimeLoop)
            {
                SpeedrunState.JustStartedTimeLoop = false;
                HandleNewLoopSetup();
                InitMapMarker();
                SpawnGoal(_goalPoint.transform);
                var spawnActions = SpawnActionFactory.GetActionsForSpawn(SpeedrunState.SpawnPoint?.internalId);
                spawnActions.Do(action => action.Invoke());

                if (SpeedrunState.JustEnteredGame)
                {
                    SpeedrunState.JustEnteredGame = false;
                    SpeedrunState.StartTime = DateTime.Now;
                    SpeedrunState.EndTime = DateTime.MinValue;
                }
            }

            UpdateTimer();
        }

        public override void Configure(IModConfig config)
        {
            SpawnPointSelectorManager.Instance.Refresh();
        }

        private void OnStartOfTimeLoop(int loopCount)
        {
            if (SpeedrunState.ModEnabled)
            {
                SpeedrunState.JustStartedTimeLoop = true;
                CreateTimer();
                var spawner = GetSpawner();
                var spawnPoints = GetSpawnPoints(spawner);
                HandleBasicWarp(spawner, spawnPoints);
            }
        }

        private void CreateTimer()
        {
            var screenPromptListObj = GameObject.Find("ScreenPromptListBottomLeft");
            var screenPromptList = screenPromptListObj.GetComponent<ScreenPromptList>();

            var font = GetFontByName(Constants.OW_MENU_FONT_NAME);

            _timerPrompt = new ScreenPrompt("");
            var timerScreenPromptElementObj = ScreenPromptElement.CreateNewScreenPrompt(
                _timerPrompt,
                20,
                font,
                screenPromptListObj.transform,
                TextAnchor.LowerLeft
            );

            _spawnGoalPrompt = new ScreenPrompt("");
            var spawnGoalScreenPromptElementObj = ScreenPromptElement.CreateNewScreenPrompt(
                _spawnGoalPrompt,
                12,
                font,
                screenPromptListObj.transform,
                TextAnchor.LowerLeft
            );

            var spawnGoalScreenPromptElement = spawnGoalScreenPromptElementObj.GetComponent<ScreenPromptElement>();
            var timerScreenPromptElement = timerScreenPromptElementObj.GetComponent<ScreenPromptElement>();

            screenPromptList.AddScreenPrompt(spawnGoalScreenPromptElement);
            screenPromptList.AddScreenPrompt(timerScreenPromptElement);
        }

        private void UpdateTimer()
        {
            (var elapsed, var color) =
                SpeedrunState.IsComplete()
                    ? (SpeedrunState.EndTime - SpeedrunState.StartTime, Constants.OW_SELECTED_COLOR)
                    : (DateTime.Now - SpeedrunState.StartTime, Constants.OW_ORANGE_COLOR);

            var colorStr = ColorUtility.ToHtmlStringRGB(color);

            var pathDescription = $"{SpeedrunState.SpawnPoint?.displayName} - {SpeedrunState.GoalPoint?.displayName}";
            _spawnGoalPrompt.SetText($"<color=#{colorStr}>{pathDescription}</color>");

            var elapsedStr = string.Format("{0:D2}:{1:D2}.{2:D3}", elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds);
            _timerPrompt.SetText($"<color=#{colorStr}>{elapsedStr}</color>");
        }

        private void HandleBasicWarp(PlayerSpawner spawner, SpawnPoint[] spawnPoints)
        {
            if (!SpeedrunState.SpawnPoint.HasValue || !SpeedrunState.GoalPoint.HasValue)
            {
                throw new InvalidOperationException("Spawn point or goal point was null when attempting to warp");
            }

            _spawnPoint = GetSpawnPointByName(spawnPoints, SpeedrunState.SpawnPoint?.internalId);
            _goalPoint = GetSpawnPointByName(spawnPoints, SpeedrunState.GoalPoint?.internalId);
            ModHelper.Console.WriteLine($"Warp to {_spawnPoint.ToString()}!", MessageType.Success);
            spawner.SetInitialSpawnPoint(_spawnPoint);
            Locator.GetPlayerBody().gameObject.AddComponent<MatchInitialMotion>();
            spawner.SpawnPlayer();
        }

        private void HandleNewLoopSetup()
        {
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

            if (!SpeedrunState.SpawnPoint.HasValue)
            {
                ModHelper.Console.WriteLine(
                    "Spawn point was null when attempting to determine if village music should be deactivated",
                    MessageType.Warning
                );
            }
            if (!(bool)SpeedrunState.SpawnPoint?.isThVillage)
            {
                var villageMusicController = FindObjectOfType<VillageMusicVolume>();
                villageMusicController.Deactivate();
            }
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
            var titleStreaming = ModHelper.Menus.MainMenu.ResumeExpeditionButton.Button
                .GetComponent<SubmitActionLoadScene>()
                ._titleScreenStreaming;
            _manager.TitleStreaming = titleStreaming;
            _manager.DisplayMenu();
        }

        private void ResetRunButton_OnClick()
        {
            SpeedrunState.SpawnPoint = GetRandomSpawnConfig(SpawnFilter);
            SpeedrunState.GoalPoint = GetRandomSpawnConfig(GoalFilter);

            SpeedrunState.JustEnteredGame = true;
            Locator.GetDeathManager().KillPlayer(DeathType.Meditation);
            ModHelper.Menus.PauseMenu.Close();
        }

        private SpawnPoint[] GetSpawnPoints(PlayerSpawner spawner)
        {
            spawner.FindPlanetSpawns();
            var spawnPointsField = typeof(PlayerSpawner).GetField("_spawnList", BindingFlags.NonPublic | BindingFlags.Instance);
            var spawnPoints = spawnPointsField?.GetValue(spawner) as SpawnPoint[];
            spawnPoints = spawnPoints.OrderBy(x => x.name).ToArray();

            ModHelper.Console.WriteLine($"Registered {spawnPoints.Length} spawn points", MessageType.Info);

            return spawnPoints;
        }

        private void InitMapMarker()
        {
            if (!SpeedrunState.SpawnPoint.HasValue || !SpeedrunState.GoalPoint.HasValue)
            {
                ModHelper.Console.WriteLine("Goal point was null when attempting to create goal marker", MessageType.Warning);
            }

            var labelText = $"GOAL: {SpeedrunState.GoalPoint?.displayName.ToUpper()}";
            var markerManager = Locator.GetMarkerManager();

            OuterFogWarpVolume outerWarp = GameObject.Find(OuterWarpPathsBySpawnPointID.GetValueOrDefault(SpeedrunState.GoalPoint.Value.internalId, ""))?
                .GetComponent<OuterFogWarpVolume>();

            _canvasMarker = markerManager.InstantiateNewMarker();
            markerManager.RegisterMarker(_canvasMarker, _goalPoint.transform, labelText);
            _canvasMarker._mainTextField.color = Constants.OW_ORANGE_COLOR;
            _canvasMarker._marker.material.color = Constants.OW_ORANGE_COLOR;
            _canvasMarker._offScreenIndicator._textField.color = Constants.OW_ORANGE_COLOR;
            _canvasMarker._offScreenIndicator._arrow.GetComponentInChildren<MeshRenderer>().material.color = Constants.OW_ORANGE_COLOR;
            if (outerWarp != null)
            {
                _canvasMarker.SetOuterFogWarpVolume(outerWarp);
            }
            _canvasMarker.SetVisibility(true);
            

            var mapMarkerManager = Locator.GetMapController().GetMarkerManager();
            var mapMarker = mapMarkerManager.InstantiateNewMarker(true);
            mapMarkerManager.RegisterMarker(mapMarker, _goalPoint.transform, UITextType.None);
            mapMarker.SetLabel(labelText);
            var materialInstance = Instantiate(mapMarker._textField.material);
            materialInstance.color = Constants.OW_ORANGE_COLOR;
            mapMarker._textField.material = materialInstance;
            mapMarker.SetColor(Constants.OW_ORANGE_COLOR);
            if (outerWarp != null)
            {
                mapMarker.SetOuterFogWarpVolume(outerWarp);
            }
            mapMarker.SetVisibility(true);
        }

        private PlayerSpawner GetSpawner()
        {
            ModHelper.Console.WriteLine($"initialize spawner.", MessageType.Info);
            return GameObject.FindGameObjectWithTag("Player").GetRequiredComponent<PlayerSpawner>();
        }

        private SpawnPointConfig GetRandomSpawnConfig(Func<SpawnPointConfig, bool> filter = null) =>
            _spawnPointPool.RandomSpawnPointConfig(_random, filter);

        private SpawnPoint GetSpawnPointByName(SpawnPoint[] spawnPoints, string name)
        {
            return spawnPoints
                .Where(point =>
                {
                    return point.name.Equals(name);
                })
                .First();
        }

        private void SpawnGoal(Transform parent)
        {
            var go = new GameObject("GoalPoint");
            var collider = go.AddComponent<SphereCollider>();
            collider.isTrigger = true;

            var mesh = new GameObject("CollectibleMarshmellow_Mesh");
            var marshmallowGameObject = GameObject.Find(
                "Player_Body/RoastingSystem/Stick_Root/Stick_Pivot/Stick_Tip/Mallow_Root/Props_HEA_Marshmallow"
            );
            if (_marshmallowMesh == null)
            {
                _marshmallowMesh = marshmallowGameObject.GetComponent<MeshFilter>().mesh;
            }
            if (_marshmallowMaterial == null)
            {
                _marshmallowMaterial = marshmallowGameObject.GetComponent<MeshRenderer>().material;
            }

            mesh.AddComponent<MeshFilter>().mesh = _marshmallowMesh;
            mesh.AddComponent<MeshRenderer>().material = _marshmallowMaterial;
            mesh.transform.parent = go.transform;
            mesh.transform.localScale = Vector3.one * 10f;
            mesh.transform.localEulerAngles = new Vector3(0f, 0f, 0f);

            Marshmallow marshmallow = go.AddComponent<Marshmallow>();
            marshmallow.OnCollected += () =>
            {
                ModHelper.Console.WriteLine($"VICTORY!!!!", MessageType.Info);
                SpeedrunState.EndTime = DateTime.Now;
                marshmallow.gameObject.SetActive(false);
                _canvasMarker.gameObject.SetActive(false);
                Locator.GetPlayerAudioController().PlayMarshmallowEat();

                _confettiBundle = _confettiBundle == null ? ModHelper.Assets.LoadBundle("assets/confetti") : _confettiBundle;
                var confettiPrefab = _confettiBundle.LoadAsset<GameObject>("assets/confetticontainer.prefab");
                Instantiate(confettiPrefab, Locator.GetPlayerCamera()._mainCamera.transform);
            };

            go.transform.parent = parent;
            go.transform.localPosition = new Vector3(0, 0, 0);
        }
    }
}
