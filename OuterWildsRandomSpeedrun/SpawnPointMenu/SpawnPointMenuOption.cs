using SpawnPointSelector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using OWML.Common;

namespace OuterWildsRandomSpeedrun
{
  public class SpawnPointMenuOption : MenuOption, ISubmitHandler, ICancelHandler, IMoveHandler
  {
    public IModHelper ModHelper;
    public override void OnSelect(BaseEventData eventData)
    {

      var listItem = this.gameObject.GetComponent<SpawnPointListItem>();
      listItem.LeftArrow.SetActive(true);
      listItem.RightArrow.SetActive(true);
      listItem.Text.color = Constants.OW_SELECTED_COLOR;
  
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
      listItem.Text.color = Constants.OW_ORANGE_COLOR;
    }

    public void OnSubmit(BaseEventData eventData)
    {
      SpawnPointSelectorManager.Instance.OnConfirmPressed(eventData);
    }

    public void OnCancel(BaseEventData eventData)
    {
      SpawnPointSelectorManager.Instance.OnCancelPressed(eventData);
    }

    public void OnMove(AxisEventData eventData)
    {
      if (eventData.moveDir == MoveDirection.Left || eventData.moveDir == MoveDirection.Right)
      {
        SpawnPointSelectorManager.Instance.OnLeftRightPressed(eventData);
      }
  }
  }
}