using SpawnPointSelector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NomaiGrandPrix
{
    public class SpawnPointMenuOption : MenuOption, ISubmitHandler, ICancelHandler, IMoveHandler, IPointerClickHandler
    {
        public SpawnPointConfig SpawnPoint { get; set; }

        private Coroutine _selectionCoroutine;

        public override void OnSelect(BaseEventData eventData)
        {
            var listItem = this.gameObject.GetComponent<SpawnPointListItem>();
            listItem.LeftArrow.SetActive(true);
            listItem.RightArrow.SetActive(true);
            listItem.Text.color = Constants.OW_SELECTED_COLOR;

            var list = listItem.transform.parent.parent.GetComponentInParent<SpawnPointList>();

            if (_selectionCoroutine != null)
            {
                StopCoroutine(_selectionCoroutine);
            }

            _selectionCoroutine = StartCoroutine(list.MoveContentToPosition(eventData.selectedObject));

            var menu = listItem.transform.parent.parent.GetComponentInParent<SpawnPointMenu>();
            menu.SetSelectOnActivate(GetComponent<Selectable>());
        }

        public override void OnDeselect(BaseEventData eventData)
        {
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

        public void OnPointerClick(PointerEventData eventData)
        {
            SpawnPointSelectorManager.Instance.OnItemClicked(eventData, this);
        }
    }
}
