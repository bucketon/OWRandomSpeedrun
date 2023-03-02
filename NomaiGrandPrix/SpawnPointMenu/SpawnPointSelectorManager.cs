using OWML.Common;
using SpawnPointSelector;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace NomaiGrandPrix
{
    public class SpawnPointSelectorManager : MonoBehaviour
    {
        public IModHelper ModHelper { get; set; }

        public static SpawnPointSelectorManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _goInstance = new GameObject("SpawnPointSelectorManager");
                    _instance = _goInstance.AddComponent<SpawnPointSelectorManager>();
                }
                return _instance;
            }
        }

        public List<SpawnPointConfig> SpawnPointConfigs
        {
            set
            {
                if (_spawnPointConfigs != null)
                {
                    return;
                }

                _spawnPointConfigs = value;
                _spawnPointConfigs.Sort((spawn1, spawn2) => spawn1.displayName.CompareTo(spawn2.displayName));
            }
        }

        public TitleScreenStreaming TitleStreaming { get; set; }

        private List<SpawnPointConfig> _spawnPointConfigs;

        /// <summary>
        /// The GameObject that hosts the SpawnPointSelectorManager singleton
        /// </summary>
        private static GameObject _goInstance;

        /// <summary>
        /// Singleton instance of SpawnPointSelectorManager
        /// </summary>
        private static SpawnPointSelectorManager _instance;

        /// <summary>
        /// The top-level script associated with the spawn selector UI
        /// </summary>
        private SpawnPointSelector.SpawnPointSelector _selector;

        /// <summary>
        /// The "From" menu, represented in Outer Wild's format
        /// </summary>
        private SpawnPointMenu _fromMenu;

        /// <summary>
        /// The "To" menu, represented in Outer Wild's format
        /// </summary>
        private SpawnPointMenu _toMenu;

        /// <summary>
        /// The "From" menu, represented as a script associated with our prefab
        /// </summary>
        private SpawnPointList _fromList;

        public SpawnPointList FromList
        {
            get => _fromList;
        }

        /// <summary>
        /// The "To" menu, represented as a script associated with our prefab
        /// </summary>
        private SpawnPointList _toList;

        private SubmitActionLoadScene _submitAction;

        private System.Random _random = new System.Random((int)DateTime.Now.Ticks);

        private Dictionary<Area, SpawnPointPlanet> _areaToPlanetDict = new Dictionary<Area, SpawnPointPlanet>()
        {
            { Area.None, SpawnPointPlanet.None },
            { Area.SunStation, SpawnPointPlanet.SunStation },
            { Area.AshTwin, SpawnPointPlanet.AshTwin },
            { Area.EmberTwin, SpawnPointPlanet.EmberTwin },
            { Area.TimberHearth, SpawnPointPlanet.TimberHearth },
            { Area.BrittleHollow, SpawnPointPlanet.BrittleHollow },
            { Area.GiantsDeep, SpawnPointPlanet.GiantsDeep },
            { Area.DarkBramble, SpawnPointPlanet.DarkBramble },
            { Area.Interloper, SpawnPointPlanet.Interloper },
            { Area.WhiteHole, SpawnPointPlanet.WhiteHole },
            { Area.QuantumMoon, SpawnPointPlanet.QuantumMoon },
            { Area.Stranger, SpawnPointPlanet.Stranger },
            { Area.DreamZone, SpawnPointPlanet.DreamZone }
        };

        public void Update()
        {
            if (_selector.gameObject.activeSelf && OWInput.IsNewlyPressed(InputLibrary.setDefaults, InputMode.All))
            {
                RandomizeSelections();
            }
        }

        public void DisplayMenu()
        {
            InitializeSelector();
            InitializeMenus();
            InitializeSubmitAction();

            _fromMenu.EnableMenu(true);
            _toMenu.EnableMenu(false);

            UpdateTooltipIcons();
            UpdateTooltipText();

            _selector.gameObject.SetActive(true);

            OWInput.inputManagerInstance.OnUpdateInputDevice += this.OnUpdateInputDevice;
        }

        public void DisableMenu()
        {
            _fromMenu.EnableMenu(false);
            _toMenu.EnableMenu(false);

            _selector.gameObject.SetActive(false);

            OWInput.inputManagerInstance.OnUpdateInputDevice -= this.OnUpdateInputDevice;
        }

        private void OnDestroy()
        {
            OWInput.inputManagerInstance.OnUpdateInputDevice -= this.OnUpdateInputDevice;
        }

        public void InitializeSubmitAction()
        {
            if (_submitAction != null)
            {
                return;
            }
            if (_selector == null)
            {
                ModHelper.Console.WriteLine(
                    "SpawnPointSelector needs to be initialized before calling InitializeSubmitAction!",
                    MessageType.Error
                );
                return;
            }

            _submitAction = _goInstance.AddComponent<SubmitActionLoadScene>();
            _submitAction.SetSceneToLoad(SubmitActionLoadScene.LoadableScenes.GAME);
            _submitAction.EnableConfirm(false);
            _submitAction._titleScreenStreaming = TitleStreaming;
            _submitAction._loadingText = _selector.CourseSelectText;
        }

        public void RandomizeSelections()
        {
            var selectedMenu = _fromMenu.IsMenuEnabled() ? _fromMenu : _toMenu;
            var unselectedMenu = selectedMenu == _fromMenu ? _toMenu : _fromMenu;
            var unselectedList = selectedMenu == _fromMenu ? _toList : _fromList;

            var selectedMenuSelectable = GetRandomSelectable(selectedMenu);
            var unselectedMenuSelectable = GetRandomSelectable(unselectedMenu);
            selectedMenuSelectable.Select();
            unselectedMenu.SetSelectOnActivate(unselectedMenuSelectable);
            unselectedList.SetContentPosition(unselectedMenuSelectable.gameObject);
        }

        public void OnLeftRightPressed(AxisEventData eventData)
        {
            SwapMenus();
        }

        public void OnConfirmPressed(BaseEventData eventData)
        {
            if (_fromMenu.IsMenuEnabled())
            {
                SwapMenus();
                return;
            }

            var from = _fromMenu._lastSelected.GetComponent<SpawnPointMenuOption>().SpawnPoint;
            var to = _toMenu._lastSelected.GetComponent<SpawnPointMenuOption>().SpawnPoint;

            var speedrunState = NomaiGrandPrix.Instance.SpeedrunState;
            speedrunState.SpawnPoint = from;
            speedrunState.GoalPoint = to;
            speedrunState.ModEnabled = true;
            speedrunState.JustEnteredGame = true;

            ModHelper.Console.WriteLine($"Starting game with spawn points: {from.displayName} -> {to.displayName}");
            _submitAction.Submit();
        }

        public void OnCancelPressed(BaseEventData eventData)
        {
            DisableMenu();
        }

        public void OnItemClicked(PointerEventData eventData, SpawnPointMenuOption option)
        {
            var eventMenu = GetMenuForMenuOption(option);

            if (!eventMenu.IsMenuEnabled())
            {
                SwapMenus();
            }
        }

        private void OnUpdateInputDevice()
        {
            UpdateTooltipIcons();
        }

        private SpawnPointMenu GetMenuForMenuOption(SpawnPointMenuOption option)
        {
            var menu = option.transform.parent.parent.parent == _fromList.transform ? _fromMenu : _toMenu;
            return menu;
        }

        private void InitializeMenus()
        {
            if (_fromMenu != null)
            {
                return;
            }

            _fromMenu = _fromList.gameObject.AddComponent<SpawnPointMenu>();
            _toMenu = _toList.gameObject.AddComponent<SpawnPointMenu>();

            var fromMenuOptions = new List<MenuOption>();
            var toMenuOptions = new List<MenuOption>();

            foreach (SpawnPointConfig spawnConfig in _spawnPointConfigs)
            {
                //TODO: get the menus to refresh when settings are changed.
                if (spawnConfig.shouldSpawn && (spawnConfig.area == Area.None || ModHelper.Config.GetSettingsValue<bool>($"Spawn{NomaiGrandPrix.areaNameMap[spawnConfig.area]}")))
                {
                    addMenuItem(spawnConfig, _fromList, fromMenuOptions);
                }
                if (spawnConfig.shouldGoal && (spawnConfig.area == Area.None || ModHelper.Config.GetSettingsValue<bool>($"Goal{NomaiGrandPrix.areaNameMap[spawnConfig.area]}")))
                {
                    addMenuItem(spawnConfig, _toList, toMenuOptions);
                }
            }

            _fromMenu._menuOptions = fromMenuOptions.ToArray();
            _toMenu._menuOptions = toMenuOptions.ToArray();

            InitializeMenu(_fromMenu, _fromList);
            InitializeMenu(_toMenu, _toList);
        }

        private void InitializeSelector()
        {
            if (_selector != null)
            {
                return;
            }

            // Create top-level lists/menus
            var titleMenu = GameObject.Find("TitleMenu");
            var spawnPointBundle = ModHelper.Assets.LoadBundle("assets/spawnpointselector");
            var spawnPointSelectorPrefab = spawnPointBundle.LoadAsset<GameObject>("assets/spawnpointselector.prefab");
            var spawnPointListItemPrefab = spawnPointBundle.LoadAsset<GameObject>("assets/spawnpointlistitem.prefab");
            var spawnPointSelectorGO = Instantiate(spawnPointSelectorPrefab, titleMenu.transform);
            _selector = spawnPointSelectorGO.GetComponent<SpawnPointSelector.SpawnPointSelector>();
            _fromList = _selector.FromSpawnPointList;
            _toList = _selector.ToSpawnPointList;

            _fromList.SpawnPointListItemPrefab = spawnPointListItemPrefab;
            _toList.SpawnPointListItemPrefab = spawnPointListItemPrefab;

            spawnPointBundle.Unload(false);
        }

        private void SwapMenus()
        {
            var disableMenu = _fromMenu.IsMenuEnabled() ? _fromMenu : _toMenu;
            var enableMenu = disableMenu == _fromMenu ? _toMenu : _fromMenu;

            disableMenu.EnableMenu(false);
            enableMenu.EnableMenu(true);
            UpdateTooltipText();
        }

        private void UpdateTooltipIcons()
        {
            var leftSprite = GetSpriteForInput(InputLibrary.menuLeft);
            var rightSprite = GetSpriteForInput(InputLibrary.menuRight);
            var confirmSprite = GetSpriteForInput(InputLibrary.menuConfirm);
            var cancelSprite = GetSpriteForInput(InputLibrary.cancel);
            var randomizeSprite = GetSpriteForInput(InputLibrary.setDefaults);

            _selector.Tooltip.RightImage.sprite = rightSprite;
            _selector.Tooltip.ConfirmImage.sprite = confirmSprite;
            _selector.Tooltip.CancelImage.sprite = cancelSprite;
            _selector.Tooltip.RandomizeImage.sprite = randomizeSprite;

            if (OWInput.UsingGamepad())
            {
                _selector.Tooltip.LeftImage.enabled = false;
                _selector.Tooltip.SeparatorText.enabled = false;
            }
            else
            {
                _selector.Tooltip.LeftImage.enabled = true;
                _selector.Tooltip.LeftImage.sprite = leftSprite;
                _selector.Tooltip.SeparatorText.enabled = true;
            }
        }

        private void UpdateTooltipText()
        {
            var text = "Next";
            if (_toMenu.IsMenuEnabled())
            {
                text = "Start";
            }
            _selector.Tooltip.NextText.text = text;
        }

        private Sprite GetSpriteForInput(IInputCommands input)
        {
            var texture = input.GetUITextures(OWInput.UsingGamepad(), false)[0];
            var rect = new Rect(0, 0, texture.width, texture.height);
            var pivot = new Vector2(0.5f, 0.5f);
            return Sprite.Create(texture, rect, pivot, texture.width);
        }

        private void InitializeMenu(SpawnPointMenu menu, SpawnPointList list)
        {
            // Don't reset the initially-selected item every time we activate the menu
            menu._setMenuNavigationOnActivate = false;

            list.Initialize();
            SetInitialSelection(menu, list);
        }

        private void SetInitialSelection(SpawnPointMenu menu, SpawnPointList list)
        {
            var speedrunState = NomaiGrandPrix.Instance.SpeedrunState;
            var currentSpawn = menu == _fromMenu ? speedrunState.SpawnPoint : speedrunState.GoalPoint;
            var selectable = currentSpawn.HasValue ? FindSelectableForSpawn(menu, currentSpawn) : GetRandomSelectable(menu);
            list.SetContentPosition(selectable.gameObject);
            menu.SetSelectOnActivate(selectable);
        }

        private Selectable GetRandomSelectable(SpawnPointMenu menu)
        {
            var randomIndex = _random.Next(_fromMenu._menuOptions.Length);
            return menu._menuOptions[randomIndex]._selectable;
        }

        private Selectable FindSelectableForSpawn(SpawnPointMenu menu, SpawnPointConfig? currentSpawn)
        {
            var foundMenuOption = Array.Find<MenuOption>(
                menu._menuOptions,
                menuOption => ((SpawnPointMenuOption)menuOption).SpawnPoint.internalId == currentSpawn?.internalId
            );
            return foundMenuOption._selectable;
        }

        private void AddMenuItem(SpawnPointConfig spawnConfig, SpawnPointList list, List<MenuOption> options)
        {
            var listItem = list.AddItem($"{options.Count + 1}. {spawnConfig.displayName}", _areaToPlanetDict[spawnConfig.area]);
            listItem.gameObject.AddComponent<SelectableAudioPlayer>();

            var menuOption = listItem.gameObject.AddComponent<SpawnPointMenuOption>();
            menuOption.SpawnPoint = spawnConfig;
            menuOption.Initialize();
            menuOption.ModHelper = ModHelper;
            options.Add(menuOption);
        }
    }
}
