using SpawnPointSelector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using OWML.Common;
using OWML.ModHelper;

namespace OuterWildsRandomSpeedrun
{
  public class SpawnPointMenuOption : MenuOption
  {
    public IModHelper ModHelper;
    private static Color OW_ORANGE_COLOR = new Color(0.968f, 0.498f, 0.207f);
    private static Color OW_SELECTED_COLOR = new Color(0.9882f, 0.8627f, 0.7686f);
    public override void OnSelect(BaseEventData eventData)
    {

      var listItem = this.gameObject.GetComponent<SpawnPointListItem>();
      listItem.LeftArrow.SetActive(true);
      listItem.RightArrow.SetActive(true);
      listItem.Text.color = OW_SELECTED_COLOR;
  
      var list = listItem.transform.parent.parent.GetComponentInParent<SpawnPointList>();
      list.SetContentPosition(eventData.selectedObject);
      var menu = listItem.transform.parent.parent.GetComponentInParent<SpawnPointMenu>();
      menu.SetSelectOnActivate(GetComponent<Selectable>());
    }

    private void SetListPosition(SpawnPointList list, GameObject selectedObj)
    {
      var listTransform = list.GetComponent<RectTransform>();
      var listSpacing = list.ContentTransform.GetComponent<VerticalLayoutGroup>().spacing;
      var selectedObjHeight = selectedObj.GetComponent<RectTransform>().sizeDelta.y;
      var selectedObjIndex = selectedObj.transform.GetSiblingIndex();
      var listContentTransform = list.ContentTransform.GetComponent<RectTransform>();
      var newPosition = selectedObjHeight / 2 + selectedObjIndex * (selectedObjHeight + listSpacing);
      listContentTransform.anchoredPosition = new Vector2(listContentTransform.anchoredPosition.x, newPosition);
    }

    public override void OnDeselect(BaseEventData eventData) {
      var listItem = this.gameObject.GetComponent<SpawnPointListItem>();
      listItem.LeftArrow.SetActive(false);
      listItem.RightArrow.SetActive(false);
      listItem.Text.color = OW_ORANGE_COLOR;
    }
  }
}