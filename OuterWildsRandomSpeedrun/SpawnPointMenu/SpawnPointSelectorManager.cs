using OWML.Common;
using SpawnPointSelector;
using UnityEngine;
using System;
using System.Collections.Generic;
using static UnityEngine.InputSystem.InputAction;

namespace OuterWildsRandomSpeedrun
{
  public class SpawnPointSelectorManager : MonoBehaviour
  {
    public IModHelper ModHelper { get; set; }

    public static SpawnPointSelectorManager Instance {
      get => _instance;
    }

    private bool _menuDisplayed = false;

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

    /// <summary>
    /// The "To" menu, represented as a script associated with our prefab
    /// </summary>
    private SpawnPointList _toList;

    private static SpawnPointSelectorManager _instance;

    public void Awake()
    {
      _instance = this;
    }

    public void DisplayMenu()
    {
      _menuDisplayed = true;

      InitializeSelector();
      InitializeMenus();

      _selector.gameObject.SetActive(true);
      
      var action = (InputLibrary.menuLeft as InputCommands).Action as BasicInputAction;
      action.Action.performed += OnLeftRightPressed;
      action = (InputLibrary.menuRight as InputCommands).Action as BasicInputAction;
      action.Action.performed += OnLeftRightPressed;
      action = (InputLibrary.enter as InputCommands).Action as BasicInputAction;
      action.Action.performed += OnConfirmPressed;
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
      var spawnNames = new string[] {
        "Ash Twin",
        "Brittle Hollow",
        "Dark Bramble",
        "Ember Twin",
        "Giant's Deep",
        "Feldspar's Camp",
        "Sun Station",
        "Timber Hearth",
        "White Hole Station",
        "Bingus Station",
        "Pingus Station",
        "Dingus Station"
      };

      foreach (string spawnName in spawnNames)
      {
        addMenuItem(spawnName, _fromList, fromMenuOptions);
        addMenuItem(spawnName, _toList, toMenuOptions);
      }

      _fromMenu._menuOptions = fromMenuOptions.ToArray();
      _toMenu._menuOptions = toMenuOptions.ToArray();

      InitializeMenu(_fromMenu, _fromList);
      InitializeMenu(_toMenu, _toList);

      _fromMenu.EnableMenu(true);
      _toMenu.EnableMenu(false);
    }

    public void DisableMenu()
    {
      _menuDisplayed = false;
      _fromMenu.EnableMenu(false);
      _toMenu.EnableMenu(false);

      _selector.gameObject.SetActive(false);

      var action = (InputLibrary.menuLeft as InputCommands).Action as BasicInputAction;
      action.Action.performed -= OnLeftRightPressed;
      action = (InputLibrary.menuRight as InputCommands).Action as BasicInputAction;
      action.Action.performed -= OnLeftRightPressed;
      action = (InputLibrary.enter as InputCommands).Action as BasicInputAction;
      action.Action.performed -= OnConfirmPressed;

      Destroy(_fromMenu);
      Destroy(_toMenu);

      _fromMenu = null;
      _toMenu = null;
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

    private void InitializeMenu(SpawnPointMenu menu, SpawnPointList list)
    {
      // Don't reset the initially-selected item every time we activate the menu
      menu._setMenuNavigationOnActivate = false;

      list.Initialize();
      SetInitialSelection(menu, list);
    }

    private void SetInitialSelection(SpawnPointMenu menu, SpawnPointList list){
      var optionCount = menu._menuOptions.Length;
      var middleOption = menu._menuOptions[(int) Math.Ceiling(optionCount / 2f) - 1];
      var selectable = middleOption._selectable;
      list.SetContentPosition(selectable.gameObject);
      menu.SetSelectOnActivate(selectable);
    }

    private void addMenuItem(string spawnName, SpawnPointList list, List<MenuOption> options)
    {
        var listItem = list.AddItem(spawnName);
        var menuOption = listItem.gameObject.AddComponent<SpawnPointMenuOption>();
        listItem.gameObject.AddComponent<SelectableAudioPlayer>();
        menuOption.Initialize();
        menuOption.ModHelper = ModHelper;
        options.Add(menuOption);
    }

    private void OnLeftRightPressed (CallbackContext context) {
      var disableMenu = _fromMenu.IsMenuEnabled() ? _fromMenu : _toMenu;
      var enableMenu = disableMenu == _fromMenu ? _toMenu : _fromMenu;

      disableMenu.EnableMenu(false);
      enableMenu.EnableMenu(true);
    }

    private void OnConfirmPressed(CallbackContext context)
    {
      var from = _fromMenu._lastSelected.GetComponent<SpawnPointListItem>().Text.text;
      var to = _toMenu._lastSelected.GetComponent<SpawnPointListItem>().Text.text;
      ModHelper.Console.WriteLine($"Oh my, we've been confirmed with {from} and {to}");
      DisableMenu();
    }
  }
}