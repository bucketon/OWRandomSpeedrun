using OWML.Common;
using SpawnPointSelector;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace OuterWildsRandomSpeedrun
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
    public SpawnPointList FromList {
      get => _fromList;
    }
  /// <summary>
  /// The "To" menu, represented as a script associated with our prefab
  /// </summary>
  private SpawnPointList _toList;

    public void DisplayMenu()
    {
      InitializeSelector();
      InitializeMenus();

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

    public void OnLeftRightPressed (AxisEventData eventData) {
      SwapMenus();
    }

    public void OnConfirmPressed(BaseEventData eventData)
    {
      if (_fromMenu.IsMenuEnabled())
      {
        SwapMenus();
        return;
      }

      var from = _fromMenu._lastSelected.GetComponent<SpawnPointListItem>().Text.text;
      var to = _toMenu._lastSelected.GetComponent<SpawnPointListItem>().Text.text;
      ModHelper.Console.WriteLine($"Oh my, we've been confirmed with {from} and {to}");
      DisableMenu();
    }

    public void OnCancelPressed(BaseEventData eventData)
    {
      DisableMenu();
    }

    private void OnUpdateInputDevice()
    {
      UpdateTooltipIcons();
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

      _selector.Tooltip.RightImage.sprite = rightSprite;
      _selector.Tooltip.ConfirmImage.sprite = confirmSprite;
      _selector.Tooltip.CancelImage.sprite = cancelSprite;

      if (OWInput.UsingGamepad()) {
        _selector.Tooltip.LeftImage.enabled = false;
        _selector.Tooltip.SeparatorText.enabled = false;
      } else {
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

    private void SetInitialSelection(SpawnPointMenu menu, SpawnPointList list){
      var optionCount = menu._menuOptions.Length;
      var middleOption = menu._menuOptions[(int) Math.Ceiling(optionCount / 2f) - 1];
      var selectable = middleOption._selectable;
      list.SetContentPosition(selectable.gameObject);
      list.SetItemAlphas(middleOption.transform.GetSiblingIndex());
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
  }
}